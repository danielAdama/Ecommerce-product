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
		public string ImageUrl { get; set; }

		[Display(Name ="Category")]
		public long CategoryId { get; set; }
		public Category Category { get; set; }

		// One-to-one relationship: Each item belong to a category
		//[ForeignKey("CategoryId")]
		//public ProductCategoryEnum ProductCategory { get; set; } //FK

		//public ICollection<Seller> Sellers { get; set; }

	}

	//    public class Order : BaseEntity
	//    {
	//#nullable disable
	//        public Guid TrackingId { get; set; }


	//        //[ForeignKey("EcommerceUser")]
	//        // A user can have many orders
	//        public string UserId { get; set; }
	//        //public long UserId { get; set; } // FK UserId for the user that orders for product
	//        //public EcommerceUser? User { get; set; }


	//        // One-to-many relationship: there are items in different cart
	//        //public ICollection<ShoppingCartItem> CartItems { get; set; } // do not uncomment this
	//        public string Country { get; set; }
	//        public string PhoneNumber { get; set; }
	//        public ICollection<OrderItem> OrderItems { get; set; } // list of items in an order
	//    }

	//    public class OrderItem : BaseEntity
	//    {
	//#nullable disable
	//        public int Quantity { get; set; }
	//        public double Price { get; set; }



	//        [ForeignKey("ProductId")]
	//        public Product Product { get; set; }
	//        public long ProductId { get; set; } //FK


	//        [ForeignKey("OrderId")]
	//        public Order Order { get; set; }
	//        public long OrderId { get; set; } //FK


	//    }
}
