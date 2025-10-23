using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityDemo.Models
{
    public class ApplicationUser:IdentityUser
    {
       
        [Column("shopid")]
        public int ShopId { get; set; }
        public string? Role { get; set; }
        public string? Address {  get; set; }
        public int? Forgot {  get; set; }
        [Column("user_image")]
        public string UserImageName { get; set; }
        [Column("delete")]
        public int Deleted { get; set; }
    }
}
