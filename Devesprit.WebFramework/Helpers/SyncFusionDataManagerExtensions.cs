using System.Linq;
using Syncfusion.JavaScript;
using Syncfusion.JavaScript.DataSources;

namespace Devesprit.WebFramework.Helpers
{
    public static partial class SyncFusionDataManagerExtensions
    {
        public static IQueryable<T> ApplyDataManager<T>(this IQueryable<T> dataSource, DataManager manager, out int count, bool applySearch = false)
        {
            if (manager?.Where != null && manager.Where.Count > 0)
                dataSource = dataSource.PerformWhereFilter<T>(manager.Where, string.Empty);
            if (manager?.Search != null && manager.Search.Count > 0 && applySearch)
                dataSource = QueryableDataOperations.PerformSearching<T>(dataSource, manager.Search);
            if (manager?.Sorted != null && manager.Sorted.Count > 0)
                dataSource = dataSource.PerformSorting<T>(manager.Sorted);

            count = dataSource.Count();

            if (manager != null && manager.Skip != 0)
                dataSource = dataSource.PerformSkip<T>(manager.Skip);
            if (manager != null && manager.Take != 0)
                dataSource = dataSource.PerformTake<T>(manager.Take);
            return dataSource;
        }
    }
}