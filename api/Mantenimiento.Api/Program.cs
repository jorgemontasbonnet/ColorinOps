using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using Mantenimiento.Api.Helpers;
using Mantenimiento.Db;
using Cbsdg.Data.Helpers;


var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Iniciando Mantenimiento.Web Server");

try
{
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var commaOrigins = configuration["AllowedOrigins"];

    logger.Info("CORS Origins: {0}", commaOrigins ?? "*");

    var origin = commaOrigins?.Split(',') ?? ["*"];
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(origin)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["SecIssuer"],
                ValidAudience = configuration["SecAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["SecurityKey"] ?? string.Empty))
            };
        });

    builder.Services.AddOutputCache();
    builder.Services.AddScoped(op => new MailHelper(configuration));
  //  builder.Services.AddScoped(op => new Cypher(configuration["CyferKeyString"]));
    var authConx = configuration.GetConnectionString("AuthDbConx");
    if (string.IsNullOrEmpty(authConx))
    {
        throw new InvalidOperationException("No se ha configurado la cadena de conexión a la base de datos!");
    }
     builder.Services.AddSingleton<IDbProviderFactory>( op => new DbProviderFactory(authConx));

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();   

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }
    
    app.UseCors();
    app.UseHttpsRedirection();

    app.UseOutputCache();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    
    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Programa detenido por un error iniciando!");
}
finally
{
    NLog.LogManager.Shutdown();
}




