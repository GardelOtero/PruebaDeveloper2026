using PruebaDeveloper2026.Infrastructure.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<PruebaDeveloper2026.Infrastructure.Data.IEnsuFileCatalog, PruebaDeveloper2026.Infrastructure.Data.EnsuFileCatalog>();
builder.Services.AddSingleton<PruebaDeveloper2026.Infrastructure.Csv.IEnsuCsvReader, PruebaDeveloper2026.Infrastructure.Csv.EnsuCsvReader>();
builder.Services.AddSingleton<PruebaDeveloper2026.Infrastructure.Repositories.IEnsuRepository, PruebaDeveloper2026.Infrastructure.Repositories.EnsuRepository>();

builder.Services.AddScoped<PruebaDeveloper2026.Application.Services.IDashboardService, PruebaDeveloper2026.Application.Services.DashboardService>();

builder.Services.Configure<PruebaDeveloper2026.Infrastructure.Config.EnsuColumnOptions>(builder.Configuration.GetSection("EnsuColumns"));
builder.Services.Configure<PruebaDeveloper2026.Infrastructure.Options.EnsuDisplayOptions>(builder.Configuration.GetSection("EnsuDisplay"));
builder.Services.Configure<EnsuDataOptions>(builder.Configuration.GetSection("EnsuData"));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
