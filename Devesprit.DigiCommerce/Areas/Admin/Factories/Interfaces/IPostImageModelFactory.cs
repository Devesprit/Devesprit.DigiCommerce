using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface IPostImageModelFactory
    {
        Task<PostImageModel> PreparePostImageModelAsync(TblPostImages image, int postId);
        Task<TblPostImages> PrepareTblPostImagesAsync(PostImageModel image);
    }
}