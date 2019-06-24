using System.Data.Entity;
using Devesprit.Data;

namespace Plugin.Widgets.Slider.DB
{
    public class SliderDbContextCustomizer : IDbContextCustomizer
    {
        public void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TblSlider>().ToTable("Plugin_Widgets_Slider");
        }

        public int Order => 1;
    }
}