using WeatherApp.Model;
using System;
using Prism.Commands;

namespace WeatherApp.ViewModel;

public class MainWindowViewModel
{
	WeatherStorage _weather;

	public string CityName
	{
		get => _weather.CityName;
		set => _weather.CityName = value;
	}

	public string CountryName
	{
		get => _weather.CountryName;
		set => _weather.CountryName = value;
	}

	public DateTime Date
	{
		get => _weather.Date.ToDateTime(TimeOnly.MinValue);
		set => _weather.Date = DateOnly.FromDateTime(value);
	}
	
	#region Commands

	public DelegateCommand Display
	{
		get => new DelegateCommand(() => _weather.DisplayWeather(), () =>
		{
			return true;
		});
	}
	
	#endregion

	public MainWindowViewModel()
	{
		_weather = new WeatherStorage();
	}
}