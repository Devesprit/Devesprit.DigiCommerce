using Devesprit.DigiCommerce.Models.Post;
using Devesprit.Utilities.Extensions;

namespace Devesprit.DigiCommerce.Models.Products
{
    public partial class ProductCardViewModel : PostCardViewModel
    {
        public int NumberOfDownloads { get; set; }
        public string NumberOfDownloadsStr => NumberOfDownloads.FormatNumber();
        public ProductCardDownloadModel DownloadModel { get; set; }
    }
}