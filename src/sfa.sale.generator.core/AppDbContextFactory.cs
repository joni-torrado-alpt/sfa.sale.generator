// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Design;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;

// namespace sfa.sale.generator.core;

// public class AppDbContextFactory : IDesignTimeDbContextFactory<SfaDbContext>
// {
//     public AppDbContextFactory()
//     {

//     }

//     private readonly IConfiguration _configuration;
//     public AppDbContextFactory(IConfiguration configuration)
//     {
//         _configuration = configuration;
//     }

//     public SfaDbContext CreateDbContext(string[] args)
//     {
//         // string filePath = @"..\sfa-automation-playwright\";

//         // IConfiguration Configuration = new ConfigurationBuilder()
//         //    .SetBasePath(Path.GetDirectoryName(filePath))
//         //    .AddJsonFile("appSettings.json")
//         //    .Build();

//         var optionsBuilder = new DbContextOptionsBuilder<SfaDbContext>();
//         // optionsBuilder.UseSqlServer(Configuration.GetConnectionString("Default"));
//         optionsBuilder.UseSqlServer(_configuration.GetConnectionString("Default"));

//         return new SfaDbContext(optionsBuilder.Options);
//     }
// }

// // public static class ISserviceCollectionExtension
// // {
// //     public static IServiceCollection configureservice(this IServiceCollection service, IConfiguration Configuration)
// //     {
// //         //access the appsetting json file in your WebApplication File

// //         string filePath = @"..\sfa-automation-playwright\appsettings.json";

// //         Configuration = new ConfigurationBuilder()
// //            .SetBasePath(Path.GetDirectoryName(filePath))
// //            .AddJsonFile("appSettings.json")
// //            .Build();

// //         service.AddDbContext<SfaDbContext>(options =>
// //             options.UseSqlServer(Configuration.GetConnectionString("xxx")));
// //         return service;
// //     }
// // }