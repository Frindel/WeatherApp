using System;
using System.Globalization;
using System.Windows.Data;
using WeatherApp.Data.Entities;

namespace WeatherApp.Converters;

public class TimePeriodConverter:IValueConverter
{
	private IValueConverter _valueConverterImplementation;
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		DateTime date = (DateTime)value;

		return date.ToString("HH:mm");
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return _valueConverterImplementation.ConvertBack(value, targetType, parameter, culture);
	}
}