using H4H.Application.Interfaces;
using H4H.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H4H.Domain.Entities;
using System.Threading.Tasks;


namespace H4H.Application.Services
{

    public class WeatherService : IWeatherService
    {
        private readonly IWeatherRepository _weatherRepository;

        public WeatherService(IWeatherRepository weatherRepository)
        {
            _weatherRepository = weatherRepository;
        }

        public async Task<WeatherInfo> GetWeatherAsync(string location)
        {
            return await _weatherRepository.GetWeatherAsync(location);
        }
    }



}


