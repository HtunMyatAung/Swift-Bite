using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityDemo.Models
{
    [Table("wishlist")]
    public class WishListModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("userId")]
        public string UserId { get; set; }
        [Column("itemId")]
        public int ItemId {  get; set; }
    }
}
