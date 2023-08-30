using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using WeatherApp.ViewModel;

namespace WeatherApp
{
	public partial class MainWindow : Window
	{
		private const string _sitiesJsonPath = "\\json\\cities.json";

		public MainWindow()
		{
			InitializeComponent();

			LoadSitiesAsync();

			DataContext = new MainWindowViewModel();
		}

		async void LoadSitiesAsync()
		{
			List<Country> countries = new List<Country>();

			string json;
			using (StreamReader reader = new StreamReader(Environment.CurrentDirectory + _sitiesJsonPath))
				json = await reader.ReadToEndAsync();

			var countriesJson = JArray.Parse(json);

			// десериализация данных о городах
			foreach (var countryJson in countriesJson)
			{
				List<Sity> sities = countryJson["cities"].Select(c => new Sity(c.Value<string>())).ToList();
				Country country = new Country(countryJson["country"].Value<string>(), sities);

				countries.Add(country);
			}

			Application.Current.Dispatcher.Invoke(() => { countriesList.ItemsSource = countries; });
		}

		private class Country
		{
			public string Name { get; set; }

			public List<Sity> Sities { get; set; } = new List<Sity>();

			public Country(string name, List<Sity> sities)
			{
				Name = name;
				Sities = sities;
			}
		}

		private class Sity
		{
			public string Name { get; set; }

			public Sity(string name)
			{
				Name = name;
			}
		}
	}
}