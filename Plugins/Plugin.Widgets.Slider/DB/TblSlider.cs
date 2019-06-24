using System.ComponentModel.DataAnnotations;
using Devesprit.Data;

namespace Plugin.Widgets.Slider.DB
{
    public class TblSlider : BaseEntity
    {
        [MaxLength(500)]
        public string ImageUrl { get; set; }
        [MaxLength(500)]
        public string Link { get; set; }
        public string Target { get; set; }
        public string OnClickJs { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Visible { get; set; }
        [Required]
        public string Zone { get; set; }
        public int DisplayOrder { get; set; }
    }
}