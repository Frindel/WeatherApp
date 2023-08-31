using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using WeatherApp.Data.Entities;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Prism.Mvvm;
using WeatherApp.View;
using AppContext = WeatherApp.Data.AppContext;

namespace WeatherApp.Model;

public class WeatherStorage : BindableBase
{
	private const string _weatherApiUrl = "https://api.met.no/weatherapi/locationforecast/2.0/compact?";
	private const string _locationApiUrl = "https://nominatim.openstreetmap.org/search?";

	string _cityName;
	string _countryName;
	DateOnly _date;

	#region Binding properties

	public string CityName
	{
		get => _cityName;
		set
		{
			_cityName = value;
			RaisePropertyChanged(nameof(CityName));
		}
	}

	public string CountryName
	{
		get => _countryName;
		set
		{
			_countryName = value;
			RaisePropertyChanged(nameof(CountryName));
		}
	}

	public DateOnly Date
	{
		get => _date;
		set
		{
			_date = value;
			RaisePropertyChanged(nameof(Date));
		}
	}

	#endregion

	public WeatherStorage()
	{
		_date = DateOnly.FromDateTime(DateTime.Now);
	}

	public async Task DisplayWeather()
	{
		DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now.ToUniversalTime());
		var a = currentDate.AddDays(10);
		if (currentDate.AddDays(10) < Date)
		{
			MessageBox.Show("Данные на даты свыше 10 следующих дней отсутствуют", "Ошибка", MessageBoxButton.OK,
				MessageBoxImage.Information);
			return;
		}

		// проверка на наличие информации о погоде в прошлом
		if (currentDate > Date)
		{
			if (!ForecastIsExists(currentDate))
			{
				MessageBox.Show("Данные на прошлую дату отсутствуют", "Ошибка", MessageBoxButton.OK,
					MessageBoxImage.Information);
			}

			return;
		}

		// получение информации о погоде
		Point coordinates = await GetCityCoordinates(CityName);
		List<Forecast> forecast = await GetForecast(coordinates);

		// добавление полученного прогноза в БД
		UpdateDate(forecast);

		// отображение окна с информацией о погоде
		Application.Current.Dispatcher.Invoke(() => new WeatherDisplay(Date, GetCityId()).Show());
	}

	#region Protected methods

	protected async Task<Point> GetCityCoordinates(string cityName)
	{
		// получение json описания локации
		HttpClient client = new HttpClient();
		client.DefaultRequestHeaders.Add("User-Agent", "weatherApp");
		string json = await client.GetStringAsync(_locationApiUrl + $"q={cityName}&format=geojson");
		JObject locationJson = JObject.Parse(json);

		// сохранение координат локации
		var coordinatesJson = locationJson["features"][0]["geometry"]["coordinates"];
		double latitude = Math.Round(coordinatesJson[0].Value<double>(), 2);
		double longitude = Math.Round(coordinatesJson[1].Value<double>(), 2);

		Point coordinates = new Point(latitude, longitude);
		return coordinates;
	}

	protected async Task<List<Forecast>> GetForecast(Point coordinates)
	{
		// получение json прогноза погоды
		HttpClient client = new HttpClient();
		client.DefaultRequestHeaders.Add("User-Agent", "weatherApp/0.1 github.com/frindel");
		string json = await client.GetStringAsync(_weatherApiUrl +
		                                          $"lon={coordinates.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
		                                          $"&lat={coordinates.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

		// извлечение прогноза
		var forecastJson = JObject.Parse(json)["properties"]["timeseries"];
		List<Forecast> forecast = new List<Forecast>();
		Forecast oldInfo = null;
		var a = forecastJson.ToArray();
		foreach (var forecastInfo in forecastJson)
		{
			DateTime forecastDate = forecastInfo["time"].Value<DateTime>();

			if (DateOnly.FromDateTime(forecastDate.ToLocalTime()) != Date)
				continue;

			double temperature = forecastInfo["data"]["instant"]["details"]["air_temperature"].Value<double>();
			double pressure = forecastInfo["data"]["instant"]["details"]["air_pressure_at_sea_level"].Value<double>();

			Forecast currentForecast = new Forecast()
			{
				Temperature = (int)temperature,
				Pressure = (int)pressure,
				DateTo = forecastDate
			};

			// вычисление средних значений данных между текущей и предыдущей точками прогноза
			if (oldInfo != null)
			{
				currentForecast.Temperature = (int)((temperature + oldInfo.Temperature) / 2);
				currentForecast.Pressure = (int)((pressure + oldInfo.Pressure) / 2);
				currentForecast.DateFrom = oldInfo.DateTo;

				forecast.Add(currentForecast);
			}

			oldInfo = currentForecast;
		}

		return forecast;
	}

	protected bool ForecastIsExists(DateOnly date)
	{
		using (Data.AppContext context = new Data.AppContext())
		{
			DateTime utcDate = Date.ToDateTime(TimeOnly.MinValue).ToUniversalTime();

			return context.Forecast.Any(f => f.DateFrom == utcDate);
		}
	}

	protected void UpdateDate(List<Forecast> forecast)
	{
		int cityId = GetCityId();

		using (Data.AppContext context = new Data.AppContext())
		{
			foreach (Forecast forecastInfo in forecast)
			{
				Forecast savedForecast = context.Forecast.FirstOrDefault(f => f.CityId == cityId
				                                                              && f.DateFrom == forecastInfo.DateFrom
				                                                              && f.DateTo == forecastInfo.DateTo);
				// проверка наличия записи о прогнозе в БД
				if (savedForecast != null)
				{
					savedForecast.Temperature = forecastInfo.Temperature;
					savedForecast.Pressure = forecastInfo.Pressure;

					context.Forecast.Update(savedForecast);
					continue;
				}

				forecastInfo.CityId = cityId;
				context.Forecast.Add(forecastInfo);
			}

			context.SaveChanges();
		}
	}

	protected int GetCityId()
	{
		using (Data.AppContext context = new Data.AppContext())
		{
			City city = context.Cities.FirstOrDefault(c => c.Name == CityName && c.Country == CountryName);

			if (city != null)
				return city.Id;

			City newCity = new City()
			{
				Name = CityName,
				Country = CountryName
			};

			context.Cities.Add(newCity);
			context.SaveChanges();

			return newCity.Id;
		}
	}

	#endregion
}