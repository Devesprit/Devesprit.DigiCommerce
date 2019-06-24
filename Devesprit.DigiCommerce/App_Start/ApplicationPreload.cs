using System.Web.Hosting;
using Hangfire;

namespace Devesprit.DigiCommerce
{
    public class ApplicationPreload : IProcessHostPreloadClient
    {
        public void Preload(string[] parameters)
        {
            HangfireAspNet.Use(Startup.GetHangfireConfiguration);
        }
    }
}