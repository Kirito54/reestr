using reestr.Components;
using MudBlazor.Services;
using reestr.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddSingleton<ExcelDataService>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var service = new ExcelDataService();
    var file = Path.Combine(env.WebRootPath, "data.xlsx");
    if (File.Exists(file))
    {
        try
        {
            service.LoadFromFile(file);
        }
        catch (Exception ex)
        {
            sp.GetRequiredService<ILogger<Program>>().LogWarning(ex, "Failed to load Excel data");
        }
    }
    return service;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
