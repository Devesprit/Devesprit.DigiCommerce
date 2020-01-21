using System;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Text;
using System.Web.Mvc;
using Npgsql;

namespace Devesprit.Data.Migrations
{
    public partial class Configuration : DbMigrationsConfiguration<AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;

            var providerName = ConfigurationManager.ConnectionStrings["SITE.CONNSTR"].ProviderName;
            if (providerName.ToLower().Contains("MySql".ToLower()))
            {
                SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator());
            }
            else
            if (providerName.ToLower().Contains("Npgsql".ToLower()))
            {
                SetSqlGenerator("Npgsql", new NpgsqlMigrationSqlGenerator());
            }
        }

        protected override void Seed(AppDbContext context)
        {
            var dbInitializers = DependencyResolver.Current.GetServices<IApplicationDbInitializer>();
            foreach (var dbInitializer in dbInitializers)
            {
                try
                {
                    dbInitializer.Initialize(context);
                }
                catch (DbEntityValidationException e)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine();
                    sb.AppendLine();
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        sb.AppendLine(
                            $"- Entity of type \"{eve.Entry.Entity.GetType().FullName}\" in state \"{eve.Entry.State}\" has the following validation errors:");
                        foreach (var ve in eve.ValidationErrors)
                        {
                            sb.AppendLine(
                                $"-- Property: \"{ve.PropertyName}\", Value: \"{eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName)}\", Error: \"{ve.ErrorMessage}\"");
                        }
                    }
                    sb.AppendLine();
                    throw new Exception(sb.ToString());
                }
            }

            base.Seed(context);
        }
    }
}
