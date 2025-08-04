using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BackendApi.Jokes.Models;

namespace BackendApi.Topics.Models
{
    public class TopicModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        // Navigation property for many-to-many relationship with Jokes
        public ICollection<JokeModel> Jokes { get; set; } = new List<JokeModel>();
    }
}