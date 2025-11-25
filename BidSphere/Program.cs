#region References
using AutoMapper;
using BidSphere.Repository.Implementation;
using BidSphere.Repository.Interface;
using BidSphere.Service.Implementation;
using Microsoft.EntityFrameworkCore;
using BidSphere.Data;
using BidSphere.Service.Interface;
using BidSphere.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using BidSphere.BackgroundServices;
#endregion

var builder = WebApplication.CreateBuilder(args);

// Configure Database Provider
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "Postgres";

#pragma warning disable CS8604 // Possible null reference argument.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (databaseProvider == "InMemory")
    {
        options.UseInMemoryDatabase("BidSphereDb");
    }
    else
    {
        options.UseNpgsql(
            builder.Environment.IsProduction()
                ? GetPostgresConnectionString()
                : builder.Configuration.GetConnectionString("DefaultConnection")
        );
    }
});
#pragma warning restore CS8604 // Possible null reference argument.


//getting values from evnironment variables
static string GetPostgresConnectionString()

{

    var host = Environment.GetEnvironmentVariable("DB_HOST");

    var database = Environment.GetEnvironmentVariable("DB_NAME");

    var username = Environment.GetEnvironmentVariable("DB_USER");

    var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

    var port = Environment.GetEnvironmentVariable("DB_PORT");

    return $"Host={host};Port={port};Database={database};Username={username};Password={password}";

}


//Add Identity with API Endpoints (includes JWT automatically)
builder.Services.AddIdentityApiEndpoints<User>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole<int>>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization();

//Add Automapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add services to the container.
builder.Services.AddControllers();

//inject Service layer
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBidService, BidService>();

//inject Data Access Layer - Repository
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddScoped<IBidRepository, BidRepository>();

// Background Services
builder.Services.AddHostedService<AuctionExpiryMonitor>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BidSphere API",
        Version = "v1",
        Description = "Auction Management System API with JWT Authentication"
    });

    // Add JWT Bearer authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {your token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Auto Apply Migrations (skip for InMemory)
if (databaseProvider != "InMemory")
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();

        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                ""MigrationId"" varchar(150) PRIMARY KEY,
                ""ProductVersion"" varchar(32) NOT NULL
            )
        ");

        context.Database.Migrate();
    }
}
else
{
    // For InMemory, ensure database is created
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
    }
}

// Seed Identity Roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

    string[] roles = { "Admin", "User", "Guest" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(role));
        }
    }
}

// Seed Sample Data
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedDataAsync(scope.ServiceProvider);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<User>();

app.Run();
