namespace WeatherApp.Data.Entities;

public class Forecast
{
	public int Id { get; set; }
	public int CityId { get; set; }
	public DateTime DateFrom { get; set; }
	public DateTime DateTo { get; set; }
	public int Temperature { get; set; }
	public int Pressure { get; set; }

	public City City { get; set; }
}