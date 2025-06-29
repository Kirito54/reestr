using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using reestr.Components;
using MudBlazor.Services;
using reestr.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<ExcelDataService>();

var host = builder.Build();

var excelService = host.Services.GetRequiredService<ExcelDataService>();
try
{
    using var http = host.Services.GetRequiredService<HttpClient>();
    using var stream = await http.GetStreamAsync("data.xlsx");
    excelService.LoadFromStream(stream);
}
catch (Exception ex)
{
    host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program").LogWarning(ex, "Failed to load Excel data");
}

await host.RunAsync();
