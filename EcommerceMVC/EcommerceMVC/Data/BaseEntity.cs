using System.ComponentModel.DataAnnotations;

namespace EcommerceMVC.Data
{
    public class BaseEntity
    {
		[Key]
        public long Id { get; set; }
		public DateTimeOffset TimeCreated { get; set; } = DateTimeOffset.UtcNow;
		public DateTimeOffset TimeUpdated { get; set; }
	}
}
