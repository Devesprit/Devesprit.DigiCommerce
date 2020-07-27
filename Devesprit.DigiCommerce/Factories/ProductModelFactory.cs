using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Factories.Interfaces;
using Devesprit.DigiCommerce.Models.Post;
using Devesprit.DigiCommerce.Models.Products;
using Devesprit.Services.Currency;
using Devesprit.Services.Localization;
using Devesprit.Services.Products;
using Devesprit.Services.Users;
using Devesprit.Utilities;
using Devesprit.Utilities.Extensions;
using Mapster;
using X.PagedList;

namespace Devesprit.DigiCommerce.Factories
{
    public partial class ProductModelFactory : IProductModelFactory
    {
        private readonly IProductService _productService;
        private readonly IUserLikesService _userLikesService;
        private readonly IUserWishlistService _userWishlistService;
        private readonly IUsersService _usersService;
        private readonly IUserGroupsService _userGroupsService;
        private readonly IProductDiscountsForUserGroupsService _productDiscountsForUserGroupsService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductCheckoutAttributesService _checkoutAttributesService;
        private readonly HttpContextBase _httpContext;

        public ProductModelFactory(IProductService productService,
            IUserLikesService userLikesService,
            IUserWishlistService userWishlistService,
            IUsersService usersService,
            IUserGroupsService userGroupsService,
            IProductDiscountsForUserGroupsService productDiscountsForUserGroupsService,
            ILocalizationService localizationService,
            IProductCheckoutAttributesService checkoutAttributesService,
            HttpContextBase httpContext)
        {
            _productService = productService;
            _userLikesService = userLikesService;
            _userWishlistService = userWishlistService;
            _usersService = usersService;
            _userGroupsService = userGroupsService;
            _productDiscountsForUserGroupsService = productDiscountsForUserGroupsService;
            _localizationService = localizationService;
            _checkoutAttributesService = checkoutAttributesService;
            _httpContext = httpContext;
        }

        public virtual IPagedList<ProductCardViewModel> PrepareProductCardViewModel(IPagedList<TblProducts> products, TblUsers currentUser, UrlHelper url)
        {
            var userAddedThisPostsToWishlist = _userWishlistService.UserAddedThisPostToWishlist(products.Select(p => p.Id).ToArray(), currentUser?.Id);
            var userLikedThisPosts = _userLikesService.UserLikedThisPost(products.Select(p => p.Id).ToArray(), currentUser?.Id);
            var userGroups = _userGroupsService.GetAsEnumerable().ToList();

            var result = new List<ProductCardViewModel>();
            foreach (var product in products)
            {
                var model = product.Adapt<ProductCardViewModel>();

                model.LastUpDate = product.LastUpDate ?? product.PublishDate;
                model.MainImageUrl = product.Images?.OrderBy(p => p.DisplayOrder).FirstOrDefault()
                                          ?.ImageUrl ?? "";
                model.Categories = product.Categories
                    .Select(p => new PostCategoriesModel()
                    {
                        Id = p.Id,
                        CategoryName = p.GetLocalized(x => x.CategoryName),
                        Slug = p.Slug,
                        CategoryUrl = url.Action("FilterByCategory", "Product", new { slug = p.Slug, page = 1 })
                    })
                    .ToList();
                var desc = product.Descriptions?.OrderBy(p => p.DisplayOrder).FirstOrDefault()?.GetLocalized(x => x.HtmlDescription) ?? "";
                model.DescriptionTruncated = desc.ConvertHtmlToText().TruncateText(350);

                model.PostUrl = new Uri(url.Action("Index", "Product", new { id = product.Id, slug = product.Slug }, _httpContext.Request.Url.Scheme)).ToString();

                var likeWishlistButtonsModel = new LikeWishlistButtonsModel()
                {
                    PostId = model.Id
                };

                if (userAddedThisPostsToWishlist.ContainsKey(model.Id))
                    likeWishlistButtonsModel.AlreadyAddedToWishlist = userAddedThisPostsToWishlist[model.Id];
                if (userLikedThisPosts.ContainsKey(model.Id))
                    likeWishlistButtonsModel.AlreadyLiked = userLikedThisPosts[model.Id];

                model.LikeWishlistButtonsModel = likeWishlistButtonsModel;

                model.DownloadModel = new ProductCardDownloadModel
                {
                    ProductId = product.Id,
                    DownloadLimitedToUserGroup = product.DownloadLimitedToUserGroupId != null ? userGroups.FirstOrDefault(p => p.Id == product.DownloadLimitedToUserGroupId.Value) : null,
                    ShowPurchaseBtn = product.Price > 0 || !string.IsNullOrWhiteSpace(product.LicenseGeneratorServiceId),
                    HigherUserGroupsCanDownload = product.HigherUserGroupsCanDownload,
                    PriceForCurrentUser = product.Price
                };

                result.Add(model);
            }

            return new StaticPagedList<ProductCardViewModel>(result, products.PageNumber,
                products.PageSize, products.TotalItemCount);
        }

