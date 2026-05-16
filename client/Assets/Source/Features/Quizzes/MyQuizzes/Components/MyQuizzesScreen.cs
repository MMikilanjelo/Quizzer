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
        private readonly ReactiveVisualElementList _quizzesSkeletonList;
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
                text = "Quizzes"
            };
            Header.AddToClassList("screen__title");

            _quizStateFiltersList = new ReactiveVisualElementList<QuizzesStateFilterItemViewModel>
            {
                ColumnGap = 8,
                Direction = FlexDirection.Row,
            };
            _quizStateFiltersList.AddToClassList("screen__filters-list");

            _quizzesList = new ReactiveVisualElementList<QuizItemViewModel>
            {
                ColumnGap = 16,
                Direction = FlexDirection.Column,
                BottomPadding = 104
            };

            _quizzesSkeletonList = new ReactiveVisualElementList
            {
                ColumnGap = 16,
                Direction = FlexDirection.Column,
                BottomPadding = 104
            };
            _noQuizzesFound = new NoQuizzesFound(iconProvider);

            screenContainer.Add(_quizStateFiltersList);
            screenContainer.Add(_quizzesSkeletonList);
            screenContainer.Add(_quizzesList);
            screenContainer.Add(_noQuizzesFound);

            Root.Add(screenContainer);
        }

        public void Initialize()
        {
            _disposable = new CompositeDisposable();

            _quizStateFiltersList.Bind(
                makeItem: () => new Chip(),
                bindItem: (chip, model) =>
                {
                    chip.Text = model.Name;
                    chip.Variant = model.IsSelected.Value ? Chip.ChipVariant.Filled : Chip.ChipVariant.Outline;
                    chip
                        .BindProperty(model.IsSelected, (c, isSelected) => { c.Variant = isSelected ? Chip.ChipVariant.Filled : Chip.ChipVariant.Outline; })
                        .AddTo(chip.Disposables);
                }
            );

            _quizStateFiltersList.Set(_viewModel.States);

            _quizStateFiltersList.ItemClicked
                .Subscribe(vm => _viewModel.SelectStateFilter.Execute(vm).Forget())
                .AddTo(_disposable);

            _quizzesList.Bind(
                makeItem: () => new QuizCard(),
                bindItem: (card, model) =>
                {
                    card.Bind(model)
                        .AddTo(card.Disposables);
                }
            );
            _quizzesList.Set(_viewModel.Quizzes);

            _quizzesSkeletonList.AddItem(new QuizCardSkeleton());
            _quizzesSkeletonList.AddItem(new QuizCardSkeleton());
            _quizzesSkeletonList.AddItem(new QuizCardSkeleton());

            _noQuizzesFound.Bind(_viewModel.EmptyState).AddTo(_disposable);

            _viewModel.Quizzes.Added.Subscribe(_ => UpdateViewState()).AddTo(_disposable);
            _viewModel.Quizzes.Removed.Subscribe(_ => UpdateViewState()).AddTo(_disposable);
            _viewModel.Quizzes.Cleared.Subscribe(_ => UpdateViewState()).AddTo(_disposable);
            _viewModel.IsLoading.Subscribe(_ => UpdateViewState()).AddTo(_disposable);

            UpdateViewState();

            _viewModel.FetchQuizzesCommand.Execute();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        private void UpdateViewState()
        {
            var isLoading = _viewModel.IsLoading.Value;
            var isEmpty = _viewModel.Quizzes.Count == 0;

            _quizzesSkeletonList.style.display = DisplayStyle.None;
            _quizzesList.style.display = DisplayStyle.None;
            _noQuizzesFound.style.display = DisplayStyle.None;

            if (isLoading)
            {
                _quizzesSkeletonList.style.display = DisplayStyle.Flex;
            }
            else if (isEmpty)
            {
                _noQuizzesFound.style.display = DisplayStyle.Flex;
            }
            else
            {
                _quizzesList.style.display = DisplayStyle.Flex;
            }
        }
    }
}