using EcommerceMVC.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Utilities
{
    public static class IdentityHelper
    {

        public static long GetUserId(IIdentity identity)
        {
            var claimsIdentity = (ClaimsIdentity)identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            return Convert.ToInt64(claim.Value);
        }
    }
}
