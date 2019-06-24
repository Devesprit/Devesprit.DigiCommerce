using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace Devesprit.FileServer.ElmahConfig
{
    public partial class ElmahErrorHandler : IErrorHandler
    {
        public virtual bool HandleError(Exception error)
        {
            return false;
        }
        public virtual void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (error == null)
            {
                return;
            }

            if (HttpContext.Current == null)
            {
                return;
            }

            Elmah.ErrorSignal.FromCurrentContext().Raise(error);
        }
    }
}