        public virtual ProductModel PrepareProductModel(TblProducts product, TblUsers currentUser, UrlHelper url)
        {
            var result = product.Adapt<TblProducts, ProductModel>();
            result.Title = product.GetLocalized(p => p.Title);
            result.PageTitle = product.GetLocalized(p => p.PageTitle);
            result.MetaDescription = product.GetLocalized(p => p.MetaDescription);
            result.MetaKeyWords = product.GetLocalized(p => p.MetaKeyWords);
            result.LastUpdate = product.LastUpDate ?? product.PublishDate;
            result.Categories = product.Categories
                .Select(p => new PostCategoriesModel()
                {
                    Id = p.Id,
                    CategoryName = p.GetLocalized(x => x.CategoryName),
                    Slug = p.Slug,
                    CategoryUrl = url.Action("FilterByCategory", "Product", new { slug = p.Slug, page = 1 })
                })
                .ToList();
            result.TagsList = product.Tags
                .Select(p => new Tuple<int, string>(p.Id, p.GetLocalized(x => x.Tag)))
                .ToList();

            //user purchased current product ?
            var purchasedProducts = _usersService.GetUserPurchasedProducts(currentUser?.Id, product.Id);
            if (purchasedProducts.Any(p => p.PurchaseExpiration > DateTime.Now))
            {
                result.CurrentUserHasAlreadyPurchasedThisProduct = true;
            }

            result.PriceForCurrentUser = _productService.CalculateProductPriceForUser(product, currentUser);
            result.CurrentUserGroup = currentUser?.UserGroup;

            result.LikeWishlistButtonsModel = new LikeWishlistButtonsModel()
            {
                PostId = product.Id,
                AlreadyAddedToWishlist = _userWishlistService.UserAddedThisPostToWishlist(product.Id, currentUser?.Id),
                AlreadyLiked = _userLikesService.UserLikedThisPost(product.Id, currentUser?.Id)
            };

            result.DownloadModel = PrepareProductDownloadPurchaseButtonModel(product, currentUser);
            
            result.AlternativeSlugs.Clear();
            foreach (var slug in product.AlternativeSlugs)
            {
                result.AlternativeSlugs.Add(new PostSlugsModel()
                {
                    Slug = slug.Slug
                });
            }

            result.Images.Clear();
            foreach (var img in product.Images.OrderBy(p => p.DisplayOrder))
            {
                result.Images.Add(new PostImagesModel()
                {
                    Title = img.GetLocalized(p => p.Title) ?? result.PageTitle,
                    Alt = img.GetLocalized(p => p.Alt) ?? result.Title,
                    ImageUrl = img.GetLocalized(p => p.ImageUrl),
                    DisplayOrder = img.DisplayOrder
                });
            }

            result.Descriptions.Clear();
            foreach (var desc in product.Descriptions.OrderBy(p => p.DisplayOrder))
            {
                var description = desc.GetLocalized(x => x.HtmlDescription);
                result.Descriptions.Add(new PostDescriptionsModel()
                {
                    Description = description,
                    Title = desc.GetLocalized(x => x.Title),
                    IsRtl = description.StripHtml().IsRtlLanguage()
                });
            }

            result.Attributes.Clear();
            foreach (var attr in product.Attributes)
            {
                result.Attributes.Add(new PostAttributesModel()
                {
                    Type = attr.PostAttribute.AttributeType,
                    Name = attr.PostAttribute.GetLocalized(p => p.Name),
                    Value = attr.PostAttribute.AttributeType == PostAttributeType.Option
                        ? attr.AttributeOption.GetLocalized(p => p.Name)
                        : attr.GetLocalized(p => p.Value),
                    DisplayOrder = attr.DisplayOrder
                });
            }

            result.CheckoutAttributes.Clear();
            foreach (var attr in product.CheckoutAttributes)
            {
                result.CheckoutAttributes.Add(new TblProductCheckoutAttributes()
                {
                    Id = attr.Id,
                    Name = attr.GetLocalized(p => p.Name),
                    Description = attr.GetLocalized(p => p.Description),
                    AttributeType = attr.AttributeType,
                    DisplayOrder = attr.DisplayOrder,
                    ProductId = attr.ProductId,
                    Required = attr.Required,
                    MaxRange = attr.MaxRange,
                    MinRange = attr.MinRange,
                    LicenseGeneratorServiceId = attr.LicenseGeneratorServiceId,
                    UnitPrice = attr.UnitPrice,
                    Options = attr.Options.Select(p=> p.Adapt<TblProductCheckoutAttributeOptions>()).ToList()
                });
            }

            var protocol = _httpContext?.Request.Url?.Scheme ?? "http";
            result.PostUrl = new Uri(url.Action("Index", "Product", new { id = product.Id, slug = product.Slug },protocol: protocol) ?? "").ToString();

            return result;
        }

