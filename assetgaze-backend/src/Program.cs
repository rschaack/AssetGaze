// In: src/Assetgaze/Program.cs

using System.Text;
using System.Text.Json.Serialization;
using Assetgaze.Backend;
using Assetgaze.Backend.Features.Accounts;
using Assetgaze.Backend.Features.Brokers;
using Assetgaze.Backend.Features.Transactions;
using Assetgaze.Backend.Features.Users;
using Assetgaze.Backend.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Define a CORS policy name
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization();

// --- START CORS Configuration ---
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            // Allow credentials is not strictly needed if no cookies are sent,
            // but keeping WithOrigins is good practice.
            policy.WithOrigins("https://localhost:4200") // Your Angular frontend URL
                .AllowAnyHeader()    // Allows all headers from the client
                .AllowAnyMethod();   // Allows all HTTP methods (GET, POST, PUT, DELETE, etc.)
            // Removed: .AllowCredentials(); // No longer sending cookies from frontend
        });
});
// --- END CORS Configuration ---

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddLogging();

// Transaction Feature
builder.Services.AddScoped<ITransactionRepository, Linq2DbTransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

// User Feature
builder.Services.AddScoped<IUserRepository, Linq2DbUserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Broker Feature
builder.Services.AddScoped<IBrokerRepository, Linq2DbBrokerRepository>();
builder.Services.AddScoped<IBrokerSaveService, BrokerSaveService>();

// Account Feature
builder.Services.AddScoped<IAccountRepository, Linq2DbAccountRepository>();
builder.Services.AddScoped<IAccountSaveService, AccountSaveService>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSerilogRequestLogging();

var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    MigrationManager.ApplyMigrations(connectionString);
}

// --- START CORS Middleware Usage ---
app.UseCors(MyAllowSpecificOrigins);
// --- END CORS Middleware Usage ---

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

namespace Assetgaze.Backend
{
    public partial class Program { }
}
