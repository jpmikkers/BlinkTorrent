using QueueTorrent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

var blinktorrentfolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"blinktorrent");
if(!Directory.Exists(blinktorrentfolder))
    Directory.CreateDirectory(blinktorrentfolder);

var torrentService = new TorrentService(blinktorrentfolder);

await torrentService.Start();

builder.Services.AddSession().AddSingleton<ITorrentService>(torrentService);

var app = builder.Build();

if(!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

using(torrentService)
{
    app.Run();
}
