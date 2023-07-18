using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;
using RestaurantAPI.Authorization;
using RestaurantAPI.Entities;
using RestaurantAPI.Middleware;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Validators;
using RestaurantAPI.Services;
using System.Reflection;
using System.Text;

namespace RestaurantAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // NLog setup

            builder.Host.UseNLog();

            // Add services to the container.
            var authenticationSettings = new AuthenticationSettings();
            builder.Configuration.GetSection("Authentication").Bind(authenticationSettings);

            builder.Services.AddSingleton(authenticationSettings);

            builder.Services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = "Bearer";
                option.DefaultScheme = "Bearer";
                option.DefaultChallengeScheme = "Bearer";
            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = authenticationSettings.JwtIssuer,
                    ValidAudience = authenticationSettings.JwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey))
                };
            });

            builder.Services.AddAuthorization(options => 
            {
                options.AddPolicy("HasNationality", builder => builder.RequireClaim("Nationality", "Polish"));
                options.AddPolicy("Atleast20", builder => builder.AddRequirements(new MinimumAgeRequirement(20)));
            });

            builder.Services.AddScoped<IAuthorizationHandler, MinimumAgeRequirementHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, ResourceOperationRequirementHandler>();
            builder.Services.AddControllers().AddFluentValidation();
            builder.Services.AddDbContext<RestaurantDbContext>();
            builder.Services.AddScoped<RestaurantSeeder>();
            builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
            builder.Services.AddScoped<IRestaurantService, RestaurantService>();
            builder.Services.AddScoped<IDishService, DishService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ErrorHandlingMiddleware>();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            builder.Services.AddScoped<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();
            builder.Services.AddScoped<RequestTimeMiddleware>();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<RestaurantSeeder>();

            // Configure the HTTP request pipeline.
            seeder.Seed();

            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<RequestTimeMiddleware>();

            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant API");
            });

            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}