using Spring2026_Project3_kgmckenna.Models;

namespace Spring2026_Project3_kgmckenna.ViewModels
{
    public class MovieDetailsViewModel
    {
        public Movie Movie { get; set; } = null!;
        public List<string> ActorNames { get; set; } = new();
        public List<ReviewSentimentViewModel> ReviewsWithSentiment { get; set; } = new();
        public string OverallSentiment { get; set; } = "";
    }
}