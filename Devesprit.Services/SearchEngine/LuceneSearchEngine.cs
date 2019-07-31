using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using Devesprit.Core.Settings;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.Events;
using Devesprit.Services.Languages;
using Devesprit.Services.Localization;
using Devesprit.Services.Posts;
using Devesprit.Services.Products;
using Devesprit.Utilities;
using Devesprit.Utilities.Extensions;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Search.Similar;
using Lucene.Net.Store;
using SpellChecker.Net.Search.Spell;

namespace Devesprit.Services.SearchEngine
{
    public partial class LuceneSearchEngine : ISearchEngine
    {
        private readonly IPostService<TblPosts> _postService;
        private readonly ILanguagesService _languagesService;
        private readonly ISettingService _settingService;
        private readonly IEventPublisher _eventPublisher;

        private readonly string _indexFilesPath = "LuceneFTSData";
        private readonly string _spellFilesPath;
        private const Lucene.Net.Util.Version Version = Lucene.Net.Util.Version.LUCENE_30;

        public LuceneSearchEngine(IPostService<TblPosts> postService,
            ILanguagesService languagesService,
            ISettingService settingService,
            IEventPublisher eventPublisher)
        {
            _postService = postService;
            _languagesService = languagesService;
            _settingService = settingService;
            _eventPublisher = eventPublisher;

            _indexFilesPath = HostingEnvironment.MapPath("~")?.TrimEnd('\\') + "\\" + _indexFilesPath ?? _indexFilesPath;
            _spellFilesPath = _indexFilesPath.TrimEnd('\\') + "\\Spell";
        }

