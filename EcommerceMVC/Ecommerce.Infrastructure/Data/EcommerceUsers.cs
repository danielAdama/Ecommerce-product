using Microsoft.AspNetCore.Identity;

namespace EcommerceMVC.Data
{
    public class EcommerceUser : IdentityUser<long>
    {
#nullable disable
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public long RoleId { get; set; }
        //public long? CountryId { get; set; }
        public DateTimeOffset TimeCreated { get; set; }
        public DateTimeOffset TimeUpdated { get; set; }
    }
    public class ApplicationRole : IdentityRole<long> { }
}
