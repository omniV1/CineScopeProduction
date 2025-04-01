using CineScope.Server.Interfaces;
using CineScope.Server.Services;
using CineScope.Server.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CineScope.Shared.Config;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);



// Configure MVC and Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure MongoDB settings
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(nameof(MongoDbSettings)));

// Add memory cache 
builder.Services.AddMemoryCache();

// Register MongoDB service
builder.Services.AddSingleton<IMongoDbService, MongoDbService>();

// Register caching services
builder.Services.AddSingleton<MovieCacheService>();

// Register other services
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<DataSeedService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<ContentFilterService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddHttpClient<RecaptchaService>();
builder.Services.AddScoped<RecaptchaService>();
builder.Services.Configure<RecaptchaSettings>(
    builder.Configuration.GetSection("Recaptcha"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    try
    {
        // Get JWT settings with fallbacks
        var jwtIssuer = builder.Configuration["JwtSettings__Issuer"] ??
                        builder.Configuration["JwtSettings:Issuer"] ??
                        "https://cinescope-ctaec7bchqbehtdf.westus-01.azurewebsites.net";

        var jwtAudience = builder.Configuration["JwtSettings__Audience"] ??
                          builder.Configuration["JwtSettings:Audience"] ??
                          "https://cinescope-ctaec7bchqbehtdf.westus-01.azurewebsites.net";

        var jwtSecret = builder.Configuration["JwtSettings__Secret"] ??
                        builder.Configuration["JwtSettings:Secret"] ??
                        "very_long_secret_key_for_development_purposes_at_least_32_characters";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };

        // For troubleshooting
        Console.WriteLine($"JWT Config - Issuer: {jwtIssuer}, Audience: {jwtAudience}, Secret Length: {jwtSecret?.Length ?? 0}");
    }
    catch (Exception ex)
    {
        // Log the error but don't throw
        Console.WriteLine($"Error configuring JWT: {ex.Message}");

        // Use a default fallback configuration
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("very_long_secret_key_for_development_purposes_at_least_32_characters"))
        };
    }
});

// Add authorization
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Now build the app AFTER all service registrations
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();
app.MapFallbackToFile("index.html");

app.Run();