        public virtual async Task<SearchResult> SearchAsync(string term, int? filterByCategory = null, int languageId = 0, PostType? postType = null, SearchPlace searchPlace = SearchPlace.Anywhere,
            int maxResult = 1000)
        {
            var result = new SearchResult();
            term = term.Trim();

            if (string.IsNullOrWhiteSpace(term))
            {
                return result;
            }

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            try
            {
                await Task.Run(() =>
                {
                    using (var directory = FSDirectory.Open(new DirectoryInfo(_indexFilesPath)))
                    {
                        using (var searcher = new IndexSearcher(directory, readOnly: true))
                        {
                            var searchInFields = new List<string>();
                            if (searchPlace == SearchPlace.Anywhere)
                            {
                                searchInFields.AddRange(new string[] { "Title", "Description", "Keywords", "Tags" });
                            }
                            else
                            {
                                if (searchPlace.HasFlagFast(SearchPlace.Title))
                                {
                                    searchInFields.Add("Title");
                                }

                                if (searchPlace.HasFlagFast(SearchPlace.Description))
                                {
                                    searchInFields.Add("Description");
                                }

                                if (searchPlace.HasFlagFast(SearchPlace.Keywords))
                                {
                                    searchInFields.Add("Keywords");
                                }

                                if (searchPlace.HasFlagFast(SearchPlace.Tags))
                                {
                                    searchInFields.Add("Tags");
                                }
                            }

                            var analyzer = new StandardAnalyzer(Version);
                            
                            QueryScorer scorer = null;
                            var parser = new MultiFieldQueryParser(Version, searchInFields.ToArray(), analyzer);
                            var query = ParseQuery(term, parser);

                            scorer = new QueryScorer(query);

                            BooleanFilter filter = null;
                            if (languageId > 0 || filterByCategory != null || postType != null)
                            {
                                filter = new BooleanFilter();
                                if (languageId > 0)
                                {
                                    filter.Add(new FilterClause(
                                        new QueryWrapperFilter(new TermQuery(new Term("LanguageId", languageId.ToString()))),
                                        Occur.MUST));
                                }
                                if (filterByCategory != null)
                                {
                                    filter.Add(new FilterClause(
                                        new QueryWrapperFilter(new TermQuery(new Term("Categories",
                                            filterByCategory.Value.ToString()))), Occur.MUST));
                                }
                                if (postType != null)
                                {
                                    filter.Add(new FilterClause(
                                        new QueryWrapperFilter(new TermQuery(new Term("PostType",
                                            postType.Value.ToString()))), Occur.MUST));
                                }
                            }

                            var currentSettings = _settingService.LoadSetting<SiteSettings>();
                            if (!currentSettings.EnableBlog)
                            {
                                //Filter Blog Posts if Blog is disabled
                                if (filter == null)
                                {
                                    filter = new BooleanFilter();
                                }
                                filter.Add(new FilterClause(
                                    new QueryWrapperFilter(new TermQuery(new Term("PostType",
                                        PostType.BlogPost.ToString()))), Occur.MUST_NOT));
                            }

                            var hits = filter == null
                                ? searcher.Search(query, maxResult).ScoreDocs
                                : searcher.Search(query, filter, maxResult).ScoreDocs;
                            if (hits.Length == 0)
                            {
                                term = SearchByPartialWords(term);
                                query = ParseQuery(term, parser);
                                scorer = new QueryScorer(query);
                                hits = filter == null
                                    ? searcher.Search(query, maxResult).ScoreDocs
                                    : searcher.Search(query, filter, maxResult).ScoreDocs;
                            }

                            var formatter = new SimpleHTMLFormatter(
                                "<span class='badge badge-warning'>",
                                "</span>");
                            var fragmenter = new SimpleFragmenter(300);
                            var highlighter = new Highlighter(formatter, scorer) { TextFragmenter = fragmenter };

                            foreach (var scoreDoc in hits)
                            {
                                var doc = searcher.Doc(scoreDoc.Doc);
                                result.Documents.Add(new SearchResultDocument()
                                {
                                    DocumentId = int.Parse(doc.Get("ID")),
                                    LanguageId = int.Parse(doc.Get("LanguageId")),
                                    LanguageIsoCode = doc.Get("LanguageCode"),
                                    Score = scoreDoc.Score,
                                    DocumentTitle = GetHighlight("Title", highlighter, analyzer, doc.Get("Title"), false),
                                    DocumentBody = GetHighlight("Description", highlighter, analyzer, doc.Get("Description"), true),
                                    DocumentKeywords = doc.Get("Keywords"),
                                    DocumentTags = doc.Get("Tags"),
                                });
                            }

                            result.Documents = result.Documents.DistinctBy(p => new { p.DocumentBody, p.DocumentTitle })
                                .ToList();

                            analyzer.Close();

                            //SuggestSimilar
                            using (var spellDirectory = FSDirectory.Open(new DirectoryInfo(_spellFilesPath)))
                            {
                                using (var spellChecker = new SpellChecker.Net.Search.Spell.SpellChecker(spellDirectory))
                                {
                                    result.SuggestSimilar.AddRange(spellChecker.SuggestSimilar(term, 10, null, null, true));
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                result.Error = ex;
                result.HasError = true;
            }

            watch.Stop();
            result.ElapsedMilliseconds = watch.ElapsedMilliseconds;

            _eventPublisher.Publish(new SearchEvent(term, filterByCategory, languageId, postType, searchPlace, maxResult, result));

            return result;
        }

        public virtual SearchResult MoreLikeThis(int postId, int? filterByCategory = null, int languageId = 0, PostType? postType = null, SearchPlace searchPlace = SearchPlace.Title | SearchPlace.Description,
            int maxResult = 5)
        {
            var result = new SearchResult();

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            try
            {
                using (var directory = FSDirectory.Open(new DirectoryInfo(_indexFilesPath)))
                {
                    using (var searcher = new IndexSearcher(directory, readOnly: true))
                    {
                        var docNumber = GetLuceneDocNumber(postId, searcher);

                        if (docNumber == 0)
                        {
                            return result;
                        }

                        var searchInFields = new List<string>();
                        if (searchPlace == SearchPlace.Anywhere)
                        {
                            searchInFields.AddRange(new string[] { "Title", "Description", "Keywords", "Tags" });
                        }
                        else
                        {
                            if (searchPlace.HasFlagFast(SearchPlace.Title))
                            {
                                searchInFields.Add("Title");
                            }

                            if (searchPlace.HasFlagFast(SearchPlace.Description))
                            {
                                searchInFields.Add("Description");
                            }

                            if (searchPlace.HasFlagFast(SearchPlace.Keywords))
                            {
                                searchInFields.Add("Keywords");
                            }

                            if (searchPlace.HasFlagFast(SearchPlace.Tags))
                            {
                                searchInFields.Add("Tags");
                            }
                        }

                        var analyzer = new StandardAnalyzer(Version);
                        var moreLikeThis = new MoreLikeThis(searcher.IndexReader) { Analyzer = analyzer };
                        moreLikeThis.SetFieldNames(searchInFields.ToArray());
                        moreLikeThis.MinDocFreq = 1;
                        moreLikeThis.MinTermFreq = 1;
                        moreLikeThis.Boost = true;

                        var query = moreLikeThis.Like(docNumber);

                        var filter = new BooleanFilter();

                        filter.Add(new FilterClause(
                            new QueryWrapperFilter(new TermQuery(new Term("ID",
                                postId.ToString()))),
                            Occur.MUST_NOT));

                        if (languageId > 0)
                        {
                            filter.Add(new FilterClause(
                                new QueryWrapperFilter(new TermQuery(new Term("LanguageId",
                                    languageId.ToString()))),
                                Occur.MUST));
                        }
                        if (filterByCategory != null)
                        {
                            filter.Add(new FilterClause(
                                new QueryWrapperFilter(new TermQuery(new Term("Categories",
                                    filterByCategory.Value.ToString()))), Occur.MUST));
                        }
                        if (postType != null)
                        {
                            filter.Add(new FilterClause(
                                new QueryWrapperFilter(new TermQuery(new Term("PostType",
                                    postType.Value.ToString()))), Occur.MUST));
                        }


                        var hits = searcher.Search(query, filter, maxResult).ScoreDocs;

                        foreach (var scoreDoc in hits)
                        {
                            var doc = searcher.Doc(scoreDoc.Doc);
                            result.Documents.Add(new SearchResultDocument()
                            {
                                DocumentId = int.Parse(doc.Get("ID")),
                                LanguageId = int.Parse(doc.Get("LanguageId")),
                                LanguageIsoCode = doc.Get("LanguageCode"),
                                Score = scoreDoc.Score,
                                DocumentTitle = doc.Get("Title"),
                                DocumentBody = doc.Get("Description"),
                                DocumentKeywords = doc.Get("Keywords"),
                                DocumentTags = doc.Get("Tags"),
                            });
                        }

                        result.Documents = result.Documents.DistinctBy(p => new { p.DocumentBody, p.DocumentTitle })
                            .ToList();

                        analyzer.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                result.Error = ex;
                result.HasError = true;
            }

            watch.Stop();
            result.ElapsedMilliseconds = watch.ElapsedMilliseconds;
            return result;
        }

        public virtual async Task<SearchResult> AutoCompleteAsync(string prefix, int languageId = 0, int maxResult = 10)
        {
            var result = new SearchResult();
            prefix = prefix.Trim();

            if (string.IsNullOrWhiteSpace(prefix))
            {
                return result;
            }

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            try
            {
                await Task.Run(() =>
                {
                    using (var directory = FSDirectory.Open(new DirectoryInfo(_indexFilesPath)))
                    {
                        using (var searcher = new IndexSearcher(directory, readOnly: true))
                        {
                            BooleanFilter filter = null;
                            if (languageId > 0)
                            {
                                filter = new BooleanFilter();
                                filter.Add(new FilterClause(
                                    new QueryWrapperFilter(new TermQuery(new Term("LanguageId",
                                        languageId.ToString()))),
                                    Occur.MUST));
                            }

                            var hits = filter == null
                                ? searcher.Search(new PrefixQuery(new Term("Title", prefix)), maxResult).ScoreDocs
                                : searcher.Search(new PrefixQuery(new Term("Title", prefix)), filter, maxResult).ScoreDocs;
                            if (hits.Length == 0)
                            {
                                hits = filter == null
                                    ? searcher.Search(new PrefixQuery(new Term("Description", prefix)), maxResult).ScoreDocs
                                    : searcher.Search(new PrefixQuery(new Term("Description", prefix)), filter, maxResult).ScoreDocs;
                            }

                            foreach (var scoreDoc in hits)
                            {
                                var doc = searcher.Doc(scoreDoc.Doc);
                                result.Documents.Add(new SearchResultDocument()
                                {
                                    DocumentId = int.Parse(doc.Get("ID")),
                                    LanguageId = int.Parse(doc.Get("LanguageId")),
                                    LanguageIsoCode = doc.Get("LanguageCode"),
                                    Score = scoreDoc.Score,
                                    DocumentTitle = doc.Get("Title"),
                                    DocumentBody = doc.Get("Description"),
                                    DocumentKeywords = doc.Get("Keywords"),
                                    DocumentTags = doc.Get("Tags"),
                                });
                            }

                            result.Documents = result.Documents.DistinctBy(p => new { p.DocumentBody, p.DocumentTitle })
                                .ToList();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                result.Error = ex;
                result.HasError = true;
            }

            watch.Stop();
            result.ElapsedMilliseconds = watch.ElapsedMilliseconds;
            return result;
        }

        protected virtual int GetLuceneDocNumber(int postId, IndexSearcher searcher)
        {
            var analyzer = new StandardAnalyzer(Version);
            var parser = new QueryParser(Version, "ID", analyzer);
            var query = parser.Parse(postId.ToString());
            var doc = searcher.Search(query, 1);
            if (doc.TotalHits == 0)
            {
                return 0;
            }
            return doc.ScoreDocs[0].Doc;
        }

        protected virtual Query ParseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        protected virtual string SearchByPartialWords(string bodyTerm)
        {
            bodyTerm = bodyTerm.Replace("*", "").Replace("?", "");
            var terms = bodyTerm.Trim().Replace("-", " ").Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim() + "*");
            bodyTerm = string.Join(" ", terms);
            return bodyTerm;
        }

        protected virtual string GetHighlight(string fieldName, Highlighter highlighter, Analyzer analyzer, string fieldContent, bool truncateDefaultText)
        {
            var stream = analyzer.TokenStream(fieldName, new StringReader(fieldContent));
            var result = highlighter.GetBestFragments(stream, fieldContent, 1, "...");
            if (string.IsNullOrWhiteSpace(result))
            {
                result = truncateDefaultText ? fieldContent.ConvertHtmlToText().TruncateText(350) : fieldContent;
            }
            return result;
        }

        public virtual long CreateIndex()
        {
            try
            {
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();

                if (System.IO.Directory.Exists(_indexFilesPath))
                {
                    try
                    {
                        using (var directory = FSDirectory.Open(new DirectoryInfo(_indexFilesPath)))
                            directory.ClearLock(directory.GetLockId());
                    }
                    catch
                    {
                    }

                    FileUtils.DeleteDirRecursively(new DirectoryInfo(_indexFilesPath));
                }

                System.IO.Directory.CreateDirectory(_indexFilesPath);

                if (System.IO.Directory.Exists(_spellFilesPath))
                {
                    try
                    {
                        using (var directory = FSDirectory.Open(new DirectoryInfo(_spellFilesPath)))
                            directory.ClearLock(directory.GetLockId());
                    }
                    catch
                    {
                    }

                    FileUtils.DeleteDirRecursively(new DirectoryInfo(_spellFilesPath));
                }

                System.IO.Directory.CreateDirectory(_spellFilesPath);


                var allPosts = _postService.GetAsQueryable().Where(p => p.Published)
                    .Include(p => p.Descriptions)
                    .Include(p => p.Tags)
                    .Include(p => p.Categories);
                var languages = _languagesService.GetAsEnumerable();

                var analyzer = new StandardAnalyzer(Version);
                using (var directory = FSDirectory.Open(new DirectoryInfo(_indexFilesPath)))
                {
                    using (var writer =
                        new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                    {
                        foreach (var language in languages.OrderByDescending(p => p.IsDefault))
                        {
                            foreach (var post in allPosts)
                            {
                                writer.AddDocument(MapPost(post, language, post.PostType));
                            }
                        }

                        writer.Optimize();
                        writer.Commit();
                    }
                }

                using (var directory = FSDirectory.Open(new DirectoryInfo(_indexFilesPath)))
                {
                    using (var spellDirectory = FSDirectory.Open(new DirectoryInfo(_spellFilesPath)))
                    {
                        using (var indexReader = IndexReader.Open(directory, readOnly: true))
                        {
                            using (var spellChecker = new SpellChecker.Net.Search.Spell.SpellChecker(spellDirectory))
                            {
                                // Create SpellChecker Index
                                spellChecker.ClearIndex();
                                spellChecker.IndexDictionary(new LuceneDictionary(indexReader, "Title"));
                                spellChecker.IndexDictionary(new LuceneDictionary(indexReader, "Description"));

                                spellChecker.Close();
                            }
                        }
                    }
                }

                watch.Stop();

                return watch.ElapsedMilliseconds;
            }
            catch (Exception e)
            {
                _eventPublisher.Publish(new CreateSearchIndexesFailEvent(e));
                throw e;
            }
        }

        protected virtual Document MapPost(TblPosts post, TblLanguages language, PostType? postType)
        {
            var document = new Document();


            document.Add(new Field("ID", post.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field("PostType", postType?.ToString() ?? "", Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field("LanguageId", language.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field("LanguageCode", language.IsoCode, Field.Store.YES, Field.Index.NOT_ANALYZED));


            var titleField = new Field("Title",
                post.GetLocalized(p => p.Title, language.Id),
                Field.Store.YES,
                Field.Index.ANALYZED,
                Field.TermVector.WITH_POSITIONS_OFFSETS)
            {
                Boost = 100
            };
            document.Add(titleField);


            var postDescription = post.Descriptions.Where(p => p.AddToSearchEngineIndexes).Aggregate("",
                (current, description) =>
                    current + description.GetLocalized(p => p.HtmlDescription, language.Id).ConvertHtmlToText() +
                    Environment.NewLine + Environment.NewLine);
            var descriptionField = new Field("Description",
                postDescription,
                Field.Store.YES,
                Field.Index.ANALYZED,
                Field.TermVector.WITH_POSITIONS_OFFSETS)
            {
                Boost = 50
            };
            document.Add(descriptionField);


            var postTags = post.Tags.Aggregate("",
                (current, tag) => current + (tag.GetLocalized(p => p.Tag, language.Id) + ", "));
            var tagField = new Field("Tags",
                postTags,
                Field.Store.YES,
                Field.Index.ANALYZED,
                Field.TermVector.WITH_POSITIONS_OFFSETS)
            {
                Boost = 50
            };
            document.Add(tagField);

            var postCategories = post.Categories.Aggregate("",
                (current, cat) => current + (cat.Id + " , "));
            var categoriesField = new Field("Categories",
                postCategories,
                Field.Store.YES,
                Field.Index.ANALYZED);
            document.Add(categoriesField);


            var keywordsField = new Field("Keywords",
                post.GetLocalized(p => p.MetaKeyWords, language.Id),
                Field.Store.YES,
                Field.Index.ANALYZED,
                Field.TermVector.WITH_POSITIONS_OFFSETS)
            {
                Boost = 50
            };
            document.Add(keywordsField);


            return document;
        }
    }
}