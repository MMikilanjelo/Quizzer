using System;
using Source.Shared;
using Source.Shared.Extensions;
using Source.Shared.Persistence;
using Source.Shared.Services;
using UnityEngine.Device;

namespace Source.Features.Auth.Models
{
    internal class TokensStore : ITokensStore
    {
        private readonly IRepository<TokenModel, int> _db;
        private readonly IEncryptionService _encryptionService;
        private const int TokenId = 1;

        public TokensStore(
            IRepository<TokenModel, int> db,
            IEncryptionService encryptionService
        )
        {
            _db = db;
            _encryptionService = encryptionService;
        }

        private static string GetDeviceSecret() =>
            SystemInfo.deviceUniqueIdentifier;

        public Result SaveTokens(string accessToken, string refreshToken)
        {
            var secret = GetDeviceSecret();

            return _encryptionService
                .Encrypt(accessToken, secret)
                .Bind(encryptedAccess => _encryptionService
                    .Encrypt(refreshToken, secret)
                    .Map(encryptedRefresh => new TokenModel
                    {
                        Id = TokenId,
                        EncryptedAccess = encryptedAccess,
                        EncryptedRefresh = encryptedRefresh
                    })
                )
                .Bind(doc => _db.Upsert(doc));
        }

        public Result<string> GetAccessToken() =>
            GetDecryptedToken(token => token.EncryptedAccess);

        public Result<string> GetRefreshToken() =>
            GetDecryptedToken(token => token.EncryptedRefresh);

        private Result<string> GetDecryptedToken(Func<TokenModel, string> tokenSelector)
        {
            return _db.GetById(TokenId)
                .Map(tokenSelector)
                .Bind(cipherText => _encryptionService.Decrypt(cipherText, GetDeviceSecret()));
        }

        public Result ClearTokens() =>
            _db.Delete(TokenId);

        public Result<bool> HasTokens() =>
            _db.GetById(TokenId).IsSuccess;
    }
}