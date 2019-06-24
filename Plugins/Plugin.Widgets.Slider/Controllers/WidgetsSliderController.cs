using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Localization;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Plugin.Widgets.Slider.DB;
using Plugin.Widgets.Slider.Models;
using Syncfusion.JavaScript;
using Z.EntityFramework.Plus;

namespace Plugin.Widgets.Slider.Controllers
{
    public partial class WidgetsSliderController : BaseController
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly SliderDbContext _dbContext;

        private const string CacheKey = "Plugin.Widgets.Slider";

        public WidgetsSliderController(ILocalizationService localizationService, 
            ILocalizedEntityService localizedEntityService,
            SliderDbContext dbContext)
        {
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _dbContext = dbContext;
        }

        [Authorize(Roles = "Admin")]
        public virtual ActionResult Configure()
        {
            return View("~/Plugins/Plugin.Widgets.Slider/Views/Configure.cshtml");
        }

        [Authorize(Roles = "Admin")]
        public virtual async Task<ActionResult> Editor(int? id)
        {
            if (id != null)
            {
                var record = await _dbContext.Slider
                    .DeferredFirstOrDefault(p => p.Id == id)
                    .FromCacheAsync(CacheKey);
                if (record != null)
                {
                    var model = Mapper.Map<SliderViewModel>(record);
                    await record.LoadAllLocalizedStringsToModelAsync(model);
                    return View("~/Plugins/Plugin.Widgets.Slider/Views/Editor.cshtml", model);
                }
            }
            return View("~/Plugins/Plugin.Widgets.Slider/Views/Editor.cshtml", new SliderViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Editor(SliderViewModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Plugins/Plugin.Widgets.Slider/Views/Editor.cshtml", model);
            }

            var record = Mapper.Map<TblSlider>(model);
            var recordId = model.Id;
            try
            {
                if (model.Id == null)
                {
                    //Add new record
                    _dbContext.Slider.Add(record);
                    await _dbContext.SaveChangesAsync();
                    QueryCacheManager.ExpireTag(CacheKey);
                    recordId = record.Id;
                }
                else
                {
                    //Edit record
                    _dbContext.Slider.AddOrUpdate(record);
                    await _dbContext.SaveChangesAsync();
                    QueryCacheManager.ExpireTag(CacheKey);
                }

                await _localizedEntityService.SaveAllLocalizedStringsAsync(record, model);
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                return View("~/Plugins/Plugin.Widgets.Slider/Views/Editor.cshtml", model);
            }

            if (saveAndContinue != null && saveAndContinue.Value)
            {
                return RedirectToAction("Editor", new { id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshSlidersGrid();
                             </script>");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                {
                    await _dbContext.Slider.Where(p => p.Id == key).DeleteAsync();
                    await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(TblSlider).Name, key);
                }
                QueryCacheManager.ExpireTag(CacheKey);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [Authorize(Roles = "Admin")]
        public virtual ActionResult GridDataSource(DataManager dm)
        {
            var query = _dbContext.Slider;

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.ImageUrl,
                p.Title,
                p.Zone,
                p.Visible,
                p.DisplayOrder
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        public virtual ActionResult Index(string widgetZone, object additionalData = null)
        {
            var images = _dbContext.Slider
                .Where(p => p.Zone == widgetZone && p.Visible).FromCache(CacheKey);
            return View("~/Plugins/Plugin.Widgets.Slider/Views/Slider.cshtml", images);
        }
    }
}
