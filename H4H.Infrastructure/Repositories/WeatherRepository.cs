using H4H.Domain.Entities;
using H4H.Domain.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace H4H.Infrastructure.Repositories
{
    public class WeatherRepository : IWeatherRepository
    {
        private readonly HttpClient _httpClient;

        public WeatherRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherInfo> GetWeatherAsync(string location)
        {
            // Replace with your actual weather API endpoint and API key
            string apiKey = "YOUR_API_KEY";
            var response = await _httpClient.GetFromJsonAsync<WeatherInfo>($"https://api.weatherapi.com/v1/current.json?key={apiKey}&q={location}");
            return response;
        }
    }
}
