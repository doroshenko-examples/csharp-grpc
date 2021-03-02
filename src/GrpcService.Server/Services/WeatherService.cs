using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService.Server.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GrpcService.Server
{
    public class WeatherService : WeatherExtractor.WeatherExtractorBase
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<WeatherService> _logger;
        private readonly HttpClient _httpClient;

        public WeatherService(IHttpClientFactory factory, ILogger<WeatherService> logger)
        {
            _factory = factory;
            _httpClient = _factory.CreateClient();
            _logger = logger;
        }

        public override async Task<WeatherResponse> GetCurrentWeather(
            GetCurrentWeatherForCityRequest request, ServerCallContext context)
        {
            var weatherData = await GetCurrentTemperatureAsync(request);
            return new WeatherResponse
            {
                Temperature = weatherData.Main.Temp,
                FeelsLike = weatherData.Main.FeelsLike,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };
        }

        public override async Task GetCurrentWeatherStream(GetCurrentWeatherForCityRequest request, IServerStreamWriter<WeatherResponse> responseStream, ServerCallContext context)
        {
            foreach(var _ in Enumerable.Range(0, 10))
            {
                if (context.CancellationToken.IsCancellationRequested) {
                    _logger.LogWarning("Cancelled task");
                    break;
                }
                var weatherData = await GetCurrentTemperatureAsync(request);
                var response = new WeatherResponse
                {
                    Temperature = weatherData.Main.Temp,
                    FeelsLike = weatherData.Main.FeelsLike,
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
                };
                await responseStream.WriteAsync(response);
                await Task.Delay(1000);
            }
        }

        private async Task<WeatherApiResponse> GetCurrentTemperatureAsync(GetCurrentWeatherForCityRequest request)
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={request.City}&appid=503e8ef6fda8828c5cd93e4312dfdca5&units={request.Units}";
            var response = await _httpClient.GetStringAsync(url);

            return JsonConvert.DeserializeObject<WeatherApiResponse>(response);
        }
    }
}