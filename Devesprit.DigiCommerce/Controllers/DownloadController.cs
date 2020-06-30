using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Models.Download;
using Devesprit.Services.Currency;
using Devesprit.Services.FileManagerServiceReference;
using Devesprit.Services.FileServers;
using Devesprit.Services.Localization;
using Devesprit.Services.Products;
using Devesprit.Utilities.Extensions;
using Microsoft.AspNet.Identity;

namespace Devesprit.DigiCommerce.Controllers
{
    public partial class DownloadController : BaseController
    {
        private readonly IProductService _productService;
        private readonly IFileServersService _fileServersService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductCheckoutAttributesService _productCheckoutAttributesService;
        private readonly IProductDiscountsForUserGroupsService _productDiscountsForUserGroupsService;
        private readonly IProductDownloadsLogService _downloadsLogService;

        public DownloadController(
            IProductService productService,
            IFileServersService fileServersService,
            ILocalizationService localizationService,
            IProductCheckoutAttributesService productCheckoutAttributesService,
            IProductDiscountsForUserGroupsService productDiscountsForUserGroupsService,
            IProductDownloadsLogService downloadsLogService)
        {
            _productService = productService;
            _fileServersService = fileServersService;
            _localizationService = localizationService;
            _productCheckoutAttributesService = productCheckoutAttributesService;
            _productDiscountsForUserGroupsService = productDiscountsForUserGroupsService;
            _downloadsLogService = downloadsLogService;
        }

