using System.Collections.Generic;
using Source.Features.MyProfile.ViewModels;
using Source.Shared.Components.Elements.Label;
using Source.Shared.Components.Headers.FlowHeader;
using Source.Shared.Components.Screens;
using Source.Shared.Components.Skeleton;
using Source.Shared.Reactive;
using Source.Shared.Reactive.Disposables;
using Source.Shared.Reactive.List;
using Source.Shared.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Features.MyProfile.Components
{
    public class MyProfileScreen : IScreenView, IScreenWithHeaderView
    {
        public VisualElement Root { get; }
        public VisualElement Header { get; }

        private readonly IMyProfileScreenViewModel _viewModel;
        private CompositeDisposable _disposable;

        private readonly VisualElement _realContentContainer;
        private readonly VisualElement _skeletonContainer;

        private readonly ProfileStatBox _scoreBox;
        private readonly ProfileStatBox _quizzesBox;
        private readonly ProfileStatBox _perfectBox;
        private readonly ProfileStatBox _streakBox;
        private readonly VisualElement _strengthsContainer;
        private readonly VisualElement _focusContainer;

        public MyProfileScreen(TemplateContainer visualElement, IMyProfileScreenViewModel viewModel, IIconProvider iconProvider)
        {
            _viewModel = viewModel;
            Root = visualElement;

            var screenContainer = new ScrollView(ScrollViewMode.Vertical)
            {
                style = { paddingBottom = 104 }
            };
            screenContainer.AddToClassList("screen__container");

            Header = new CustomLabel
            {
                Variant = CustomLabel.TextVariant.Title2,
                Weight = CustomLabel.FontWeight.Bold,
                Alignment = TextAnchor.MiddleLeft,
                text = "My Profile"
            };
            Header.AddToClassList("screen__title");

            _skeletonContainer = new VisualElement();
            _skeletonContainer.Add(new MyProfileStatsGridSkeleton());
            _skeletonContainer.Add(new MyProfileMasterySectionSkeleton());
            _skeletonContainer.Add(new MyProfileMasterySectionSkeleton(2));

            _realContentContainer = new VisualElement
            {
                style =
                {
                    display = DisplayStyle.None
                }
            };

            var statsGrid = new VisualElement();
            statsGrid.AddToClassList("my-profile__stats-grid");

            _scoreBox = new ProfileStatBox("Avg Score");
            _quizzesBox = new ProfileStatBox("Total Quizzes");
            _perfectBox = new ProfileStatBox("Perfect Runs");
            _streakBox = new ProfileStatBox("Day Streak");

            statsGrid.Add(_scoreBox);
            statsGrid.Add(_quizzesBox);
            statsGrid.Add(_perfectBox);
            statsGrid.Add(_streakBox);
            _realContentContainer.Add(statsGrid);

            var strengthsSection = new VisualElement();
            strengthsSection.AddToClassList("my-profile__section");

            var strengthsTitle = new CustomLabel
            {
                text = "Top Strengths",
                Variant = CustomLabel.TextVariant.Large,
                Weight = CustomLabel.FontWeight.Bold
            };
            strengthsTitle.AddToClassList("my-profile__section-title");
            _strengthsContainer = new VisualElement();

            strengthsSection.Add(strengthsTitle);
            strengthsSection.Add(_strengthsContainer);
            _realContentContainer.Add(strengthsSection);

            var focusSection = new VisualElement();
            focusSection.AddToClassList("my-profile__section");

            var focusTitle = new CustomLabel
            {
                text = "Focus Areas",
                Variant = CustomLabel.TextVariant.Large,
                Weight = CustomLabel.FontWeight.Bold
            };
            focusTitle.AddToClassList("my-profile__section-title");
            _focusContainer = new VisualElement();

            focusSection.Add(focusTitle);
            focusSection.Add(_focusContainer);
            _realContentContainer.Add(focusSection);

            screenContainer.Add(_skeletonContainer);
            screenContainer.Add(_realContentContainer);
            Root.Add(screenContainer);
        }


        public void Initialize()
        {
            _disposable = new CompositeDisposable();

            _viewModel.AverageScore.Subscribe(score => _scoreBox.SetValue($"{score:0.0}%")).AddTo(_disposable);
            _viewModel.TotalQuizzes.Subscribe(total => _quizzesBox.SetValue(total.ToString())).AddTo(_disposable);
            _viewModel.PerfectQuizzes.Subscribe(perfect => _perfectBox.SetValue(perfect.ToString())).AddTo(_disposable);
            _viewModel.StreakDays.Subscribe(streak => _streakBox.SetValue($"{streak}")).AddTo(_disposable);

            RefreshStrengths();
            _viewModel.TopStrengths.Added.Subscribe(_ => RefreshStrengths()).AddTo(_disposable);
            _viewModel.TopStrengths.Removed.Subscribe(_ => RefreshStrengths()).AddTo(_disposable);
            _viewModel.TopStrengths.Cleared.Subscribe(_ => RefreshStrengths()).AddTo(_disposable);

            RefreshFocusAreas();
            _viewModel.FocusAreas.Added.Subscribe(_ => RefreshFocusAreas()).AddTo(_disposable);
            _viewModel.FocusAreas.Removed.Subscribe(_ => RefreshFocusAreas()).AddTo(_disposable);
            _viewModel.FocusAreas.Cleared.Subscribe(_ => RefreshFocusAreas()).AddTo(_disposable);

            _viewModel.IsLoading
                .Subscribe(isLoading =>
                {
                    _skeletonContainer.style.display = isLoading ? DisplayStyle.Flex : DisplayStyle.None;
                    _realContentContainer.style.display = isLoading ? DisplayStyle.None : DisplayStyle.Flex;
                })
                .AddTo(_disposable);

            _scoreBox.SetValue($"{_viewModel.AverageScore.Value:0.0}%");
            _quizzesBox.SetValue(_viewModel.TotalQuizzes.Value.ToString());
            _perfectBox.SetValue(_viewModel.PerfectQuizzes.Value.ToString());
            _streakBox.SetValue($"{_viewModel.StreakDays.Value}");

            _skeletonContainer.style.display = _viewModel.IsLoading.Value ? DisplayStyle.Flex : DisplayStyle.None;
            _realContentContainer.style.display = _viewModel.IsLoading.Value ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void Dispose() =>
            _disposable?.Dispose();

        private void RefreshStrengths()
        {
            _strengthsContainer.Clear();
            foreach (var item in _viewModel.TopStrengths)
            {
                _strengthsContainer.Add(new MasterySkillRow(item, isFocusArea: false));
            }
        }

        private void RefreshFocusAreas()
        {
            _focusContainer.Clear();
            foreach (var item in _viewModel.FocusAreas)
            {
                _focusContainer.Add(new MasterySkillRow(item, isFocusArea: true));
            }
        }
    }

    public class MyProfileStatsGridSkeleton : VisualElement
    {
        public MyProfileStatsGridSkeleton()
        {
            AddToClassList("my-profile__stats-grid");
            Add(new ProfileStatBoxSkeleton());
            Add(new ProfileStatBoxSkeleton());
            Add(new ProfileStatBoxSkeleton());
            Add(new ProfileStatBoxSkeleton());
        }
    }

    public class MyProfileMasterySectionSkeleton : VisualElement
    {
        public MyProfileMasterySectionSkeleton(int rowCount = 3)
        {
            AddToClassList("my-profile__section");
            var titleSkeleton = new Skeleton { Variant = Skeleton.SkeletonVariant.Large };
            titleSkeleton.AddToClassList("my-profile__section-title");
            titleSkeleton.style.width = Length.Percent(40);
            titleSkeleton.style.marginBottom = 16;

            Add(titleSkeleton);

            for (var i = 0; i < rowCount; i++)
            {
                var rowSkeleton = new MasterySkillRowSkeleton();

                if (i < rowCount - 1)
                {
                    rowSkeleton.style.marginBottom = 16;
                }

                Add(rowSkeleton);
            }
        }
    }
}