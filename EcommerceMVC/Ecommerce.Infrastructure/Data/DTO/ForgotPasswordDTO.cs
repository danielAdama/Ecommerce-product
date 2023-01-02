using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Data.DTO
{
    public class ForgotPasswordDTO
    {
#nullable disable
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
    }
}
