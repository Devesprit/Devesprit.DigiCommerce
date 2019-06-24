using System.Collections.Generic;
using Devesprit.Data.Domain;
using Devesprit.Data.Events;

namespace Devesprit.Services.Posts
{
    public partial class PostTagsUpdatedEvent: IEvent
    {
        public TblPosts Post { get; }
        public List<string> TagsList { get; }

        public PostTagsUpdatedEvent(TblPosts post, List<string> tagsList)
        {
            Post = post;
            TagsList = tagsList;
        }
    }
}