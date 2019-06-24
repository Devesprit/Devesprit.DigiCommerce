using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;
using X.PagedList;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class AdminProductModelFactory : IAdminProductModelFactory
    {
        public virtual async Task<ProductModel> PrepareProductModelAsync(TblProducts product)
        {
            ProductModel result;
            if (product == null)
            {
                result = new ProductModel();
            }
            else
            {
                result = Mapper.Map<ProductModel>(product);
                await product.LoadAllLocalizedStringsToModelAsync(result);

                var tags = await product.Tags.Select(p => p.Tag).OrderBy(p => p).ToListAsync();
                result.ProductTags = tags?.ToArray() ?? new string[] { };

                var categories = await product.Categories.OrderBy(p => p.CategoryName).Select(p => p.Id).ToListAsync();
                result.ProductCategories = categories.ToArray();
            }

            return result;
        }

        public virtual TblProducts PrepareTblProducts(ProductModel product)
        {
            var result = Mapper.Map<TblProducts>(product);
            result.Tags = product.ProductTags?.Select(p => new TblPostTags() { Tag = p }).ToList();
            result.Categories =
                product.ProductCategories?.Select(p => new TblPostCategories() { Id = p }).ToList();

            return result;
        }

    }
}