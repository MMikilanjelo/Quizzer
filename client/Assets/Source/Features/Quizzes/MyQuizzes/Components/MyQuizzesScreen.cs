using Cysharp.Threading.Tasks;
using Source.Features.Quizzes.MyQuizzes.ViewModels;
using Source.Shared.Components.Elements.Chip;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Components.List;
using Source.Shared.Components.Screens;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.Quizzes.MyQuizzes.Components
{
    public class MyQuizzesScreen : IScreenWithHeaderView, IScreenView
    {
        public VisualElement Root { get; }
        public VisualElement Header { get; }

        private readonly ReactiveVisualElementList<QuizzesStateFilterItemViewModel> _quizStateFiltersList;
        private readonly ReactiveVisualElementList<QuizItemViewModel> _quizzesList;
        private readonly NoQuizzesFound _noQuizzesFound;

        private readonly IMyQuizzesScreenViewModel _viewModel;
        private CompositeDisposable _disposable;

        public MyQuizzesScreen(TemplateContainer visualElement, IMyQuizzesScreenViewModel viewModel, IIconProvider iconProvider)
        {
            _viewModel = viewModel;

            Root = visualElement;

            var screenContainer = new VisualElement { name = "Container" };
            screenContainer.AddToClassList("screen__container");

            Header = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Title2,
                Weight = CustomLabel.FontWeight.Bold,
                Alignment = TextAnchor.MiddleLeft,
                text = "Quizzes",
                style = { marginBottom = 24, marginLeft = 24 }
            };

            _quizStateFiltersList = new ReactiveVisualElementList<QuizzesStateFilterItemViewModel>
            {
                ColumnGap = 8,
                Direction = FlexDirection.Row,
                style =
                {
                    flexGrow = 0,
                    flexShrink = 0,
                    marginBottom = 24
                }
            };

            _quizzesList = new ReactiveVisualElementList<QuizItemViewModel>
            {
                ColumnGap = 16,
                Direction = FlexDirection.Column,
                BottomPadding = 104
            };
            _noQuizzesFound = new NoQuizzesFound(iconProvider);

            screenContainer.Add(_quizStateFiltersList);
            screenContainer.Add(_quizzesList);
            screenContainer.Add(_noQuizzesFound);

            Root.Add(screenContainer);
        }

        public void Initialize()
        {
            _disposable = new CompositeDisposable();

            _quizStateFiltersList.Bind(
                makeItem: () => new Chip(),
                bindItem: (chip, model, disposable) =>
                {
                    chip.Text = model.Name;
                    chip.Variant = model.IsSelected.Value ? Chip.ChipVariant.Filled : Chip.ChipVariant.Outline;
                    chip
                        .BindProperty(model.IsSelected, (c, isSelected) => { c.Variant = isSelected ? Chip.ChipVariant.Filled : Chip.ChipVariant.Outline; })
                        .AddTo(disposable);
                }
            );

            _quizStateFiltersList.Set(_viewModel.States);

            _quizStateFiltersList.ItemClicked
                .Subscribe(vm => _viewModel.SelectStateFilter.Execute(vm).Forget())
                .AddTo(_disposable);

            _quizzesList.Bind(
                makeItem: () => new QuizCard(),
                bindItem: (card, model, disposable) => { card.Bind(model).AddTo(disposable); });

            _quizzesList.Set(_viewModel.Quizzes);

            _noQuizzesFound.Bind(_viewModel.EmptyState).AddTo(_disposable);

            _viewModel.Quizzes.Added.Subscribe(_ => OnQuizzesListModified()).AddTo(_disposable);
            _viewModel.Quizzes.Removed.Subscribe(_ => OnQuizzesListModified()).AddTo(_disposable);
            _viewModel.Quizzes.Cleared.Subscribe(_ => OnQuizzesListModified()).AddTo(_disposable);

            OnQuizzesListModified();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        private void OnQuizzesListModified()
        {
            var isEmpty = _viewModel.Quizzes.Count == 0;

            if (isEmpty)
            {
                _noQuizzesFound.style.display = DisplayStyle.Flex;
                _quizzesList.style.display = DisplayStyle.None;
            }
            else
            {
                _noQuizzesFound.style.display = DisplayStyle.None;
                _quizzesList.style.display = DisplayStyle.Flex;
            }
        }
    }
}