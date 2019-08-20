using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Languages;
using Devesprit.Services.MemoryCache;
using Devesprit.Utilities.Extensions;
using Elmah;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Localization
{
    public partial class LocalizationService : ILocalizationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IWorkContext _workContext;
        private readonly ILanguagesService _languagesService;
        private readonly IMemoryCache _memoryCache;

        public LocalizationService(AppDbContext dbContext, IWorkContext workContext, ILanguagesService languagesService, IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _workContext = workContext;
            _languagesService = languagesService;
            _memoryCache = memoryCache;
        }

        public virtual IQueryable<TblLocalizedStrings> GetAsQueryable()
        {
            return _dbContext.LocalizedStrings.OrderBy(p => p.ResourceName);
        }

        public virtual void Delete(int id)
        {
            _dbContext.LocalizedStrings.Where(p=> p.Id == id).Delete();
            ClearCache();
        }

        public virtual TblLocalizedStrings FindById(int id)
        {
            var result = GetAsEnumerable().FirstOrDefault(p => p.Id == id);
            return result;
        }

        public virtual async Task<TblLocalizedStrings> FindByIdAsync(int id)
        {
            var result = (await GetAsEnumerableAsync()).FirstOrDefault(p => p.Id == id);
            return result;
        }
        
        public virtual TblLocalizedStrings FindByName(string resourceName, int languageId, bool logIfNotFound = false)
        {
            var result = GetAsEnumerable().FirstOrDefault(p =>
                p.ResourceName.Trim().Equals(resourceName.Trim(), StringComparison.InvariantCultureIgnoreCase) &&
                p.LanguageId == languageId);

            if (result == null && logIfNotFound)
            {
                try
                {
                    var language = _languagesService.FindById(languageId);
                    Elmah.ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(
                        new Error(
                            new Exception(
                                $"Can not found resource with name '{resourceName}' (Language: {language?.LanguageName ?? " ID= " + languageId})"),
                            System.Web.HttpContext.Current)
                        { Type = "Localization Warning" });
                }
                catch 
                {}
            }

            return result;
        }

        public virtual int Add(TblLocalizedStrings record)
        {
            _dbContext.LocalizedStrings.Add(record);
            _dbContext.SaveChanges();

            ClearCache();

            return record.Id;
        }

        public virtual void Update(TblLocalizedStrings record)
        {
            _dbContext.LocalizedStrings.AddOrUpdate(record);
            _dbContext.SaveChanges();

            ClearCache();
        }

        public virtual string GetResource(string resourceName)
        {
            resourceName = resourceName.Trim().ToLowerInvariant();

            if (_workContext.CurrentLanguage != null)
                return GetResource(resourceName, _workContext.CurrentLanguage.Id, 
                    "", false, true);

            return "";
        }

        public virtual string GetResource(string resourceName, int languageId, string defaultValue = "",
            bool returnEmptyIfNotFound = false, bool logIfNotFound = false)
        {
            var result = FindByName(resourceName, languageId, logIfNotFound)?.ResourceValue ?? string.Empty;

            if (string.IsNullOrEmpty(result))
            {
                if (!string.IsNullOrEmpty(defaultValue))
                {
                    result = defaultValue;
                }
                else
                {
                    if (!returnEmptyIfNotFound)
                    {
                        var defaultLanguageValue = FindByName(resourceName, _languagesService.GetDefaultLanguage().Id, logIfNotFound)?.ResourceValue ?? string.Empty;
                        if (!string.IsNullOrEmpty(defaultLanguageValue))
                        {
                            result = defaultLanguageValue;
                        }
                        else
                        {
                            result = resourceName;
                        }
                    }
                }
            }
            return result;
        }

        public virtual async Task<IEnumerable<TblLocalizedStrings>> GetAsEnumerableAsync()
        {
            // Try get result from cache
            if (_memoryCache.Contains(QueryCacheTag.LocalizedString))
            {
                return _memoryCache.GetObject<List<TblLocalizedStrings>>(QueryCacheTag.LocalizedString);
            }

            var result = await GetAsQueryable().FromCacheAsync(QueryCacheTag.LocalizedString);

            _memoryCache.AddObject(QueryCacheTag.LocalizedString, result, TimeSpan.FromDays(30));
            return result;
        }

        public virtual IEnumerable<TblLocalizedStrings> GetAsEnumerable()
        {
            // Try get result from cache
            if (_memoryCache.Contains(QueryCacheTag.LocalizedString))
            {
                return _memoryCache.GetObject<List<TblLocalizedStrings>>(QueryCacheTag.LocalizedString);
            }

            var result = GetAsQueryable().FromCache(QueryCacheTag.LocalizedString).ToList();

            _memoryCache.AddObject(QueryCacheTag.LocalizedString, result, TimeSpan.FromDays(30));
            return result;
        }

        public virtual async Task<string> ExportResourcesToXmlAsync(TblLanguages language)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Language");
            xmlWriter.WriteAttributeString("Name", language.LanguageName);
            xmlWriter.WriteAttributeString("ISO", language.IsoCode);
            
            var resources = (await GetAsEnumerableAsync()).Where(p=> p.LanguageId == language.Id);
            foreach (var resource in resources)
            {
                xmlWriter.WriteStartElement("LocaleResource");
                xmlWriter.WriteAttributeString("Name", resource.ResourceName);
                xmlWriter.WriteElementString("Value", null, resource.ResourceValue);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString().PrettyXml();
        }

        public virtual async Task ImportResourcesFromXmlAsync(int languageId, string xml, bool updateExistingResources = true)
        {
            if (String.IsNullOrEmpty(xml))
                return;

            var language = _languagesService.GetAsQueryable().FirstOrDefault(p=> p.Id == languageId);

            if (language == null)
            {
                return;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            var nodes = xmlDoc.SelectNodes(@"//Language/LocaleResource");
            if (nodes == null)
            {
                return;
            }
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes != null)
                {
                    string name = node.Attributes["Name"]?.InnerText.Trim();
                    string value = "";
                    var valueNode = node.SelectSingleNode("Value");
                    if (valueNode != null)
                        value = valueNode.InnerText;

                    if (String.IsNullOrEmpty(name))
                        continue;

                    var resource = language.LocalizedStrings.FirstOrDefault(x => x.ResourceName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                    if (resource != null)
                    {
                        if (updateExistingResources)
                        {
                            resource.ResourceValue = value;
                            _dbContext.LocalizedStrings.AddOrUpdate(resource);
                        }
                    }
                    else
                    {
                        _dbContext.LocalizedStrings.Add(new TblLocalizedStrings()
                        {
                            LanguageId = language.Id,
                            ResourceName = name,
                            ResourceValue = value
                        });
                    }
                }
            }

            await _dbContext.SaveChangesAsync();

            //clear cache
            ClearCache();
        }

        public virtual void ClearCache()
        {
            QueryCacheManager.ExpireTag(QueryCacheTag.LocalizedString);
            _memoryCache.RemoveObject(QueryCacheTag.LocalizedString);
        }
    }
}