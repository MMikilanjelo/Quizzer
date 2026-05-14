using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Source.Shared.Services
{
    public interface ISerializationService
    {
        Result<string> Serialize<T>(T obj);
        Result<T> Deserialize<T>(string payload);
    }

    public class SerializationService : ISerializationService
    {
        private readonly ILoggingService _loggingService;
        private readonly JsonSerializerSettings _settings;

        public SerializationService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(new SnakeCaseNamingStrategy())
                },
                Formatting = Formatting.Indented
            };
        }

        public Result<string> Serialize<T>(T obj)
        {
            try
            {
                _loggingService.Info("Serializing obj of type {TargetType}", typeof(T).Name);

                return JsonConvert.SerializeObject(obj, Formatting.None, _settings);
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Failed to serialize object of type {TargetType}", typeof(T).Name);

                return Error.Local(ErrorCodes.SerializationFailure);
            }
        }

        public Result<T> Deserialize<T>(string payload)
        {
            try
            {
                _loggingService.Info("Deserializing payload {Payload} into type {TargetType}", payload, typeof(T).Name);

                return JsonConvert.DeserializeObject<T>(payload, _settings);
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Failed to deserialize payload into type {TargetType}", typeof(T).Name);

                return Error.Local(ErrorCodes.DeserializationFailure);
            }
        }
    }
}