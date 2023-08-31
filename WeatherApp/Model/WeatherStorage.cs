﻿using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using WeatherApp.Data.Entities;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Prism.Mvvm;

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
		var coordinates = await GetCityCoordinates(CityName);

		var forecast = await GetForecast(coordinates);
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
		                                          $"lat={coordinates.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
		                                          $"&lon={coordinates.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

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

	#endregion
}