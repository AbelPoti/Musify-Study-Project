using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Musify.Data.DatabaseContext;
using Musify.Data.Query.QueryObjects;
using Musify.Data.Query.QueryUtils.QueryFilters;
using Musify.Dtos.RequestDtos.FilterDtos;
using Musify.Models;
using Musify.Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen only on localhost with HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    // Listen only on loopback (localhost), not on all network interfaces.
    options.ListenLocalhost(7250, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

// Add services to the container.

// Add DB Context
builder.Services.AddDbContext<MusifyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Development"))
);

// Register Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
    .AddEntityFrameworkStores<MusifyDbContext>()
    .AddDefaultTokenProviders();

// Register custom token services
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IEmailConfirmTokenService, EmailConfirmTokenService>();

// Register date time provider
builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

// Register email service
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Register query helper service
builder.Services.AddScoped<ICategoryTreeService, CategoryTreeService>();

// Register filtering classes
builder.Services.AddScoped<IEntityFiltering<Instrument, InstrumentFilterDto>, InstrumentFiltering>();
builder.Services.AddScoped<IEntityFiltering<ShopItem, ShopItemFilterDto>, ShopItemFiltering>();

// Register custom queries classes
builder.Services.AddScoped<IQueries<Instrument, InstrumentFilterDto>, InstrumentQueries>();
builder.Services.AddScoped<IQueries<ShopItem, ShopItemFilterDto>, ShopItemQueries>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Allow enums to be serialized/deserialized as strings
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configure model validation error responses
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        var errors = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

        logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", errors));

        var response = new
        {
            Message = "Model validation failed",
            Errors = errors
        };

        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Add authentication with JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed roles and admin user on startup
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    await IdentitySeeder.SeedRolesAsync(roleManager);
    await IdentitySeeder.SeedAdminUserAsync(roleManager, userManager, builder);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
