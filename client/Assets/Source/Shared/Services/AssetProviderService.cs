using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Source.Shared.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace Source.Shared.Services
{
    public interface IIconProvider
    {
        Sprite Get(SpriteAtlasIconModel model);
    }

    public interface IAssetProviderService
    {
        IIconProvider Icons { get; }
        UniTask<Result<T>> LoadAsset<T>(string key, string scope, CancellationToken cancellationToken = default);
        UniTask<Result> WarmupAssetsByLabel(string label, IProgress<float> progress = null, CancellationToken cancellationToken = default);
        void ReleaseScope(string scope);
        void ReleaseAsset(string key);
    }

    public class AssetProviderService : IAssetProviderService
    {
        public IIconProvider Icons { get; }

        private readonly Dictionary<string, Dictionary<string, AsyncOperationHandle>> _scopedHandles = new();

        private readonly ILoggingService _loggingService;

        public AssetProviderService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            Icons = new IconProvider(this, loggingService);
        }

        public async UniTask<Result<T>> LoadAsset<T>(string key, string scope, CancellationToken cancellationToken = default)
        {
            try
            {
                if (TryGetCachedHandle(key, out AsyncOperationHandle cachedHandle))
                {
                    if (!cachedHandle.IsDone)
                    {
                        await cachedHandle.ToUniTask(cancellationToken: cancellationToken);
                    }

                    if (cachedHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        RegisterHandleToScope(key, scope, cachedHandle);

                        return (T)cachedHandle.Result;
                    }
                }

                AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);

                RegisterHandleToScope(key, scope, handle);

                await handle.ToUniTask(cancellationToken: cancellationToken);

                if (handle.Status == AsyncOperationStatus.Failed)
                {
                    ReleaseAssetFromScope(key, scope);

                    _loggingService.Warning("Failed to load asset: {AssetKey} in scope {Scope}", key, scope);

                    return Error.Local("AssetLoadFailed");
                }

                return handle.Result;
            }
            catch (OperationCanceledException)
            {
                ReleaseAssetFromScope(key, scope);
                return Error.Cancelled;
            }
            catch (Exception ex)
            {
                ReleaseAssetFromScope(key, scope);
                _loggingService.Error(ex, "Exception loading asset: {AssetKey}", key);
                return Error.Local("AssetLoadException");
            }
        }

        public async UniTask<Result> WarmupAssetsByLabel(string label, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var sizeHandle = Addressables.GetDownloadSizeAsync(label);

                long size = await sizeHandle.ToUniTask(cancellationToken: cancellationToken);

                Addressables.Release(sizeHandle);

                if (size == 0)
                {
                    return Result.Success();
                }

                var downloadHandle = Addressables.DownloadDependenciesAsync(label, true);

                while (!downloadHandle.IsDone)
                {
                    progress?.Report(downloadHandle.PercentComplete);

                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                if (downloadHandle.Status == AsyncOperationStatus.Failed)
                {
                    return Error.Local("WarmupFailed");
                }

                return Result.Success();
            }
            catch (Exception)
            {
                return Error.Local("WarmupException");
            }
        }

        public void ReleaseScope(string scope)
        {
            if (!_scopedHandles.TryGetValue(scope, out var scopeDict)) return;

            foreach (var handle in scopeDict.Values)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }

            scopeDict.Clear();
            _scopedHandles.Remove(scope);
            _loggingService.Info("Released all assets in scope: {Scope}", scope);
        }

        public void ReleaseAsset(string key)
        {
            foreach (var scopeDict in _scopedHandles.Values)
            {
                if (!scopeDict.Remove(key, out var handle))
                {
                    continue;
                }

                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }

        private bool TryGetCachedAsset<T>(string key, out T asset)
        {
            if (TryGetCachedHandle(key, out var handle) && handle is { IsDone: true, Status: AsyncOperationStatus.Succeeded })
            {
                asset = (T)handle.Result;
                return true;
            }

            asset = default;
            return false;
        }

        private bool TryGetCachedHandle(string key, out AsyncOperationHandle handle)
        {
            foreach (var scopeDict in _scopedHandles.Values)
            {
                if (scopeDict.TryGetValue(key, out handle))
                {
                    return true;
                }
            }

            handle = default;

            return false;
        }

        private void RegisterHandleToScope(string key, string scope, AsyncOperationHandle handle)
        {
            if (!_scopedHandles.TryGetValue(scope, out var scopeDict))
            {
                scopeDict = new Dictionary<string, AsyncOperationHandle>();
                _scopedHandles[scope] = scopeDict;
            }

            scopeDict.TryAdd(key, handle);
        }

        private void ReleaseAssetFromScope(string key, string scope)
        {
            if (_scopedHandles.TryGetValue(scope, out var scopeDict))
            {
                if (scopeDict.Remove(key, out var handle) && handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }

        private class IconProvider : IIconProvider
        {
            private readonly AssetProviderService _assetProvider;
            private readonly ILoggingService _loggingService;

            public IconProvider(AssetProviderService assetProvider, ILoggingService loggingService)
            {
                _assetProvider = assetProvider;
                _loggingService = loggingService;
            }

            public Sprite Get(SpriteAtlasIconModel model)
            {
                return model == null ? null : Get(model.Name, model.Key);
            }

            public Sprite Get(string atlasName, string spriteKey)
            {
                if (string.IsNullOrEmpty(atlasName) || string.IsNullOrEmpty(spriteKey))
                {
                    return null;
                }

                if (_assetProvider.TryGetCachedAsset<SpriteAtlas>(atlasName, out var atlas))
                {
                    var sprite = atlas.GetSprite(spriteKey);

                    if (sprite)
                    {
                        return sprite;
                    }

                    _loggingService.Warning("Sprite '{SpriteKey}' not found in atlas '{AtlasName}'", spriteKey, atlasName);
                    return null;
                }

                _loggingService.Warning("Cannot get sprite. Atlas '{AtlasName}' has not been preloaded.", atlasName);
                return null;
            }
        }
    }
}