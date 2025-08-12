using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BackendApi.Users.Models;
using BackendApi.Topics.Models;

namespace BackendApi.Jokes.Models
{
    public class JokeModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [ForeignKey("Author")]
        public int AuthorId { get; set; }

        public UserModel? Author { get; set; }

        [Required]
        [MaxLength(50)]
        public string Origin { get; set; } = "Local";

        public ICollection<TopicModel> Topics { get; set; } = new List<TopicModel>();
    }
}