using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityDemo.Models
{
    [Table("location")]
    public class LocationModel
    {
        [Key]
        [Column("id")]
        public int loction_id {  get; set; }
        [Column("shop_id")]
        public int shop_id {  get; set; }
        [Column("latitude")]
        public double Latitude { get; set; }       

        [Column("Longitude")]
        public double Longitude { get; set; }

    }
}
