

using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using src.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMediatR(c => Assembly.GetAssembly(typeof(IRequest)));
builder.Services.AddScoped<SendCustomerMessageCommandDbHandler>();

// 在子目錄 db 底下建立 db file
Directory.CreateDirectory("db");

builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseSqlite($"Data Source=./db/database.db");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");;

app.Run();
