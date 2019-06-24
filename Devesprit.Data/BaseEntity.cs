using System.ComponentModel.DataAnnotations;

namespace Devesprit.Data
{
    /// <summary>
    /// Base class for entities
    /// </summary>
    public abstract partial class BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [Key]
        public int Id { get; set; }
    }
}
