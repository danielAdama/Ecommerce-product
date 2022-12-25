using Microsoft.AspNetCore.Identity;

namespace EcommerceMVC.Data
{
    public class EcommerceUser : IdentityUser<long>
    {
#nullable disable
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTimeOffset TimeCreated { get; set; }
        public DateTimeOffset TimeUpdated { get; set; }
    }
    public class ApplicationRole : IdentityRole<long> { }
}