        // GET: Download
        [Route("{lang}/Download/{productId}/{demoFiles}", Order = 0)]
        [Route("Download/{productId}/{demoFiles}", Order = 1)]
        public virtual async Task<ActionResult> DownloadProduct(int productId, bool? demoFiles)
        {
            var product = await _productService.FindByIdAsync(productId);
            var user = await UserManager.FindByIdAsync(HttpContext.User.Identity.GetUserId());
            var requestDemoFiles = demoFiles ?? false;

            if (product == null || (!product.Published && !HttpContext.User.IsInRole("Admin")))
                return View("PageNotFound"); // product id is invalid or not published

            var model = new DownloadModel
            {
                PageTitle = product.GetLocalized(p => p.Title),
                ProductPageUrl = Url.Action("Index", "Product", new { id = product.Id, slug = product.Slug }),
                IsDemo = requestDemoFiles
            };

            var userHasAccessToFiles = _productService.UserCanDownloadProduct(product, user, requestDemoFiles);
            var canDownload =
                userHasAccessToFiles.HasFlagFast(UserCanDownloadProductResult.UserCanDownloadProduct);

            if (!canDownload &&
                (!string.IsNullOrWhiteSpace(product.FilesPath) ||
                product.CheckoutAttributes.Any(p =>
                    p.Options.Any(x => !string.IsNullOrWhiteSpace(x.FilesPath)))))
            {
                model.UserHasAccessToFiles = userHasAccessToFiles;
                model.DiscountsForUserGroups = GenerateUserGroupDiscountsDescription(product, user);
            }
            //---------------------------

            //User must upgrade subscription
            if (userHasAccessToFiles.HasFlagFast(UserCanDownloadProductResult.UserMustSubscribeToAPlan) ||
               userHasAccessToFiles.HasFlagFast(UserCanDownloadProductResult.UserMustSubscribeToAPlanOrHigher))
            {
                model.UserGroupName = product.DownloadLimitedToUserGroup.GetLocalized(p => p.GroupName);
            }

            //User download limit has been reached
            if (userHasAccessToFiles.HasFlagFast(UserCanDownloadProductResult.UserDownloadLimitReached) ||
                userHasAccessToFiles.HasFlagFast(UserCanDownloadProductResult.UserGroupDownloadLimitReached))
            {
                TimePeriodType? periodType = null;
                //User number of download limitation
                if (userHasAccessToFiles.HasFlagFast(UserCanDownloadProductResult.UserDownloadLimitReached))
                    if (user.MaxDownloadCount > 0 && user.MaxDownloadPeriodType != null)
                    {
                        model.DownloadLimit = user.MaxDownloadCount.Value;
                        periodType = user.MaxDownloadPeriodType.Value;
                    }

                //User group number of download limitation
                if (userHasAccessToFiles.HasFlagFast(UserCanDownloadProductResult.UserGroupDownloadLimitReached))
                    if (user.UserGroup?.MaxDownloadCount > 0 && user.UserGroup?.MaxDownloadPeriodType != null)
                    {
                        model.DownloadLimit = user.UserGroup.MaxDownloadCount.Value;
                        periodType = user.UserGroup.MaxDownloadPeriodType.Value;
                    }

                var date = DateTime.Now.AddTimePeriodToDateTime(periodType, -1);
                var userLatestDownloadDate = _downloadsLogService.GetAsQueryable().Where(p =>
                                                     p.UserId == user.Id &&
                                                     p.DownloadDate >= date && p.ProductId != product.Id &&
                                                     !p.IsDemoVersion)
                                                 .OrderByDescending(p => p.DownloadDate).FirstOrDefault()
                                                 ?.DownloadDate
                                             ??
                                             DateTime.Now;

                model.DownloadLimitResetDate = userLatestDownloadDate.AddTimePeriodToDateTime(periodType, 1) ?? DateTime.MaxValue;

                switch (periodType)
                {
                    case TimePeriodType.Hour:
                        model.DownloadLimitPer = _localizationService.GetResource("Hour");
                        break;
                    case TimePeriodType.Day:
                        model.DownloadLimitPer = _localizationService.GetResource("Day");
                        break;
                    case TimePeriodType.Month:
                        model.DownloadLimitPer = _localizationService.GetResource("Month");
                        break;
                    case TimePeriodType.Year:
                        model.DownloadLimitPer = _localizationService.GetResource("Year");
                        break;
                    default:
                        model.DownloadLimitPer = "";
                        break;
                }
            }
            //---------------------------

            var productTitle = product.GetLocalized(p => p.Title);
            var filesPath = requestDemoFiles ? product.DemoFilesPath : product.FilesPath;
            if (!string.IsNullOrWhiteSpace(filesPath))
            {
                if (product.FileServerId != null)
                {
                    //Load files from server
                    var entriesList = await Task.Run(() =>
                    {
                        var fileServer = _fileServersService.GetWebService(product.FileServer);
                        return fileServer.EnumerateDirectoryEntries(filesPath,
                            CurrentSettings.DownloadableFilesExtensions, true, true,
                            TimeSpan.FromHours(CurrentSettings.DownloadUrlsAgeByHour),
                            CurrentSettings.NumberOfDownloadUrlsFireLimit);
                    }).ConfigureAwait(false);

                    model.FileGroups.Add(new FileGroup()
                    {
                        Title = productTitle,
                        FileListTree = entriesList.Length > 0
                            ? GenerateFileTreeHtml(entriesList.ToList(), canDownload, productId, requestDemoFiles)
                            : ""
                    });
                }
                else
                {
                    //File(s) is in outside of server
                    var downloadLink = canDownload
                        ? Url.Action("DownloadLog", new { productId = productId, downloadLink = filesPath, version = requestDemoFiles ? ("DEMO" + productId).EncryptString() : ("FULL" + productId).EncryptString() })
                        : $"#' onclick='WarningAlert(\"{_localizationService.GetResource("Note")}\", \"{_localizationService.GetResource("YouDoNotHaveAccessRightsToThisFile")}\")";
                    model.FileGroups.Add(new FileGroup()
                    {
                        Title = productTitle,
                        FileListTree = $"<ul><li data-jstree='{{\"icon\":\"/Content/img/FileExtIcons/download.png\"}}'><a target='_blank' rel='noindex, nofollow' href='{downloadLink}'><img src='/Content/img/FileExtIcons/link.png'/> <span class='{(productTitle.IsRtlLanguage() ? "rtl-dir" : "ltr-dir")}'>{productTitle}</span></a></li></ul>"
                    });
                }
            }
            else
            {
                model.FileGroups.Add(new FileGroup()
                {
                    Title = productTitle,
                    FileListTree = ""
                });
            }

            if (!requestDemoFiles)
            {
                //Get product attributes files
                var userPurchasedAttributes = await _productService.GetUserDownloadableAttributesAsync(product, user);
                var productCheckoutAttributes =
                    await _productCheckoutAttributesService.FindProductAttributesAsync(productId);
                foreach (var attribute in productCheckoutAttributes.Where(p => p.Options.Any(x => !string.IsNullOrWhiteSpace(x.FilesPath))).OrderBy(p => p.DisplayOrder))
                {
                    var attributeName = attribute.GetLocalized(p => p.Name);
                    var fileListTreeHtml = "";

                    foreach (var attributeOption in attribute.Options.Where(p => !string.IsNullOrWhiteSpace(p.FilesPath)).OrderBy(p => p.DisplayOrder))
                    {
                        var optionName = attributeOption.GetLocalized(p => p.Name);
                        var showDownloadLink = userPurchasedAttributes.Any(p => p.Id == attributeOption.Id);
                        {
                            fileListTreeHtml +=
                                $"<li data-jstree='{{\"icon\":\"/Content/img/FileExtIcons/dir.png\"}}'>{optionName}<ul>";
                            if (!string.IsNullOrWhiteSpace(attributeOption.FilesPath))
                            {
                                if (attributeOption.FileServerId != null)
                                {
                                    //Load files from server
                                    var entriesList = await Task.Run(() =>
                                    {
                                        var fileServer = _fileServersService.GetWebService(attributeOption.FileServer);
                                        return fileServer.EnumerateDirectoryEntries(attributeOption.FilesPath,
                                            CurrentSettings.DownloadableFilesExtensions, true, true,
                                            TimeSpan.FromHours(CurrentSettings.DownloadUrlsAgeByHour),
                                            CurrentSettings.NumberOfDownloadUrlsFireLimit);
                                    }).ConfigureAwait(false);

                                    fileListTreeHtml += entriesList.Length > 0
                                        ? GenerateFileTreeHtml(entriesList.ToList(), showDownloadLink, productId, requestDemoFiles).TrimStart("<ul>").TrimEnd("</ul>")
                                        : "";
                                }
                                else
                                {
                                    //File(s) is in outside of server
                                    var downloadLink = showDownloadLink ?
                                        Url.Action("DownloadLog", new { productId = productId, downloadLink = attributeOption.FilesPath, version = requestDemoFiles ? ("DEMO" + productId).EncryptString() : ("FULL" + productId).EncryptString() })
                                        : $"#' onclick='WarningAlert(\"{_localizationService.GetResource("Note")}\", \"{_localizationService.GetResource("YouDoNotHaveAccessRightsToThisFile")}\")";
                                    fileListTreeHtml +=
                                        $"<li data-jstree='{{\"icon\":\"/Content/img/FileExtIcons/download.png\"}}'><a target='_blank' rel='noindex, nofollow' href='{downloadLink}'><img src='/Content/img/FileExtIcons/link.png'/> <span class='{(optionName.IsRtlLanguage() ? "rtl-dir" : "ltr-dir")}'>{optionName}</span></a></li>";
                                }
                            }

                            fileListTreeHtml += "</li></ul>";
                        }
                    }

                    model.FileGroups.Add(new FileGroup()
                    {
                        Title = attributeName,
                        FileListTree = string.IsNullOrWhiteSpace(fileListTreeHtml) ? "" : $"<ul>{fileListTreeHtml}</ul>"
                    });
                }
            }

            return View(model);
        }

