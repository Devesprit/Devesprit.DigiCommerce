using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using Devesprit.Data.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_Users")]
    public partial class TblUsers:IdentityUser
    {
        [MaxLength(100)]
        public string FirstName { get; set; }
        [MaxLength(100)]
        public string LastName { get; set; }
        public string Avatar { get; set; }
        public int? UserCountryId { get; set; }
        public virtual TblCountries UserCountry { get; set; }
        public int? UserGroupId { get; set; }
        public virtual TblUserGroups UserGroup { get; set; }
        public int? MaxDownloadCount { get; set; }
        public bool UserDisabled { get; set; }
        public string DisableReason { get; set; }
        public string UserLatestIP { get; set; }
        public DateTime? UserLastLoginDate { get; set; }
        public TimePeriodType? MaxDownloadPeriodType { get; set; }
        public int? RoleId { get; set; }
        public virtual TblUserRoles Role { get; set; }

        public DateTime? SubscriptionDate { get; set; }
        public DateTime? SubscriptionExpireDate { get; set; }
        public DateTime RegisterDate { get; set; }
        public virtual ICollection<TblInvoices> Invoices { get; set; }
        public virtual ICollection<TblUserLikes> Likes { get; set; }
        public virtual ICollection<TblUserWishlist> Wishlist { get; set; }
        public virtual ICollection<TblProductDownloadsLog> DownloadsLog { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<TblUsers> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}
