 
using Microsoft.AspNetCore.Identity;  

namespace InventoryApp.Data.Models  
{  
    public class ApplicationUser : IdentityUser  //inherits the built-in IdentityUser class from ASP.NET Core Identity and adds additional properties to store user-specific information such as blocking status, preferred language, and preferred theme.
    {  
        public bool IsBlocked { get; set; }  
        public string PreferredLanguage { get; set; } = "en";  
        public string PreferredTheme { get; set; } = "light";  
    }  
}  