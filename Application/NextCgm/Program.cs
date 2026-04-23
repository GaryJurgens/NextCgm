using NextCgm.Helpers.SubDomainGenerator;
using NextCgm.Helpers.Utils;
using NextCgm.Services.Actions;
using NextCgm.DContentext;
using Microsoft.EntityFrameworkCore;
using FastEndpoints;

using FastEndpoints.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthenticationJwtBearer(s => s.SigningKey = builder.Configuration["JwtSettings:Key"] ?? "f6eb6266-0f27-4f69-bb23-f2ba9d2b8335");
builder.Services.AddAuthorization();


builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCorsPolicy", policy =>
    {
        policy.WithOrigins("*") // Your Vue Port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// the following is for the SubDomainGenrator
// 1. Pull the paths from appsettings.json
// --- SubDomainGenerator Initialization ---

// 1. Pull the paths from appsettings.json
var adjPath = builder.Configuration["NamingSystem:AdjectivesPath"] ?? "Helpers/SubDomainGenerator/adjectives.txt";
var colPath = builder.Configuration["NamingSystem:ColorsPath"] ?? "Helpers/SubDomainGenerator/colors.txt";
var natoPath = builder.Configuration["NamingSystem:NatoPath"] ?? "Helpers/SubDomainGenerator/natoalphabet.txt";
bool UseAdjColorNatoSuperSlug = builder.Configuration.GetValue<bool>("NamingSystem:UseSuperSlug");

// 2. Resolve the absolute paths
string root = AppContext.BaseDirectory;

// 3. Initialize the Static Generator once
NameGenerator.Initialize(
    Path.Combine(root, adjPath),
    Path.Combine(root, colPath),
    Path.Combine(root, natoPath)
);

// Inside Program.cs after Initialize
Console.WriteLine($"Naming System Initialized with {NameGenerator.AdjectiveCount} adjectives.");

// end of SubDomainGenerator

// register services
builder.Services.Configure<DockerOptions>(
    builder.Configuration.GetSection("DockerSettings")); // gets the docker image name and tag from appsettings.json

builder.Services.Configure<DocumentDbOptions>(
    builder.Configuration.GetSection("DocumentDbSettings"));

builder.Services.Configure<NextCgm.Helpers.Utils.PaystackSettings>(
    builder.Configuration.GetSection("PaystackSettings"));

builder.Services.AddScoped<IDockerContainerService,DockerContainerService>();
builder.Services.AddScoped<IDocumentDbService,DocumentDbService>();
builder.Services.AddScoped<IUserService,UserService>();
builder.Services.AddScoped<ILocationService,LocationService>();
builder.Services.AddScoped<IBillingSubscriptionService, BillingSubscriptionService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddFastEndpoints();

builder.Services.AddScoped<IDataSeedService, DataSeedService>();

// end Register services

var app = builder.Build();
//// seed services
// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();
        var dataSeedService = scope.ServiceProvider.GetRequiredService<IDataSeedService>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed countries, states, and timezones
        await dataSeedService.SeedCountriesAndStatesAsync();

       
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}





//// end seed services

app.UseCors("DevCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.Run();