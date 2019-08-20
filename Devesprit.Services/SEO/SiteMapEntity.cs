using System;
using Devesprit.Data.Enums;

namespace Devesprit.Services.SEO
{
    public class SiteMapEntity
    {
        public string Title { get; set; }
        public int Id { get; set; }
        public string Slug { get; set; }
        public DateTime? LastUpDate { get; set; }
        public DateTime PublishDate { get; set; }
        public PostType? PostType { get; set; }
    }
}
