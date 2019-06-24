using System.Collections.Generic;
using Devesprit.Data.Domain;
using Devesprit.Data.Events;

namespace Devesprit.Services.Posts
{
    public partial class PostCategoriesUpdatedEvent: IEvent
    {
        public TblPosts Post { get; }
        public List<int> CategoriesList { get; }
        
        public PostCategoriesUpdatedEvent(TblPosts post, List<int> categoriesList)
        {
            Post = post;
            CategoriesList = categoriesList;
        }
    }
}