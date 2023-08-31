using System;
using System.Windows;
using WeatherApp.Data.Entities;
using WeatherApp.ViewModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AppContext = WeatherApp.Data.AppContext;

namespace WeatherApp.View;

public partial class WeatherDisplay : Window
{
	public WeatherDisplay(DateOnly date, int cityId)
	{
		InitializeComponent();

		// todo: устанока прогнозов
		forecast.ItemsSource = LoadForecast(date, cityId);

		this.date.Content = date.ToString("dd.MM.yy");
		this.dayOfWeek.Content = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(date.DayOfWeek);
	}

	public List<Forecast> LoadForecast(DateOnly date, int cityId)
	{
		using (AppContext context = new AppContext())
		{
			var a = context.Forecast.Where(f => f.CityId == cityId).ToList();

			// выборка данных из БД для конкретной даты и города
			return context.Forecast.Where(f => f.CityId == cityId).OrderBy(f => f.DateFrom).ToList()
				.Where(f => DateOnly.FromDateTime(f.DateFrom.ToLocalTime()) == date).Select(f =>
				{
					f.DateFrom = f.DateFrom.ToLocalTime();
					f.DateTo = f.DateTo.ToLocalTime();
					return f;
				}).ToList();
		}
	}
}