        protected virtual string GenerateFileTreeHtml(List<FileSystemEntries> entries, bool includeDownloadLink, int productId, bool isDemo)
        {
            var result = "<ul>";

            foreach (var dir in entries.Where(p => p.Type == FileSystemEntryType.Dir).OrderByDescending(p => p.ModifiedDateUtc))
            {
                result += $"<li data-jstree='{{\"icon\":\"/Content/img/FileExtIcons/dir.png\"}}'><span class='{(dir.Name.IsRtlLanguage() ? "rtl-dir" : "ltr-dir")}'>{dir.Name.Replace("_", " ")}     <small class='text-muted'>({dir.DisplaySize} - {dir.ModifiedDateUtc:G})</small></span> {GenerateFileTreeHtml(dir.SubEntries.ToList(), includeDownloadLink, productId, isDemo)}</li>";
            }

            foreach (var file in entries.Where(p => p.Type == FileSystemEntryType.File).OrderByDescending(p => p.ModifiedDateUtc))
            {
                var downloadLink = includeDownloadLink
                    ? Url.Action("DownloadLog", "Download", new { productId = productId, downloadLink = file.DownloadLink, version = isDemo ? ("DEMO" + productId).EncryptString() : ("FULL" + productId).EncryptString() })
                    : $"#' onclick='WarningAlert(\"{_localizationService.GetResource("Note")}\", \"{_localizationService.GetResource("YouDoNotHaveAccessRightsToThisFile")}\")";
                result += $"<li data-jstree='{{\"icon\":\"/Content/img/FileExtIcons/download.png\"}}'><a target='_blank' rel='noindex, nofollow' href='{downloadLink}'><img src='{GetFileImage(file)}'/><span class='{(file.Name.IsRtlLanguage() ? "rtl -dir" : "ltr-dir")}'>{file.Name.Replace("_", " ")}     <small class='text-muted'>({_localizationService.GetResource("Size")}: {file.DisplaySize} - {_localizationService.GetResource("Date")}: {file.ModifiedDateUtc:G})</small></span></a></li>";
            }
            result += "</ul>";
            return result;
        }

