using System.Linq;
using Cysharp.Threading.Tasks;
using Source.Features.Quizzes.MyQuizzes.Models;
using Source.Shared.Reactive.Commands;
using Source.Shared.Reactive.Events;
using Source.Shared.Reactive.List;

namespace Source.Features.Quizzes.MyQuizzes.ViewModels
{
    public class QuizItemViewModel
    {
        public string Id { get; }
        public IReactiveProperty<string> Name { get; }
        public IReactiveProperty<string> Status { get; }
        public IReactiveProperty<int> ProgressCount { get; }
        public IReactiveProperty<int> TotalCount { get; }
        public IReactiveProperty<float> NormalizedProgress { get; }
        public IReadOnlyReactiveList<string> Topics { get; set; }
        public ICommand ActionCommand { get; private set; }

        public QuizItemViewModel(QuizModel model, ICommand<QuizItemViewModel> continueCommand)
        {
            Id = model.Id;
            Name = new ReactiveProperty<string>(model.Name);
            Status = new ReactiveProperty<string>(model.Status);
            ProgressCount = new ReactiveProperty<int>(model.AnsweredCount);
            TotalCount = new ReactiveProperty<int>(model.QuestionCount);

            var progress = model.QuestionCount > 0 ? (float)model.AnsweredCount / model.QuestionCount : 0;
            NormalizedProgress = new ReactiveProperty<float>(progress);
            Topics = new ReactiveList<string>(model.Topics.Take(3).ToArray());

            ActionCommand = SyncCommand.Create(() => continueCommand.Execute(this).Forget());
        }
    }
}