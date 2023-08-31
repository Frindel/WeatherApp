using Microsoft.EntityFrameworkCore;
using WeatherApp.Data.Entities;
using Microsoft.Extensions.Configuration;

namespace WeatherApp.Data;

public class AppContext : DbContext
{
	public DbSet<City> Cities { get; set; }
	public DbSet<Forecast> Forecast { get; set; }

	public AppContext()
	{
		Database.EnsureCreated();
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		// получение строки подключения из конфигурационного файлаvar
		var builder = new ConfigurationBuilder();

		builder.SetBasePath(Directory.GetCurrentDirectory());
		builder.AddJsonFile("appsettings.json");
		var config = builder.Build();
		string connectionString = config.GetConnectionString("DefaultConnection");

		optionsBuilder.UseSqlite(connectionString);
	}
}