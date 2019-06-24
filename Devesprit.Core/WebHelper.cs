using System;
using System.Web;

namespace Devesprit.Core
{
    public partial class WebHelper : IWebHelper
    {
        public virtual void RestartAppDomain()
        {
            if (CommonHelper.GetTrustLevel() > AspNetHostingPermissionLevel.Medium)
            {
                //full trust
                HttpRuntime.UnloadAppDomain();

                TryWriteGlobalAsax();
            }
            else
            {
                //medium trust
                bool success = TryWriteWebConfig();
                if (!success)
                {
                    throw new Exception("Devesprit.DigiCommerce needs to be restarted due to a configuration change, but was unable to do so." + Environment.NewLine +
                                        "To prevent this issue in the future, a change to the web server configuration is required:" + Environment.NewLine +
                                        "- run the application in a full trust environment, or" + Environment.NewLine +
                                        "- give the application write access to the 'web.config' file.");
                }
                success = TryWriteGlobalAsax();

                if (!success)
                {
                    throw new Exception("Devesprit.DigiCommerce needs to be restarted due to a configuration change, but was unable to do so." + Environment.NewLine +
                                        "To prevent this issue in the future, a change to the web server configuration is required:" + Environment.NewLine +
                                        "- run the application in a full trust environment, or" + Environment.NewLine +
                                        "- give the application write access to the 'Global.asax' file.");
                }
            }
        }

        protected virtual bool TryWriteWebConfig()
        {
            try
            {
                System.IO.File.SetLastWriteTimeUtc(CommonHelper.MapPath("~/web.config"), DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected virtual bool TryWriteGlobalAsax()
        {
            try
            {
                System.IO.File.SetLastWriteTimeUtc(CommonHelper.MapPath("~/global.asax"), DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}