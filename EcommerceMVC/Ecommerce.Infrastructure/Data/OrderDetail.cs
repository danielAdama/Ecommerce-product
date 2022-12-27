using EcommerceMVC.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Data
{
    public class OrderDetail
    {
#nullable disable
        public long Id { get; set; }
        public long OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        [ValidateNever]
        public OrderHeader OrderHeader { get; set; }
        public long ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        [ValidateNever]
        public Product Product { get; set; }
        public int Count { get; set; }
        public double Price { get; set; }
    }
}
