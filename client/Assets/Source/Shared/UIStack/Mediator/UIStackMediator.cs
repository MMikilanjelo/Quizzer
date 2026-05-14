using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.Shared.Components;
using Source.Shared.Components.Dialogs;
using Source.Shared.Components.Overlays;
using Source.Shared.Components.Screens;
using Source.Shared.Extensions;
using Source.Shared.Reactive.Commands;
using Source.Shared.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Shared.UIStack.Mediator
{
    public class UIStackMediator : IUIStackMediator
    {
        private readonly IAssetProviderService _assetProviderService;

        private Components.UIStack _stack;

        private readonly Stack<IScreenView> _screens = new();
        private readonly Stack<IDialogView> _dialogs = new();
        private readonly Stack<IOverlayView> _overlays = new();

        public UIStackMediator(IAssetProviderService assetProviderService)
        {
            _assetProviderService = assetProviderService;
        }

        public async UniTask<Result> CreateUIStack(string scope, CancellationToken ct)
        {
            return await _assetProviderService
                .LoadAsset<VisualTreeAsset>("UIStack", scope, ct)
                .Bind(async layout => await _assetProviderService
                    .LoadAsset<PanelSettings>("MobilePanelSettings", scope, ct)
                    .Map(settings => (layout, settings))
                )
                .Map(data =>
                {
                    var rootGo = new GameObject("UI_Root");
                    Object.DontDestroyOnLoad(rootGo);

                    var uiDoc = rootGo.AddComponent<UIDocument>();
                    uiDoc.panelSettings = data.settings;
                    uiDoc.visualTreeAsset = data.layout;

                    _stack = new Components.UIStack(uiDoc);

                    return Result.Success();
                });
        }

        public void PopAll()
        {
            PopAllScreens();
            PopAllDialogs();
            PopAllOverlays();
        }

        public void Push(IScreenView view)
        {
            if (_screens.TryPeek(out var currentScreen))
            {
                _stack.ScreensLayer.Remove(currentScreen.Root);

                if (currentScreen is IScreenWithHeaderView { Header: not null } oldHeaderView)
                    _stack.HeaderLayer.Remove(oldHeaderView.Header);

                if (currentScreen is IScreenWithFooterView { Footer: not null } oldFooterView)
                    _stack.FooterLayer.Remove(oldFooterView.Footer);
            }

            _stack.ScreensLayer.Add(view.Root);

            if (view is IScreenWithHeaderView { Header: not null } newHeaderView)
                _stack.HeaderLayer.Add(newHeaderView.Header);

            if (view is IScreenWithFooterView { Footer: not null } newFooterView)
                _stack.FooterLayer.Add(newFooterView.Footer);

            view.Initialize();

            _screens.Push(view);
        }

        public void PopScreen()
        {
            if (_screens.Count == 0)
            {
                return;
            }

            var view = _screens.Pop();

            _stack.ScreensLayer.Remove(view.Root);

            if (view is IScreenWithHeaderView { Header: not null } poppedHeaderView)
                _stack.HeaderLayer.Remove(poppedHeaderView.Header);

            if (view is IScreenWithFooterView { Footer: not null } poppedFooterView)
                _stack.FooterLayer.Remove(poppedFooterView.Footer);

            view.Dispose();

            if (_screens.TryPeek(out var previousScreen))
            {
                _stack.ScreensLayer.Add(previousScreen.Root);

                if (previousScreen is IScreenWithHeaderView { Header: not null } prevHeaderView)
                    _stack.HeaderLayer.Add(prevHeaderView.Header);

                if (previousScreen is IScreenWithFooterView { Footer: not null } prevFooterView)
                    _stack.FooterLayer.Add(prevFooterView.Footer);

                previousScreen.Initialize();
            }
        }

        public void PopAllScreens()
        {
            while (_screens.Count > 0)
            {
                PopScreen();
            }
        }

        public void Push(IDialogView view)
        {
            _dialogs.Push(view);
            _stack.ModalsLayer.Add(view.Root);
            _stack.BackdropOverlay.Show();
        }

        public void PopDialog()
        {
            if (_dialogs.Count == 0) return;

            var view = _dialogs.Pop();
            _stack.ModalsLayer.Remove(view.Root);
            view.Dispose();

            if (_dialogs.Count == 0)
            {
                _stack.BackdropOverlay.Hide();
            }
        }

        public void PopAllDialogs()
        {
            while (_dialogs.Count > 0)
            {
                PopDialog();
            }
        }

        public void Push(IOverlayView view)
        {
            _overlays.Push(view);
            _stack.OverlaysLayer.Add(view.Root);
            view.Initialize();
        }

        public void PopOverlay()
        {
            if (_overlays.Count == 0) return;

            var view = _overlays.Pop();
            _stack.OverlaysLayer.Remove(view.Root);
            view.Dispose();
        }

        public void PopAllOverlays()
        {
            while (_overlays.Count > 0)
            {
                PopOverlay();
            }
        }

        public void ShowBackdrop() =>
            _stack.BackdropOverlay.Show();

        public void HideBackdrop() =>
            _stack.BackdropOverlay.Hide();

        public void AddToFooter(IView view) =>
            _stack.FooterLayer.Add(view.Root);

        public void RemoveFromFooter(IView view) =>
            _stack.FooterLayer.Remove(view.Root);

        public void AddToOverlay(IView view) =>
            _stack.OverlaysLayer.Add(view.Root);
    }
}