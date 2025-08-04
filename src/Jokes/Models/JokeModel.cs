using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BackendApi.Users.Models;
using BackendApi.Topics.Models;

namespace BackendApi.Jokes.Models
{
    public class JokeModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int AuthorId { get; set; }

        [Required]
        [StringLength(50)]
        public string Source { get; set; } = "Local"; // "Chuck Norris", "Dad Joke", "Local"

        // Navigation properties
        [ForeignKey("AuthorId")]
        public UserModel Author { get; set; } = null!;

        public ICollection<TopicModel> Topics { get; set; } = new List<TopicModel>();
    }
}