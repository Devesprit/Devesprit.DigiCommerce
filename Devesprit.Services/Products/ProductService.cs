using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.Currency;
using Devesprit.Services.Events;
using Devesprit.Services.Localization;
using Devesprit.Services.Posts;
using Devesprit.Services.Users;
using Devesprit.Utilities;
using Devesprit.Utilities.Extensions;
using Microsoft.AspNet.Identity;
using X.PagedList;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Products
{
    public partial class ProductService : PostService<TblProducts>, IProductService
    {
        private readonly AppDbContext _dbContext;
        private readonly IProductDownloadsLogService _productDownloadsLogService;
        private readonly IUserGroupsService _userGroupsService;
        private readonly IPostCategoriesService _categoriesService;
        private readonly IProductDiscountsForUserGroupsService _productDiscountsForUserGroupsService;
        private readonly IProductCheckoutAttributesService _productCheckoutAttributesService;
        private readonly ILocalizationService _localizationService;
        private readonly IUsersService _usersService;
        private readonly string _cacheKey;

        public ProductService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IProductDownloadsLogService productDownloadsLogService,
            IUserLikesService userLikesService,
            IUserWishlistService userWishlistService,
            IUserGroupsService userGroupsService,
            IPostCategoriesService categoriesService,
            IProductDiscountsForUserGroupsService productDiscountsForUserGroupsService,
            IProductCheckoutAttributesService productCheckoutAttributesService,
            ILocalizationService localizationService,
            IUsersService usersService,
            IEventPublisher eventPublisher) : base(dbContext,
            localizedEntityService,
            userLikesService,
            userWishlistService,
            categoriesService,
            eventPublisher)
        {
            _dbContext = dbContext;
            _productDownloadsLogService = productDownloadsLogService;
            _userGroupsService = userGroupsService;
            _categoriesService = categoriesService;
            _productDiscountsForUserGroupsService = productDiscountsForUserGroupsService;
            _productCheckoutAttributesService = productCheckoutAttributesService;
            _localizationService = localizationService;
            _usersService = usersService;

            _cacheKey = nameof(TblProducts);
        }

        public virtual IPagedList<TblProducts> GetBestSelling(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null)
        {
            IQueryable<TblInvoiceDetails> invoiceQuery;
            if (fromDate != null)
            {
                invoiceQuery = _dbContext.Invoices.Where(p => p.CreateDate > fromDate).SelectMany(p => p.InvoiceDetails)
                    .Where(p => p.ItemType == InvoiceDetailsItemType.Product);
            }
            else
            {
                invoiceQuery = _dbContext.InvoiceDetails.Where(p => p.ItemType == InvoiceDetailsItemType.Product);
            }

            var sortedItems = invoiceQuery
                .GroupBy(p => p.ItemId)
                .Select(p => new {p.FirstOrDefault().ItemId, Sum = p.Sum(c => c.Qty)}).OrderByDescending(p => p.Sum)
                .AsNoTracking()
                .Skip(pageSize * (pageIndex - 1))
                .Take(pageSize)
                .FromCache(CacheTags.Invoice).Select(p => p.ItemId).ToList();

            var query = _dbContext.Products.Where(p => p.Published);

            if (filterByCategory != null)
            {
                var subCategories = _categoriesService.GetSubCategories(filterByCategory.Value);
                query = query.Where(p => p.Categories.Any(x => subCategories.Contains(x.Id)));
            }

            var products = query
                .Where(p => sortedItems.Contains(p.Id))
                .Include(p => p.Descriptions)
                .Include(p => p.Images)
                .Include(p => p.Categories)
                .AsNoTracking()
                .FromCache(_cacheKey,
                    CacheTags.PostCategory,
                    CacheTags.PostDescription,
                    CacheTags.PostImage);
            var result = new StaticPagedList<TblProducts>(
                products.OrderBy(p=> sortedItems.IndexOf(p.Id)),
                pageIndex,
                pageSize,
                invoiceQuery
                    .GroupBy(p => p.ItemId)
                    .Select(p => new {p.FirstOrDefault().ItemId, Sum = p.Sum(c => c.Qty)}).DeferredCount()
                    .FromCache(CacheTags.Invoice));

            return result;
        }

        public virtual IPagedList<TblProducts> GetMostDownloadedItems(int pageIndex = 1, int pageSize = Int32.MaxValue, int? filterByCategory = null,
            DateTime? fromDate = null)
        {
            var query = _dbContext.Products.Where(p => p.Published);
            if (fromDate != null)
            {
                query = _dbContext.Products.Where(p => p.PublishDate >= fromDate);
            }
             
            if (filterByCategory != null)
            {
                var subCategories = _categoriesService.GetSubCategories(filterByCategory.Value);
                query = query.Where(p => p.Categories.Any(x => subCategories.Contains(x.Id)));
            }

            var result = new StaticPagedList<TblProducts>(
                query
                    .OrderByDescending(p => p.DownloadsLog.Count)
                    .Include(p => p.Descriptions)
                    .Include(p => p.Images)
                    .Include(p => p.Categories)
                    .AsNoTracking()
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCache(_cacheKey,
                        CacheTags.PostCategory,
                        CacheTags.PostDescription,
                        CacheTags.PostImage),
                pageIndex,
                pageSize,
                query
                    .DeferredCount(p => p.Published)
                    .FromCache(_cacheKey));

            return result;
        }

        public override async Task<TblProducts> FindByIdAsync(int id)
        {
            var result = await _dbContext.Products
                .Where(p => p.Id == id)
                .Include(p => p.Categories)
                .Include(p => p.Descriptions)
                .Include(p => p.Images)
                .Include(p => p.Attributes)
                .Include(p => p.Attributes.Select(x => x.AttributeOption))
                .Include(p => p.CheckoutAttributes)
                .Include(p => p.CheckoutAttributes.Select(x=> x.Options))
                .Include(p => p.DiscountsForUserGroups)
                .Include(p => p.DownloadLimitedToUserGroup)
                .Include(p => p.Tags)
                .Include(p => p.DownloadsLog)
                .Include(p => p.FileServer)
                .DeferredFirstOrDefault()
                .FromCacheAsync(_cacheKey,
                    CacheTags.PostCategory,
                    CacheTags.PostDescription,
                    CacheTags.PostImage,
                    CacheTags.PostAttribute,
                    CacheTags.ProductCheckoutAttribute,
                    CacheTags.ProductDiscountForUserGroup,
                    CacheTags.UserGroup,
                    CacheTags.PostTag,
                    CacheTags.FileServer);
            return result;
        }

        public override async Task<TblProducts> FindBySlugAsync(string slug)
        {
            var result = await _dbContext.Products
                .Where(p => p.Slug == slug)
                .Include(p => p.Categories)
                .Include(p => p.Descriptions)
                .Include(p => p.Images)
                .Include(p => p.Attributes)
                .Include(p => p.Attributes.Select(x => x.AttributeOption))
                .Include(p => p.CheckoutAttributes)
                .Include(p => p.CheckoutAttributes.Select(x => x.Options))
                .Include(p => p.DiscountsForUserGroups)
                .Include(p => p.DownloadLimitedToUserGroup)
                .Include(p => p.Tags)
                .Include(p => p.DownloadsLog)
                .Include(p => p.FileServer)
                .DeferredFirstOrDefault()
                .FromCacheAsync(_cacheKey,
                    CacheTags.PostCategory,
                    CacheTags.PostDescription,
                    CacheTags.PostImage,
                    CacheTags.PostAttribute,
                    CacheTags.ProductCheckoutAttribute,
                    CacheTags.ProductDiscountForUserGroup,
                    CacheTags.UserGroup,
                    CacheTags.PostTag,
                    CacheTags.FileServer);
            return result;
        }

        public virtual int GetNumberOfDownloads(int productId)
        {
            return _productDownloadsLogService.GetAsQueryable()
                .DeferredCount(p => p.ProductId == productId)
                .FromCache(DateTimeOffset.Now.AddHours(24));
        }

        public virtual Dictionary<int, int> GetNumberOfDownloads(int[] productIds)
        {
            var res = _productDownloadsLogService.GetAsQueryable()
                .Where(p => productIds.Contains(p.ProductId))
                .GroupBy(p => p.ProductId)
                .Select(n => new
                    {
                        PostId = n.Key,
                        LikeCount = n.Count()
                    }
                ).FromCache(DateTimeOffset.Now.AddHours(24));

            return res.ToDictionary(p => p.PostId, p => p.LikeCount);
        }

        public virtual double CalculateProductPriceForUser(TblProducts product, TblUsers user)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            var result = product.Price;

            if (result <= 0)
            {
                return 0;
            }

            var alreadyPurchased = user?.Invoices?.Where(p => p.Status == InvoiceStatus.Paid)
                .SelectMany(p => p.InvoiceDetails).Where(p => p.ItemType == InvoiceDetailsItemType.Product &&
                                                              p.ItemId == product.Id).ToList();
            if (alreadyPurchased != null &&
                alreadyPurchased.OrderByDescending(p => p.PurchaseExpiration).Any(p =>
                    DateTime.Now <= p.PurchaseExpiration))
            {
                result = product.RenewalPrice;
            }

            if (user?.UserGroup != null &&
                user.SubscriptionExpireDate > DateTime.Now)
            {
                var discountsForUserGroup = _productDiscountsForUserGroupsService.FindProductDiscounts(product.Id)?.ToList();
                if (discountsForUserGroup != null &&
                    discountsForUserGroup.Any())
                {
                    var discount =
                        discountsForUserGroup.FirstOrDefault(p => p.UserGroupId == user.UserGroupId);
                    if (discount?.DiscountPercent > 0)
                    {
                        result = result - (result * discount.DiscountPercent) / 100;
                        if (result < 0)
                        {
                            result = 0;
                        }
                    }
                    else
                    {
                        discount =
                            discountsForUserGroup
                                .Where(p => p.ApplyDiscountToHigherUserGroups &&
                                            p.UserGroup.GroupPriority <= user.UserGroup.GroupPriority)
                                .OrderByDescending(p => p.UserGroup.GroupPriority)
                                .FirstOrDefault();
                        if (discount?.DiscountPercent > 0)
                        {
                            result = result - ((result * discount.DiscountPercent) / 100);
                            if (result < 0)
                            {
                                result = 0;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public virtual UserCanDownloadProductResult UserCanDownloadProduct(TblProducts product, TblUsers user, bool demoFiles)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            var result = UserCanDownloadProductResult.None;
            
            //Admin can download everything
            if (user != null && _usersService.UserIsAdmin(user.Id))
                result |= UserCanDownloadProductResult.UserCanDownloadProduct;

            if (demoFiles)
            {
                if (product.UserMustLoggedInToDownloadDemoFiles && user == null)
                {
                    return UserCanDownloadProductResult.UserMustLoggedIn;
                }

                //Everyone can download Demo version files
                return UserCanDownloadProductResult.UserCanDownloadProduct;
            }

            //To download product, user must registered and logged in
            if (product.UserMustLoggedInToDownloadFiles && user == null)
            {
                result |= UserCanDownloadProductResult.UserMustLoggedIn;
            }

            // Download is limited to user groups
            if (product.DownloadLimitedToUserGroupId != null)
            {
                var userGroup = AsyncHelper
                    .RunSync(() => _userGroupsService.FindByIdAsync(product.DownloadLimitedToUserGroupId.Value));
                if (product.HigherUserGroupsCanDownload)
                {
                    if (user == null ||
                        (user.SubscriptionExpireDate ?? DateTime.MinValue) < DateTime.Now ||
                        (user.UserGroup?.GroupPriority ?? int.MinValue) < userGroup.GroupPriority)
                    {
                        result |= UserCanDownloadProductResult.UserMustSubscribeToAPlanOrHigher;
                    }
                }
                else
                {
                    if (user == null ||
                        (user.SubscriptionExpireDate ?? DateTime.MinValue) < DateTime.Now ||
                        product.DownloadLimitedToUserGroupId != user.UserGroupId)
                    {
                        result |= UserCanDownloadProductResult.UserMustSubscribeToAPlan;
                    }
                }
            }

            var userNeedToPurchase = product.Price > 0;
            if (user != null)
            {
                //Discounts for user groups
                if (user.UserGroupId != null && //user has subscribed to a user group ?
                    user.SubscriptionExpireDate > DateTime.Now) //user subscription not expired ?
                {
                    var discountsForUserGroup = _productDiscountsForUserGroupsService.FindProductDiscounts(product.Id)?.ToList();
                    if (discountsForUserGroup != null &&
                        discountsForUserGroup.Any()) //product has discount for user groups ?
                    {
                        var discountForUserGroup =
                            discountsForUserGroup.FirstOrDefault(p => p.UserGroupId == user.UserGroupId);

                        if (discountForUserGroup?.DiscountPercent > 0)
                        {
                            var priceForCurrentUser =
                                product.Price - (product.Price * discountForUserGroup.DiscountPercent) / 100;

                            if (priceForCurrentUser <= 0)
                            {
                                userNeedToPurchase = false;
                            }
                        }
                        else
                        {
                            discountForUserGroup =
                                discountsForUserGroup
                                    .Where(p => p.ApplyDiscountToHigherUserGroups &&
                                                p.UserGroup.GroupPriority <= user.UserGroup.GroupPriority)
                                    .OrderByDescending(p => p.UserGroup.GroupPriority)
                                    .FirstOrDefault();
                            if (discountForUserGroup?.DiscountPercent > 0)
                            {
                                var priceForCurrentUser =
                                    product.Price - (product.Price * discountForUserGroup.DiscountPercent) / 100;

                                if (priceForCurrentUser <= 0)
                                {
                                    userNeedToPurchase = false;
                                }
                            }
                        }
                    }
                }

                //user purchased current product ?
                var purchasedProducts = _usersService.GetUserPurchasedProducts(user.Id, product.Id);
                if (purchasedProducts.Any(p => p.PurchaseExpiration > DateTime.Now))
                {
                    userNeedToPurchase = false;
                }

                //User number of download limitation
                if (user.MaxDownloadCount > 0 && user.MaxDownloadPeriodType != null)
                {
                    var date = DateTime.Now.AddTimePeriodToDateTime(user.MaxDownloadPeriodType, -1);
                    var userDownloadCount = _productDownloadsLogService.GetAsQueryable()
                        .Where(p => p.UserId == user.Id && p.DownloadDate >= date && p.ProductId != product.Id &&
                                    !p.IsDemoVersion)
                        .GroupBy(p => p.ProductId).Count();

                    if (user.MaxDownloadCount <= userDownloadCount)
                    {
                        result |= UserCanDownloadProductResult.UserDownloadLimitReached;
                    }
                }

                //User group number of download limitation
                if (user.UserGroup?.MaxDownloadCount > 0 && user.UserGroup?.MaxDownloadPeriodType != null)
                {
                    var date = DateTime.Now.AddTimePeriodToDateTime(user.UserGroup.MaxDownloadPeriodType, -1);
                    var userDownloadCount = _productDownloadsLogService.GetAsQueryable()
                        .Where(p => p.UserId == user.Id && p.DownloadDate >= date && p.ProductId != product.Id &&
                                    !p.IsDemoVersion)
                        .GroupBy(p => p.ProductId).Count();

                    if (user.UserGroup.MaxDownloadCount <= userDownloadCount)
                    {
                        result |= UserCanDownloadProductResult.UserGroupDownloadLimitReached;
                    }
                }
            }

            //Product is not free and user must purchase it
            if (userNeedToPurchase)
            {
                result |= UserCanDownloadProductResult.UserMustPurchaseTheProduct;
            }

            if (result == UserCanDownloadProductResult.None)
            {
                return UserCanDownloadProductResult.UserCanDownloadProduct;
            }

            return result;
        }
        
        public virtual async Task<List<TblProductCheckoutAttributeOptions>> GetUserDownloadableAttributesAsync(TblProducts product, TblUsers user)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            var productCheckoutAttribute =
                await _productCheckoutAttributesService.FindProductAttributesAsync(product.Id);

            var result = new List<TblProductCheckoutAttributeOptions>();

            if (!UserCanDownloadProduct(product, user, false).HasFlagFast(UserCanDownloadProductResult.UserCanDownloadProduct))
            {
                return result;
            }

            if (productCheckoutAttribute == null || !productCheckoutAttribute.Any())
            {
                return result;
            }

            if (user != null)
            {
                if (_usersService.UserManager.IsInRole(user.Id, "Admin"))
                {
                    //Admin can download everything
                    foreach (var attribute in productCheckoutAttribute)
                    {
                        foreach (var option in attribute.Options)
                        {
                            result.Add(option);
                        }
                    }

                    return result;
                }

                //Get all attributes which purchased by user
                var purchasedProductAttributes = await _usersService.GetUserPurchasedProductAttributesAsync(user.Id, product.Id);
                foreach (var purchasedAttribute in purchasedProductAttributes.OrderByDescending(p => p.PurchaseDate))
                {
                    if (purchasedAttribute.PurchaseExpiration > DateTime.Now)
                    {
                        result.Add(purchasedAttribute.Option);
                    }
                }
            }

            foreach (var attribute in productCheckoutAttribute)
            {
                foreach (var option in attribute.Options)
                {
                    // Download is limited to user groups
                    if (option.DownloadLimitedToUserGroupId != null)
                    {
                        var userGroup = AsyncHelper
                            .RunSync(() => _userGroupsService.FindByIdAsync(option.DownloadLimitedToUserGroupId.Value));
                        if (user == null ||
                            (user.SubscriptionExpireDate ?? DateTime.MinValue) < DateTime.Now)
                        {
                            continue;
                        }

                        if (option.HigherUserGroupsCanDownload)
                        {
                            if ((user.UserGroup?.GroupPriority ?? int.MinValue) < userGroup.GroupPriority)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (option.DownloadLimitedToUserGroupId != user.UserGroupId)
                            {
                                continue;
                            }
                        }
                    }

                    //Free options
                    if (option.Price <= 0)
                    {
                        result.Add(option);
                    }
                    else
                    {
                        if (user?.UserGroupId != null &&
                            user.SubscriptionExpireDate > DateTime.Now)//user subscription not expired ?
                        {
                            var discountsForUserGroup = _productDiscountsForUserGroupsService
                                .FindProductDiscounts(product.Id)?.ToList();
                            //Discounts for user groups
                            if (discountsForUserGroup != null &&
                                discountsForUserGroup.Any()) //product has discount for user groups ?
                            {
                                var discountForUserGroup = discountsForUserGroup
                                    .Where(p => p.ApplyDiscountToProductAttributes)
                                    .FirstOrDefault(p => p.UserGroupId == user.UserGroupId);

                                if (discountForUserGroup?.DiscountPercent > 0)
                                {
                                    var priceForCurrentUser =
                                        option.Price - (option.Price * discountForUserGroup.DiscountPercent) / 100;

                                    if (priceForCurrentUser <= 0)
                                    {
                                        result.Add(option);
                                    }
                                }
                                else
                                {
                                    discountForUserGroup =
                                        discountsForUserGroup
                                            .Where(p => p.ApplyDiscountToProductAttributes && 
                                                        p.ApplyDiscountToHigherUserGroups &&
                                                        p.UserGroup.GroupPriority <= user.UserGroup.GroupPriority)
                                            .OrderByDescending(p => p.UserGroup.GroupPriority)
                                            .FirstOrDefault();
                                    if (discountForUserGroup?.DiscountPercent > 0)
                                    {
                                        var priceForCurrentUser =
                                            option.Price - (option.Price * discountForUserGroup.DiscountPercent) / 100;

                                        if (priceForCurrentUser <= 0)
                                        {
                                            result.Add(option);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result.DistinctBy(p => p.Id).ToList();
        }
    }
}