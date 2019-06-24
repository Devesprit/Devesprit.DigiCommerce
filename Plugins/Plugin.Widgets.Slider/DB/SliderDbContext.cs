using System.Data.Entity;
using Devesprit.Data;

namespace Plugin.Widgets.Slider.DB
{
    public class SliderDbContext: AppDbContext
    {
        public DbSet<TblSlider> Slider { get; set; }
    }
}