        public virtual ProductDownloadModel PrepareProductDownloadPurchaseButtonModel(TblProducts product, TblUsers currentUser)
        {
            var productCheckoutAttributes = AsyncHelper
                .RunSync(() => _checkoutAttributesService.FindProductAttributesAsync(product.Id)).ToList();
            TblUserGroups downloadLimitedToUserGroupRecord = null;

            if (product.DownloadLimitedToUserGroupId != null)
            {
                downloadLimitedToUserGroupRecord = AsyncHelper
                    .RunSync(() => _userGroupsService.FindByIdAsync(product.DownloadLimitedToUserGroupId.Value));
            }

            var result = new ProductDownloadModel
            {
                ProductId = product.Id,
                AlwaysShowDownloadButton = product.AlwaysShowDownloadButton,
                DownloadLimitedToUserGroup = downloadLimitedToUserGroupRecord,
                HigherUserGroupsCanDownload = product.HigherUserGroupsCanDownload,
                HasDownloadableFile = !string.IsNullOrWhiteSpace(product.FilesPath) ||
                                      productCheckoutAttributes.Any(p =>
                                          p.Options.Any(x => !string.IsNullOrWhiteSpace(x.FilesPath))),
                PriceForCurrentUser = _productService.CalculateProductPriceForUser(product, currentUser),
                DiscountForUserGroupsDescription = GenerateUserGroupDiscountsDescription(product, currentUser),
                DownloadBlockingReason = _productService.UserCanDownloadProduct(product, currentUser, false)
            };

            result.CanDownloadByCurrentUser = result.DownloadBlockingReason.HasFlagFast(UserCanDownloadProductResult.UserCanDownloadProduct);
            result.CurrentUserGroup = currentUser?.UserGroup;
            result.HasDemoVersion = !string.IsNullOrWhiteSpace(product.DemoFilesPath);

            //user purchased current product ?
            var purchasedProducts = _usersService.GetUserPurchasedProducts(currentUser?.Id, product.Id);
            if (purchasedProducts.Any(p => p.PurchaseExpiration > DateTime.Now))
            {
                result.CurrentUserHasAlreadyPurchasedThisProduct = true;
            }


            //ShowUpgradeUserAccountBtn
            if (result.DownloadLimitedToUserGroup != null &&
                (result.DownloadBlockingReason.HasFlagFast(UserCanDownloadProductResult.UserMustSubscribeToAPlan) ||
                 result.DownloadBlockingReason.HasFlagFast(UserCanDownloadProductResult.UserMustSubscribeToAPlanOrHigher)))
            {
                if (result.CurrentUserGroup == null)
                {
                    //Current user don't subscribed to any plan
                    result.ShowUpgradeUserAccountBtn = true;
                }
                else
                {
                    if (result.DownloadBlockingReason.HasFlagFast(UserCanDownloadProductResult.UserMustSubscribeToAPlan) &&
                        result.CurrentUserGroup.Id != result.DownloadLimitedToUserGroup.Id)
                    {
                        result.ShowUpgradeUserAccountBtn = true;
                    }
                    if (result.DownloadBlockingReason.HasFlagFast(UserCanDownloadProductResult.UserMustSubscribeToAPlanOrHigher) &&
                        result.CurrentUserGroup.GroupPriority < result.DownloadLimitedToUserGroup.GroupPriority)
                    {
                        result.ShowUpgradeUserAccountBtn = true;
                    }
                }
            }

            //ShowDownloadFullVersionBtn
            if (result.HasDownloadableFile)
            {
                if (result.AlwaysShowDownloadButton ||
                    result.DownloadBlockingReason.HasFlagFast(UserCanDownloadProductResult.UserCanDownloadProduct))
                {
                    result.ShowDownloadFullVersionBtn = true;
                }
                if (result.DownloadBlockingReason.HasFlagFast(UserCanDownloadProductResult.UserMustLoggedIn) ||
                    result.DownloadBlockingReason.HasFlagFast(UserCanDownloadProductResult.UserDownloadLimitReached) ||
                    result.DownloadBlockingReason.HasFlagFast(UserCanDownloadProductResult.UserGroupDownloadLimitReached))
                {
                    if (!(result.DownloadBlockingReason.HasFlagFast(UserCanDownloadProductResult.UserMustSubscribeToAPlan) ||
                          result.DownloadBlockingReason.HasFlagFast(UserCanDownloadProductResult.UserMustSubscribeToAPlanOrHigher) ||
                          result.DownloadBlockingReason.HasFlagFast(UserCanDownloadProductResult.UserMustPurchaseTheProduct)))
                    {
                        result.ShowDownloadFullVersionBtn = true;
                    }
                }
            }


            //ShowPurchaseBtn
            result.ShowPurchaseBtn =
                result.PriceForCurrentUser > 0 || !string.IsNullOrWhiteSpace(product.LicenseGeneratorServiceId) ||
                productCheckoutAttributes.Any(p =>
                    (p.UnitPrice > 0 && p.AttributeType == ProductCheckoutAttributeType.NumberBox) ||
                    !string.IsNullOrWhiteSpace(p.LicenseGeneratorServiceId))
                || productCheckoutAttributes.SelectMany(p => p.Options).Any(p =>
                    p.Price > 0 ||
                    !string.IsNullOrWhiteSpace(p.LicenseGeneratorServiceId));

            //ShowDownloadDemoVersionBtn
            result.ShowDownloadDemoVersionBtn = result.HasDemoVersion;

            return result;
        }

