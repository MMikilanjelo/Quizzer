using Source.Shared.Components;

namespace Source.Features.Quizzes.FinishedQuiz.ViewModels
{
    public class KnowledgeAreaItemViewModel
    {
        public SpriteAtlasIconModel Icon { get; set; }
        public string TopicName { get; set; }
        public int StartPercentage { get; set; }
        public int EndPercentage { get; set; }
        public string StatusText { get; set; }
        public bool NeedsReview { get; set; }
    }
}