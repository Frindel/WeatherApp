namespace WeatherApp.Data.Entities;

public class City
{
	public int Id { get; set; }

	public string Name { get; set; }

	public string Country { get; set; }

	public List<Forecast> Forecast { get; set; } = new ();
}