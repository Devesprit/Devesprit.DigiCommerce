namespace Devesprit.Data
{
    public partial interface IApplicationDbInitializer
    {
        void Initialize(AppDbContext db);
    }
}
