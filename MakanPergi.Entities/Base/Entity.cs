using Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace MakanPergi.Entities.Base
{
    public class Entity : BaseEntity
    {
        public DateTimeOffset DateTimeCreated { get; set; }
        public DateTimeOffset? DateTimeUpdated { get; set; }
        [StringLength(50)]
        public string CreatedBy { get; set; } = DefaultValue.Unknown;
        [StringLength(50)]
        public string? UpdatedBy { get; set; }
    }
}
