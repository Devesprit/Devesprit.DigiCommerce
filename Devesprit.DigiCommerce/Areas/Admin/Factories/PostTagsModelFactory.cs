﻿using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class PostTagsModelFactory : IPostTagsModelFactory
    {
        public virtual async Task<PostTagModel> PreparePostTagModelAsync(TblPostTags tag)
        {
            PostTagModel result;
            if (tag == null)
            {
                result = new PostTagModel();
            }
            else
            {
                result = tag.Adapt<PostTagModel>();
                await tag.LoadAllLocalizedStringsToModelAsync(result);
            }

            return result;
        }

        public virtual TblPostTags PrepareTblPostTags(PostTagModel tag)
        {
            var result = tag.Adapt<TblPostTags>();
            return result;
        }
    }
}