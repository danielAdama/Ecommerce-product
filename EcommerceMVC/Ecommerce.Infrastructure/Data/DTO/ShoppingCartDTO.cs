using EcommerceMVC.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Data.DTO
{
    public class ShoppingCartDTO
    {
#nullable disable
		public IEnumerable<ShoppingCart> ListCart { get; set; }
	}
}
