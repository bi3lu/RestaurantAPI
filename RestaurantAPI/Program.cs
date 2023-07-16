using RestaurantAPI.Entities;

namespace RestaurantAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddTransient<IWeatherForecastService, WeatherForecastService>();
            builder.Services.AddControllers();
            builder.Services.AddDbContext<RestaurantDbContext>();
            builder.Services.AddScoped<RestaurantSeeder>();

            var app = builder.Build();
            var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<RestaurantSeeder>();

            // Configure the HTTP request pipeline.
            seeder.Seed();

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }
}