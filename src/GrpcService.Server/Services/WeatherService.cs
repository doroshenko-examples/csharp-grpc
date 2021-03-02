using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcService.Server.Contracts;
using Newtonsoft.Json;

namespace GrpcService.Server
{
    public class WeatherService : WeatherExtractor.WeatherExtractorBase
    {
        private readonly IHttpClientFactory _factory;

        public WeatherService(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public override async Task<WeatherResponse> GetCurrentWeather(
            GetCurrentWeatherForCityRequest request, ServerCallContext context)
        {
            var httpClient = _factory.CreateClient();
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={request.City}&appid=503e8ef6fda8828c5cd93e4312dfdca5&units={request.Units}";
            var response = await httpClient.GetStringAsync(url);

            var result = JsonConvert.DeserializeObject<WeatherApiResponse>(response);
            return new WeatherResponse
            {
                Temperature = result.Main.Temp,
                FeelsLike = result.Main.FeelsLike
            };
        }
    }
}