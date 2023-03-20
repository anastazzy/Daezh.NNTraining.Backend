using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using NNTraining.App;
using NNTraining.Contracts;
using NNTraining.Contracts.Options;
using NNTraining.DataAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NNTrainingDbContext>(x =>
    x.UseNpgsql(builder.Configuration.GetConnectionString("Postgre")));

builder.Services.AddSignalR();

builder.Services.AddSingleton<IFileStorage, FileStorage>();
builder.Services.AddSingleton<MLContext>();
builder.Services.AddSingleton<IModelTrainingHubContext, ModelTrainingHubContext>();
builder.Services.AddScoped<IModelStorage, ModelStorage>();
builder.Services.AddScoped<INotifyService, NotifyService>();

builder.Services.AddScoped<IBaseModelService, BaseModelService>();

builder.Services.AddScoped<IModelInteractionService, ModelInteractionService>();
builder.Services.AddSingleton<IModelTrainerFactory, ModelTrainerFactory>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    var converters = options.JsonSerializerOptions.Converters;
    converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLocalization();
builder.Services.AddControllersWithViews()
    .AddDataAnnotationsLocalization();

builder.Services.AddOptions<MinioOptions>();
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestLocalization(new RequestLocalizationOptions
{
    ApplyCurrentCultureToResponseHeaders = true
});

app.UseHttpsRedirection();

app.UseAuthorization();

var origins = builder.Configuration.GetSection("Cors").GetSection("Hosts").Get<string[]>();
app.UseCors(corsPolicyBuilder =>
    corsPolicyBuilder.WithOrigins(origins)
        .AllowCredentials()
        .AllowAnyMethod()
        .AllowAnyHeader());

app.MapHub<ModelTrainingHub>("/training");

app.MapControllers();

var scope = app.Services.CreateScope();
await using var db = scope.ServiceProvider.GetRequiredService<NNTrainingDbContext>();
await db.Database.MigrateAsync();

await app.RunAsync();
