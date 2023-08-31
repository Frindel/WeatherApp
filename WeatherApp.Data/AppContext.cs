using Microsoft.EntityFrameworkCore;
using WeatherApp.Data.Entities;

namespace WeatherApp.Data;

public class AppContext: DbContext
{
	public DbSet<City> Cities { get; set; }
	public DbSet<Forecast> Forecast { get; set; }

	public AppContext()
	{
		Database.EnsureCreated();
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		// todo: получение строки подключения из конфигурационного файла
		optionsBuilder.UseSqlite("Data Source=helloapp.db");
	}
}