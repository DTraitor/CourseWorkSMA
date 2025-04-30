using System.Security.Cryptography;
using API.DatabaseContexts;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Services;
using API.Services.Interfaces;
using API.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 🔐 Load RSA keys
var rsaKeyService = new RSAKeyService();
RSA rsaPrivateKey = rsaKeyService.LoadPrivateKey();
RSA rsaPublicKey = rsaKeyService.LoadPublicKey();
var rsaSecurityKey = new RsaSecurityKey(rsaPublicKey);

// 🔧 Add DbContext (ArtifactsDbContext serves both purposes here)
builder.Services.AddDbContext<ArtifactsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🧩 Register Repos & UoW
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISoftwareDevArtifactRepository, SoftwareDevArtifactRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDownloadRepository, DownloadRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();

// 💉 Auth Services
builder.Services.AddSingleton(rsaPrivateKey);
builder.Services.AddScoped<ITokenService, TokenService>();

// 🔐 Add JWT Auth using RS256
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "ArtifactRepo",

            ValidateAudience = true,
            ValidAudience = "ArtifactRepoClient",

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = rsaSecurityKey,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 🔐 Enable auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();