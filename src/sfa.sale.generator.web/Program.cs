using EntityFrameworkCore.UseRowNumberForPaging;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using sfa.sale.generator.core;
using sfa.sale.generator.web.WebHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

//builder.Services.AddOptions().Configure<AppSettings>(Configuration);
builder.Services.AddMudServices();

bool.TryParse(builder.Configuration["UseDatabase"], out var useDB);
if (useDB)
    builder.Services.AddDbContextFactory<SfaDbContext>(option => option.UseSqlServer(builder.Configuration["ConnectionString"]
    , sqlOptions =>
    {
        // sqlOptions.UseRowNumberForPaging();
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
        sqlOptions.MigrationsHistoryTable("EFMigrationHistory", SfaDbContext.SCHEMA_NAME);
    }));
else
    builder.Services.AddDbContextFactory<SfaDbContext>(option => option.UseInMemoryDatabase("SfaDbContext"));


//builder.Services.AddScoped<Core.IConfiguration, sfa.sale.generator.web.Configuration.Configuration>();
//builder.Services.AddScoped<PackageDeployerManager>();
//builder.Services.AddScoped<IPackageDeployerLogger, PackageDeployerLogger>();

builder.Services.AddSingleton<EventNotifier>();
//builder.Services.AddMediatR(typeof(DeploymentDeleted));

var app = builder.Build();

if (useDB)
{
    var dbContextFactory = app.Services.GetRequiredService<IDbContextFactory<SfaDbContext>>();
    using SfaDbContext context = dbContextFactory.CreateDbContext();
    context.Database.EnsureCreated();
}

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapBlazorHub();
    endpoints.MapFallbackToPage("/_Host");
});

app.Run();