        protected virtual string GenerateUserGroupDiscountsDescription(TblProducts product, TblUsers user)
        {
            string result = "";

            var fromGroup = user?.UserGroupId != null ? user.UserGroup.GroupPriority : int.MinValue;
            var discounts = _productDiscountsForUserGroupsService.FindProductDiscounts(product.Id);

            foreach (var discount in discounts.Where(p => p.UserGroup.GroupPriority >= fromGroup))
            {
                var price = product.Price - (discount.DiscountPercent * product.Price) / 100;
                var groupName = discount.UserGroup.GetLocalized(p => p.GroupName);

                if (price <= 0)
                {
                    //Free
                    if (discount.ApplyDiscountToHigherUserGroups)
                    {
                        result += string.Format(
                                      _localizationService.GetResource("FreeForUserGroupsOrHigher"),
                                      groupName,
                                      discount.UserGroup.Id,
                                      discount.UserGroup.GroupPriority,
                                      product.Id,
                                      product.Slug) + "<br/>";
                    }
                    else
                    {
                        result += string.Format(
                                      _localizationService.GetResource("FreeForUserGroups"),
                                      groupName,
                                      discount.UserGroup.Id,
                                      discount.UserGroup.GroupPriority,
                                      product.Id,
                                      product.Slug) + "<br/>";
                    }
                }
                else
                {
                    if (discount.ApplyDiscountToHigherUserGroups)
                    {
                        result += string.Format(
                                      _localizationService.GetResource("DiscountForUserGroupsOrHigher"),
                                      discount.DiscountPercent,
                                      groupName,
                                      price.ExchangeCurrencyStr(),
                                      discount.UserGroup.Id,
                                      discount.UserGroup.GroupPriority,
                                      product.Id,
                                      product.Slug) + "<br/>";
                    }
                    else
                    {
                        result += string.Format(
                                      _localizationService.GetResource("DiscountForUserGroups"),
                                      discount.DiscountPercent,
                                      groupName,
                                      price.ExchangeCurrencyStr(),
                                      discount.UserGroup.Id,
                                      discount.UserGroup.GroupPriority,
                                      product.Id,
                                      product.Slug) + "<br/>";
                    }
                }
            }

            return result;
        }

        protected virtual string GetFileImage(FileSystemEntries file)
        {
            switch (Path.GetExtension(file.Name)?.ToLower().Replace(".", ""))
            {
                case "tar":
                case "7z":
                case "7zip":
                case "rar":
                case "zip":
                    return "/Content/img/FileExtIcons/zip.png";
                case "exe":
                case "dmg":
                case "apk":
                    return "/Content/img/FileExtIcons/exe.png";
                case "msi":
                    return "/Content/img/FileExtIcons/msi.png";
                case "iso":
                    return "/Content/img/FileExtIcons/iso.png";
                case "txt":
                    return "/Content/img/FileExtIcons/txt.png";
                case "pdf":
                    return "/Content/img/FileExtIcons/pdf.png";
                case "docx":
                case "doc":
                    return "/Content/img/FileExtIcons/doc.png";
                case "png":
                case "jpg":
                case "jpeg":
                case "gif":
                case "ico":
                    return "/Content/img/FileExtIcons/img.png";
                case "avi":
                case "wmv":
                case "mp4":
                    return "/Content/img/FileExtIcons/movie.png";
            }
            return "/Content/img/FileExtIcons/other.png";
        }

        public virtual async Task<ActionResult> DownloadLog(int productId, string downloadLink, string version)
        {
            await _downloadsLogService.AddAsync(new TblProductDownloadsLog()
            {
                DownloadDate = DateTime.Now,
                ProductId = productId,
                DownloadLink = downloadLink,
                UserId = HttpContext.User.Identity.GetUserId(),
                UserIp = HttpContext.GetClientIpAddress(),
                IsDemoVersion = version.DecryptString() == "DEMO" + productId
            });

            await _productService.IncreaseNumberOfDownloadsAsync(await _productService.FindByIdAsync(productId));

            return Redirect(downloadLink);
        }
    }
}