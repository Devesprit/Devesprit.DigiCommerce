using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Devesprit.Utilities;

namespace Devesprit.WebFramework
{
    public static partial class Utils
    {
        public static string SaveToAppData(this HttpPostedFileBase file)
        {
            var fileName = Guid.NewGuid().ToString("N") + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + "_" +
                           file.FileName;

            var path = HttpContext.Current.Server.MapPath("~").TrimEnd('\\') + "\\Content\\Upload\\" + FileUtils.DetectFileType(file.FileName) + "\\"+fileName[0]+"\\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            file.SaveAs(path + fileName);
            return "/Content/Upload/" + FileUtils.DetectFileType(file.FileName) + "/" + fileName[0] + "/" + fileName;
        }

        public static string SaveToAppData(this byte[] fileData, string fileName)
        {
            if (fileData == null || fileData.Length ==0)
            {
                return string.Empty;
            }

            fileName = Path.GetFileName(fileName);
            var localFileName = Guid.NewGuid().ToString("N") + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + "_" +
                                fileName;

            var path = HttpContext.Current.Server.MapPath("~").TrimEnd('\\') + "\\Content\\Upload\\" +
                       FileUtils.DetectFileType(fileName) + "\\" + localFileName[0] + "\\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllBytes(path + localFileName, fileData);
            return "/Content/Upload/" + FileUtils.DetectFileType(fileName) + "/" + localFileName[0] + "/" +
                   localFileName;
        }

        public static HttpPostedFileBase ConstructHttpPostedFile(byte[] data, string filename)
        {
            return (HttpPostedFileBase)new MemoryPostedFile(data, filename);
        }

        public static MvcHtmlString ToMvcHtmlString(this MvcHtmlString htmlString)
        {
            if (htmlString != null)
            {
                return new MvcHtmlString(HttpUtility.HtmlDecode(htmlString.ToString()));
            }
            return null;
        }
    }
}