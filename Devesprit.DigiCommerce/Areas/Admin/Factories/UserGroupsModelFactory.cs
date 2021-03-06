﻿using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class UserGroupsModelFactory : IUserGroupsModelFactory
    {
        public virtual async Task<UserGroupModel> PrepareUserGroupModelAsync(TblUserGroups userGroup)
        {
            UserGroupModel result;
            if (userGroup == null)
            {
                result = new UserGroupModel();
            }
            else
            {
                result = userGroup.Adapt<UserGroupModel>();
                await userGroup.LoadAllLocalizedStringsToModelAsync(result);
            }
            return result;
        }

        public virtual TblUserGroups PrepareTblUserGroup(UserGroupModel userGroup)
        {
            var result = userGroup.Adapt<TblUserGroups>();
            return result;
        }
    }
}