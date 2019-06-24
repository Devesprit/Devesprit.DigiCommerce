using System.Web.Mvc;
using Devesprit.Core;
using X.PagedList.Mvc.Common;

namespace Devesprit.WebFramework
{
    public partial class TwitterBootstrap4Pager : PagedListRenderOptions
    {
        public TwitterBootstrap4Pager()
        {
            var isRtl = DependencyResolver.Current.GetService<IWorkContext>().CurrentLanguage.IsRtl;
            this.DisplayLinkToPreviousPage = PagedListDisplayMode.Always;
            this.DisplayLinkToNextPage = PagedListDisplayMode.Always;
            this.MaximumPageNumbersToDisplay = new int?(5);
            this.LiElementClasses = new[] { "page-item" };
            this.UlElementClasses = new[] { "pagination", "justify-content-center" };
            this.PageClasses = new[] { "page-link" };
            this.EllipsesFormat = "<span class=\"page-link\"><i class=\"fa fa-ellipsis-h\"></i></span>";

            if (isRtl)
            {
                this.LinkToNextPageFormat = "<i class=\"fa fa-angle-left\"></i>";
                this.LinkToPreviousPageFormat = "<i class=\"fa fa-angle-right\"></i>";
                this.LinkToLastPageFormat = "<i class=\"fa fa-angle-double-left\"></i>";
                this.LinkToFirstPageFormat = "<i class=\"fa fa-angle-double-right\"></i>";
            }
            else
            {
                this.LinkToNextPageFormat = "<i class=\"fa fa-angle-right\"></i>";
                this.LinkToPreviousPageFormat = "<i class=\"fa fa-angle-left\"></i>";
                this.LinkToLastPageFormat = "<i class=\"fa fa-angle-double-right\"></i>";
                this.LinkToFirstPageFormat = "<i class=\"fa fa-angle-double-left\"></i>";
            }

            this.DisplayLinkToLastPage = PagedListDisplayMode.IfNeeded;
            this.DisplayLinkToFirstPage = PagedListDisplayMode.IfNeeded;
        }
    }
}