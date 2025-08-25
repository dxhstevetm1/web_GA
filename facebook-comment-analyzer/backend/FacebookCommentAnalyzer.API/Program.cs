using Microsoft.AspNetCore.Mvc;
using FacebookCommentAnalyzer.API.Models;
using FacebookCommentAnalyzer.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Configure Facebook API settings
var facebookConfig = builder.Configuration.GetSection("FacebookApi").Get<FacebookApiConfig>();
builder.Services.AddSingleton(facebookConfig ?? new FacebookApiConfig());

// Register existing Facebook API services
builder.Services.AddScoped<IFacebookService, FacebookService>();

// Register new web scraping services
builder.Services.AddScoped<IUrlParserService, UrlParserService>();
builder.Services.AddScoped<IWebScrapingService, WebScrapingService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowVueApp");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();