using System.ComponentModel.DataAnnotations;
using BackendApi.Jokes.Models;

namespace BackendApi.Topics.Models
{
    public class TopicModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public ICollection<JokeModel> Jokes { get; set; } = new List<JokeModel>();
    }
}