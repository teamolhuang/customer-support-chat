using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using src.Contexts.Redis;
using src.Contexts.Redis.Abstracts;
using src.Contexts.SharedContexts;
using src.Contexts.SharedContexts.Abstracts;
using src.Controllers;
using src.Handlers;
using src.Utilities;
using src.Utilities.Abstracts;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Controllers, MediatR

builder.Services.AddControllers();
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssemblyContaining<CustomerMessageController>());

#endregion

#region Handlers

builder.Services.AddScoped<SendCustomerMessageCommandDbHandler>();
builder.Services.AddScoped<SendCustomerMessageCommandRedisHandler>();
builder.Services.AddScoped<RegisterCommandDbHandler>();
builder.Services.AddScoped<LoginCommandHandler>();

#endregion

#region Utilities

builder.Services.AddScoped<IJwtHelper, JwtHelper>();
    
#endregion

#region Contexts

builder.Services.AddScoped<ISharedAuthorizedContext, SharedAuthorizedContext>();
builder.Services.AddScoped<IRedisContext, RedisContext>();

// 在子目錄 db 底下建立 db file
Directory.CreateDirectory("db");

builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseSqlite($"Data Source=./db/database.db");
});

#endregion

#region Configurations

builder.Services.Configure<AppSettings>(builder.Configuration);

#endregion

builder.Services.AddSwaggerGen(option =>
{
    // JWT 登入用功能
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    
    option.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new List<string>() } });
    
    // src.xml
    option.IncludeXmlComments(Assembly.GetAssembly(typeof(CustomerMessageController)));
});

// JWT handling
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(jwtOptions =>
    {
        string key = builder.Configuration.GetSection("Jwt").GetValue<string?>("SecretKey") ?? throw new NullReferenceException("JWT SigningKey is missing!");
                
        jwtOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateActor = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        };

        jwtOptions.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                IJwtHelper jwtHelper = context
                    .HttpContext
                    .RequestServices
                    .GetRequiredService<IJwtHelper>();
                
                bool isNotDeleted = await jwtHelper.ValidateUserIdInClaimAsync(context);
                        
                if (!isNotDeleted)
                    context.Fail("請重新登入");

                await jwtHelper.WriteClaimsInSharedContextAsync(context);
            }
        };
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

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");;

app.Run();
