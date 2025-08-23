using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RecipeCorner.Data;
using RecipeCorner.Interfaces;
using RecipeCorner.Models;
using RecipeCorner.Repositories;
using RecipeCorner.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// EF Core
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWorkRepo>();
// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
{
    opt.Password.RequiredLength = 6;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT
builder.Services.AddScoped<JwtTokenService>();

var jwt = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
