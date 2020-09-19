using Microsoft.AspNetCore.Identity;

namespace ASP_SampleAPI.Models
{
    public class User : IdentityUser
    {
        public string Role { get; set; }
    }
}