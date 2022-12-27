using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Utilities
{
    public class StripeSettings
    {
#nullable disable
        public string SecretKey { get; set; }
        public string PublishableKey { get; set; }

    }
}
