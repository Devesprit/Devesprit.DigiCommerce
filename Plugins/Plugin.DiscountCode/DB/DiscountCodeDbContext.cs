using System.Data.Entity;
using Devesprit.Data;

namespace Plugin.DiscountCode.DB
{
    public class DiscountCodeDbContext: AppDbContext
    {
        public DbSet<TblDiscountCodes> DiscountCode { get; set; }
        public DbSet<TblInvoicesDiscountCode> InvoicesDiscountCode { get; set; }
    }
}