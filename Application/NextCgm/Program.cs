using NextCgm.Helpers.SubDomainGenerator;
using NextCgm.Helpers.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// the following is for the SubDomainGenrator
// 1. Pull the paths from appsettings.json
// --- SubDomainGenerator Initialization ---

// 1. Pull the paths from appsettings.json
var adjPath = builder.Configuration["NamingSystem:AdjectivesPath"] ?? "Helpers/SubDomainGenerator/adjectives.txt";
var colPath = builder.Configuration["NamingSystem:ColorsPath"] ?? "Helpers/SubDomainGenerator/colors.txt";
var natoPath = builder.Configuration["NamingSystem:NatoPath"] ?? "Helpers/SubDomainGenerator/nato.txt";
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

// end Register services

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.Run();

// At App Startup (e.g., Program.cs or Startup.cs)
NameGenerator.Initialize("adj.txt", "colors.txt", "nato.txt");

// When you need a name
string newSubdomain = NameGenerator.GetAdjNatio();

string GetAdjColorNato = NameGenerator.GetAdjColorNato(UseAdjColorNatoSuperSlug = false);