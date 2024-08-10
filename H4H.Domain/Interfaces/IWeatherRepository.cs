using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H4H.Domain.Entities;

namespace H4H.Domain.Interfaces
{
    public interface IWeatherRepository
    {
        Task<WeatherInfo> GetWeatherAsync(string location);
    }
}
