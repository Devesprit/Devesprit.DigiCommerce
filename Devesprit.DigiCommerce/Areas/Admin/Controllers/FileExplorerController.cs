using System;
using System.IO;
using System.Web.Mvc;
using Devesprit.DigiCommerce.Areas.Admin.Controllers.Event;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.WebFramework.ActionFilters;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [UserHasPermission("FileManager")]
    public partial class FileExplorerController : BaseController
    {
        [HttpPost]
        public virtual ActionResult Index(string textboxId, string path)
        {
            var dir = Server.MapPath("~/FileManagerContent/");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            ViewBag.TextboxId = textboxId;

            ViewBag.Path = string.IsNullOrWhiteSpace(path) || !Directory.Exists(Server.MapPath(path)) ? "/FileManagerContent" : path;

            return View();
        }

        public virtual ActionResult FileAction(FileExplorerParams args)
        {
            try
            {
                EventPublisher.Publish(new FileExplorerActionEvent(args));

                var operation = new FileExplorerOperations();
                switch (args.ActionType)
                {
                    case "Read":
                        return Json(operation.Read(args.Path, args.ExtensionsAllow), JsonRequestBehavior.AllowGet);
                    case "CreateFolder":
                        return Json(operation.CreateFolder(args.Path, args.Name), JsonRequestBehavior.AllowGet);
                    case "Paste":
                        return Json(operation.Paste(args.LocationFrom, args.LocationTo, args.Names, args.Action, args.CommonFiles), JsonRequestBehavior.AllowGet);
                    case "Remove":
                        return Json(operation.Remove(args.Names, args.Path), JsonRequestBehavior.AllowGet);
                    case "Rename":
                        return Json(operation.Rename(args.Path, args.Name, args.NewName, args.CommonFiles), JsonRequestBehavior.AllowGet);
                    case "GetDetails":
                        return Json(operation.GetDetails(args.Path, args.Names), JsonRequestBehavior.AllowGet);
                    case "Download":
                        operation.Download(args.Path, args.Names);
                        break;
                    case "Upload":
                        operation.Upload(args.FileUpload, args.Path);
                        break;
                    case "Search":
                        return Json(operation.Search(args.Path, args.ExtensionsAllow, args.SearchString, args.CaseSensitive), JsonRequestBehavior.AllowGet);
                    case "GetImage":
                        operation.GetImage(args.Path, args.CanCompress, args.ImageSize, args.SelectedItems);
                        break;
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                var response = new FileExplorerResponse {error = e.GetType().FullName + ", " + e.Message};
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }
    }
}