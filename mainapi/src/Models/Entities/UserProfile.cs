using System.ComponentModel.DataAnnotations.Schema;

namespace LunkvayAPI.src.Models.Entities
{
    [Table("profiles")]
    public class UserProfile
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("about")]
        public string? About { get; set; }



        [ForeignKey("UserId")]
        public virtual User? User { get; set; }


        public static UserProfile Create(Guid userId, string about, string status)
        {
            return new() 
            { 
                UserId = userId,
                About = about,
                Status = status
            };
        }
    }
}
