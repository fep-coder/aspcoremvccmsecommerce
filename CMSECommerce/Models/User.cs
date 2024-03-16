using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CMSECommerce.Models
{
    public class User
    {
        public string Id { get; set; }

        [Required, MinLength(2, ErrorMessage = "Minimum length is 2")]
        [DisplayName("Username")]
        public string UserName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password), Required, MinLength(4, ErrorMessage = "Minimum length is 4")]
        public string Password { get; set; }

        [DataType(DataType.Password), Required, MinLength(4, ErrorMessage = "Minimum length is 4")]
        [DisplayName("Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords must match!")]
        public string ConfirmPassword { get; set; }
    }
}
