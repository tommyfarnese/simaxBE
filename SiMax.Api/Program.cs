using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SiMax.Api.Data;
using SiMax.Api.Identity;
using SiMax.Api.Services;
using SiMax.Api.Services.Scheduling;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHttpClient<GitHubService>();
builder.Services.AddScoped<EventsJsonService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("Admin");
    });
});

builder.Services.AddDbContext<SiMaxDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SiMaxDb"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));

builder.Services
    .AddIdentityCore<IdentityUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<SiMaxDbContext>()
    .AddSignInManager();

builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;

    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;

    options.LoginPath = "/admin/account/login";
    options.AccessDeniedPath = "/admin/account/access-denied";
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("SiMaxFrontend", policy =>
    {
        policy
            .WithOrigins(
                "https://tommyfarnese.github.io"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<RoundRobinGenerator>();
builder.Services.AddScoped<MatchScheduler>();
builder.Services.AddScoped<FinalPhaseQualificationService>();
builder.Services.AddScoped<FinalBracketGenerator>();
builder.Services.AddScoped<MatchScheduler>();
builder.Services.AddScoped<FinalMatchScheduler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("SiMaxFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(
        scope.ServiceProvider,
        builder.Configuration);
}

app.Run();
