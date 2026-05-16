using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Data;
using LibraryApi.Services;
using DotNetEnv;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
var googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? "HÄR_MÅSTE_DET_VARA_EN_NYCKEL_PÅ_MINST_32_TECKNEN_OM_ENV_SAKNAS";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = googleClientId ?? "";
    options.ClientSecret = googleClientSecret ?? "";
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = false, // Sätt till true om din TokenService sätter en specifik Issuer
        ValidateAudience = false, // Sätt till true om din TokenService sätter en specifik Audience
        ClockSkew = TimeSpan.Zero
    };
});


// 1. CORS Settings
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://din-app.netlify.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<TokenService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Aktivera verktygen
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();