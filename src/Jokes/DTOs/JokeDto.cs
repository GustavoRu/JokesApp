namespace BackendApi.Jokes.DTOs
{
    public class JokeDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public string Origin { get; set; } = string.Empty;
        public List<string> Topics { get; set; } = new List<string>();
    }

    public class CreateJokeDto
    {
        public string Text { get; set; } = string.Empty;
        public List<int> TopicIds { get; set; } = new List<int>();
    }

    public class UpdateJokeDto
    {
        public string Text { get; set; } = string.Empty;
        public List<int> TopicIds { get; set; } = new List<int>();
    }

    public class JokePairDto
    {
        public string Chuck { get; set; } = string.Empty;
        public string Dad { get; set; } = string.Empty;
        public string Combined { get; set; } = string.Empty;
    }

    public class ExternalJokeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}