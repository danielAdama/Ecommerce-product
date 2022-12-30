using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMVC.Data
{
	public class Category : BaseEntity
	{
#nullable disable
		public string Name { get; set; }
		[DisplayName("Display Order")]
		public int DisplayOrder { get; set; }
	}

	public class Product : BaseEntity
	{
#nullable disable
		public string Name { get; set; }
		public string Description { get; set; }
		public double Price { get; set; }
        [DisplayName("Image")]
        public string ImageUrl { get; set; }

		[Display(Name ="Category")]
		public long CategoryId { get; set; }
		public Category Category { get; set; }

		// One-to-one relationship: Each item belong to a category
		//[ForeignKey("CategoryId")]
		//public ProductCategoryEnum ProductCategory { get; set; } //FK

		//public ICollection<Seller> Sellers { get; set; }

	}
}
