using System.Collections.Generic;
using System.Web;
using Devesprit.Data.Events;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers.Event
{
    public partial class FileExplorerActionEvent: IEvent
    {
        public FileExplorerParams FileExplorerParams { get; }

        public FileExplorerActionEvent(FileExplorerParams fileExplorerParams)
        {
            FileExplorerParams = fileExplorerParams;
        }
    }
}