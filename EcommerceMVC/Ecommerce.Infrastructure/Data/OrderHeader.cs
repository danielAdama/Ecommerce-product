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
	public class OrderHeader
	{
#nullable disable
		public long Id { get; set; }
		public long EcommerceUserId { get; set; }
		[ForeignKey(nameof(EcommerceUserId))]
		[ValidateNever]
		public EcommerceUser EcommerceUser { get; set; }
		public DateTimeOffset OrderDate { get; set; }
		public DateTimeOffset ShippingDate { get; set; }
		public double OrderTotal { get; set; }
		/* We will take the payments 30 days after delivery */
		public string? OrderStatus { get; set; }
		public string? PaymentStatus { get; set; }
		public string? TrackingNumber { get; set; }
		public string? Carrier { get; set; }
        public DateTimeOffset PaymentDate { get; set; }
        public DateTimeOffset? PaymentDueDate { get; set; }
        /* Stripe */
		public string? SessionId { get; set; }
		public string? PaymentIntentId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string StreetAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string PhoneNumber { get; set; }

    }
}
