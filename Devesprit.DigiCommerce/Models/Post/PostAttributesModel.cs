using Devesprit.Data.Enums;

namespace Devesprit.DigiCommerce.Models.Post
{
    public partial class PostAttributesModel
    {
        public PostAttributeType Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int DisplayOrder { get; set; }
    }
}