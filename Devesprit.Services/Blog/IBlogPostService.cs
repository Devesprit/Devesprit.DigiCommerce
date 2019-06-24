using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.Services.Posts;
using Devesprit.Services.SearchEngine;
using X.PagedList;

namespace Devesprit.Services.Blog
{
    public partial interface IBlogPostService: IPostService<TblBlogPosts>
    {}
}
