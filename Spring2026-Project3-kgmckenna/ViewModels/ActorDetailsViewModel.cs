using Spring2026_Project3_kgmckenna.Models;

namespace Spring2026_Project3_kgmckenna.ViewModels
{
    public class ActorDetailsViewModel
    {
        public Actor Actor { get; set; } = null!;

        public List<string> MovieTitles { get; set; } = new();

        public List<ReviewSentimentViewModel> TweetsWithSentiment { get; set; } = new();

        public string OverallSentiment { get; set; } = "";
    }
}