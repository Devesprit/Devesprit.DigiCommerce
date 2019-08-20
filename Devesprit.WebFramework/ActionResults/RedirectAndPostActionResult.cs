using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace Devesprit.WebFramework.ActionResults
{
    public class RedirectAndPostActionResult : ActionResult
    {
        public string Url { get; set; }

        public Dictionary<string, string> PostData { get; set; }

        public RedirectAndPostActionResult(string url, Dictionary<string, string> postData)
        {
            this.Url = url;
            this.PostData = postData ?? new Dictionary<string, string>();
        }

        public override void ExecuteResult(ControllerContext context)
        {
            string s = this.BuildPostForm(this.Url, this.PostData);
            context.HttpContext.Response.Write(s);
        }

        private string BuildPostForm(string url, Dictionary<string, string> postData)
        {
            string str = "__PostForm";
            StringBuilder stringBuilder1 = new StringBuilder();
            stringBuilder1.Append(
                $"<form id=\"{(object) str}\" name=\"{(object) str}\" action=\"{(object) url}\" method=\"POST\">");
            foreach (KeyValuePair<string, string> keyValuePair in postData)
                stringBuilder1.Append(
                    $"<input type=\"hidden\" name=\"{(object) keyValuePair.Key}\" value=\"{keyValuePair.Value}\"/>");
            stringBuilder1.Append("</form>");
            StringBuilder stringBuilder2 = new StringBuilder();
            stringBuilder2.Append("<script language=\"javascript\">");
            stringBuilder2.Append($"var v{(object) str}=document.{(object) str};");
            stringBuilder2.Append($"v{(object) str}.submit();");
            stringBuilder2.Append("</script>");
            return stringBuilder1.ToString() + stringBuilder2.ToString();
        }
    }
}
