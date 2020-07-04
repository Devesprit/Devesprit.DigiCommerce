using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
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
    public partial class AdminPanelProductService : PostService<TblProducts>, IAdminPanelProductService
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

        public AdminPanelProductService(AppDbContext dbContext,
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


        public override async Task<TblProducts> FindByIdAsync(int id)
        {
            var result = await _dbContext.Products
                .Where(p => p.Id == id)
                .Include(p => p.Categories)
                .Include(p => p.Descriptions)
                .Include(p => p.Images)
                .Include(p => p.Attributes)
                .Include(p => p.Attributes.Select(x => x.PostAttribute))
                .Include(p => p.Attributes.Select(x => x.AttributeOption))
                .Include(p => p.CheckoutAttributes)
                .Include(p => p.CheckoutAttributes.Select(x => x.Options))
                .Include(p => p.CheckoutAttributes.Select(x => x.Options.Select(o => o.FileServer)))
                .Include(p => p.CheckoutAttributes.Select(x => x.Options.Select(o => o.DownloadLimitedToUserGroup)))
                .Include(p => p.DiscountsForUserGroups)
                .Include(p => p.DownloadLimitedToUserGroup)
                .Include(p => p.Tags)
                .Include(p => p.AlternativeSlugs)
                .Include(p=> p.DownloadsLog)
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
                .Where(p => p.Slug == slug || p.AlternativeSlugs.Any(x=> x.Slug == slug))
                .Include(p => p.Categories)
                .Include(p => p.Descriptions)
                .Include(p => p.Images)
                .Include(p => p.Attributes)
                .Include(p => p.Attributes.Select(x => x.PostAttribute))
                .Include(p => p.Attributes.Select(x => x.AttributeOption))
                .Include(p => p.CheckoutAttributes)
                .Include(p => p.CheckoutAttributes.Select(x => x.Options))
                .Include(p => p.CheckoutAttributes.Select(x => x.Options.Select(o => o.FileServer)))
                .Include(p => p.CheckoutAttributes.Select(x => x.Options.Select(o => o.DownloadLimitedToUserGroup)))
                .Include(p => p.DiscountsForUserGroups)
                .Include(p => p.DownloadLimitedToUserGroup)
                .Include(p => p.Tags)
                .Include(p => p.AlternativeSlugs)
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

        public virtual async Task IncreaseNumberOfPurchasesAsync(TblProducts product, int value = 1)
        {
            product.NumberOfPurchases += value;
            _dbContext.Products.AddOrUpdate(product);
            await _dbContext.SaveChangesAsync();
        }
    }
}