        protected virtual List<Tuple<TblUserGroups, string>> GenerateUserGroupDiscountsDescription(TblProducts product, TblUsers user)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            var result = new List<Tuple<TblUserGroups, string>>();

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
                        result.Add(new Tuple<TblUserGroups, string>(discount.UserGroup, string.Format(
                            _localizationService.GetResource("FreeForUserGroupsOrHigher"),
                            groupName,
                            discount.UserGroup.Id,
                            discount.UserGroup.GroupPriority,
                            product.Id,
                            product.Slug))); 
                    }
                    else
                    {
                        result.Add(new Tuple<TblUserGroups, string>(discount.UserGroup, string.Format(
                            _localizationService.GetResource("FreeForUserGroups"),
                            groupName,
                            discount.UserGroup.Id,
                            discount.UserGroup.GroupPriority,
                            product.Id,
                            product.Slug)));
                    }
                }
                else
                {
                    if (discount.ApplyDiscountToHigherUserGroups)
                    {
                        result.Add(new Tuple<TblUserGroups, string>(discount.UserGroup, string.Format(
                            _localizationService.GetResource("DiscountForUserGroupsOrHigher"),
                            discount.DiscountPercent,
                            groupName,
                            price.ExchangeCurrencyStr(),
                            discount.UserGroup.Id,
                            discount.UserGroup.GroupPriority,
                            product.Id,
                            product.Slug)));
                    }
                    else
                    {
                        result.Add(new Tuple<TblUserGroups, string>(discount.UserGroup, string.Format(
                            _localizationService.GetResource("DiscountForUserGroups"),
                            discount.DiscountPercent,
                            groupName,
                            price.ExchangeCurrencyStr(),
                            discount.UserGroup.Id,
                            discount.UserGroup.GroupPriority,
                            product.Id,
                            product.Slug)));
                    }
                }
            }

            return result;
        }
    }
}