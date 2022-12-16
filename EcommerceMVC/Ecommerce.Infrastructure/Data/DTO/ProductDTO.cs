using EcommerceMVC.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Ecommerce.Infrastructure.Data.DTO
{
	public class ProductDTO
	{
#nullable disable
		public Product product { get; set; }
		public IEnumerable<SelectListItem> CategoryList { get; set; }
	}
}
