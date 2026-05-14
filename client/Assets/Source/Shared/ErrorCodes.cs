namespace Source.Shared
{
    public static class ErrorCodes
    {
        public const string NetworkTimeout = "Network.Timeout";
        public const string NetworkConnection = "Network.Connection";
        public const string NetworkUnknown = "Network.Unknown";
        public const string OperationCancelled = "OperationCancelled";

        public const string SerializationFailure = "Serialization.Failed";
        public const string DeserializationFailure = "Serialization.DeserializationFailed";

        public const string AssetLoadFailed = "Asset.LoadFailed";
        public const string AssetDownloadFailed = "Asset.DownloadFailed";

        public const string Unauthorized = "Http.Unauthorized";
        public const string Forbidden = "Http.Forbidden";
        public const string ResourceNotFound = "Http.NotFound";
        public const string ServerConflict = "Http.Conflict";
        public const string ServerError = "Http.ServerError";

        public const string EncryptionFailed = "Security.EncryptionFailed";
        public const string DecryptionFailed = "Security.DecryptionFailed";

        public const string StorageSaveFailed = "Storage.SaveFailed";
        public const string StorageAccessFailed = "Storage.AccessFailed";
        public const string StorageDataNotFound = "Storage.NotFound";

        public static string HttpError(int statusCode) => $"ERR_HTTP_{statusCode}";
    }
}