using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Devesprit.Data
{
    public static class DbContextCustomizer
    {
        private static readonly List<Tuple<Action<DbModelBuilder>, int>> ModelCustomization;

        static DbContextCustomizer()
        {
            ModelCustomization = new List<Tuple<Action<DbModelBuilder>, int>>();
        }

        public static void RegisterModelCustomization(Action<DbModelBuilder> modelCustomization, int order)
        {
            ModelCustomization.Add(new Tuple<Action<DbModelBuilder>, int>(modelCustomization, order));
        }

        internal static void ApplyCustomization(DbModelBuilder modelBuilder)
        {
            foreach (var tuple in ModelCustomization.OrderBy(p=> p.Item2))
            {
                tuple.Item1?.Invoke(modelBuilder);
            }
        }
    }
}
