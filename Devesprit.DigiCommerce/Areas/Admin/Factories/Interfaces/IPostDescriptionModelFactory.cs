using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface IPostDescriptionModelFactory
    {
        Task<PostDescriptionModel> PreparePostDescriptionModelAsync(TblPostDescriptions description, int postId);
        TblPostDescriptions PrepareTblPostDescriptions(PostDescriptionModel description);
    }
}