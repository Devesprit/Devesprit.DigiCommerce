using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.Services.Posts;
using X.PagedList;

namespace Devesprit.Services.Products
{
    public partial interface IProductService: IPostService<TblProducts>
    {
        IPagedList<TblProducts> GetBestSelling(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);
        int GetNumberOfDownloads(int productId);
        string GenerateDiscountsForUserGroupsDescription(TblProducts product, TblUsers user);
        double CalculateProductPriceForUser(TblProducts product, TblUsers user);
        ProductService.UserCanDownloadProductResult UserCanDownloadProduct(TblProducts product, TblUsers user, bool demoFiles);
        Task<List<TblProductCheckoutAttributeOptions>> GetUserDownloadableAttributesAsync(TblProducts product,
            TblUsers user);
    }
}
