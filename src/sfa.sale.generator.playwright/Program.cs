using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using sfa.sale.generator.core;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddDbContext<SfaDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("Default"),
            x => x.MigrationsAssembly("sfa.sale.generator.core")));

builder.Build();

var playwrithApp = new PlaywrightApp();
await playwrithApp.SFA_Should_Create_Sale();

public static class DbOptionsFactory
{
    static DbOptionsFactory()
    {
        // var builder = Host.CreateApplicationBuilder();
        // var app = builder.Build();
        // app.Run();
        // var connectionString = builder.Configuration.GetConnectionString("Default");
        
        // var config = new ConfigurationBuilder()
        //     .AddJsonFile("appsettings.json")
        //     .Build();
        // var connectionString = config["ConnectionString:Default"];
        var connectionString = "server=PJDSFA01;database=Portal;Integrated Security=SSPI;Encrypt=False";

        DbContextOptions = new DbContextOptionsBuilder<SfaDbContext>()
            .UseSqlServer(connectionString)
            .Options;       
    }

    public static DbContextOptions<SfaDbContext> DbContextOptions { get; }
}