using System.Data.Entity;

namespace Devesprit.Data
{
    public partial interface IDbContextCustomizer
    {
        void OnModelCreating(DbModelBuilder modelBuilder);

        int Order { get; }
    }
}
