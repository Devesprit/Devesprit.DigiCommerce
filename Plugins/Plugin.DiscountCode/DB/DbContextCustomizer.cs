using System.Data.Entity;
using Devesprit.Data;

namespace Plugin.DiscountCode.DB
{
    public class SliderDbContextCustomizer : IDbContextCustomizer
    {
        public void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TblDiscountCodes>().ToTable("Plugin_Discount_Codes");
            modelBuilder.Entity<TblInvoicesDiscountCode>().ToTable("Plugin_Invoices_Discount_Code");
        }

        public int Order => 1;
    }
}