

using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using src.Contexts.Redis;
using src.Contexts.Redis.Abstracts;
using src.Controllers;
using src.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssemblyContaining<CustomerMessageController>());

builder.Services.AddScoped<SendCustomerMessageCommandDbHandler>();
builder.Services.AddScoped<SendCustomerMessageCommandRedisHandler>();
builder.Services.AddScoped<RegisterCommandDbHandler>();

builder.Services.AddScoped<IRedisContext, RedisContext>();

builder.Services.AddSwaggerGen(option =>
{
    // src.xml
    option.IncludeXmlComments(Assembly.GetAssembly(typeof(CustomerMessageController)));
});

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
else
{
    // 先確保已建立並更新 db
    await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
    await using DatabaseContext db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    await db.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");;

app.Run();
