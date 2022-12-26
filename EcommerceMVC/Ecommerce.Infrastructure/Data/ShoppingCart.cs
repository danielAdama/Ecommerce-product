using EcommerceMVC.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Data
{
    public class ShoppingCart
    {
#nullable disable
        public long Id { get; set; }
        public long ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }
        public IEnumerable<Product> Products { get; set; }
        [Range(1, 1000, ErrorMessage = "Please enter a value between 1 and 1000")]
        public int Count { get; set; }
        public long EcommerceUserId { get; set; }
        [ForeignKey("EcommerceUserId")]
        [ValidateNever]
        public EcommerceUser EcommerceUser { get; set; }
        [NotMapped]
        public double Price { get; set; }
    }
}
