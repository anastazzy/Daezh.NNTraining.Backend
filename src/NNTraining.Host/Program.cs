using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using NNTraining.App;
using NNTraining.Contracts;
using NNTraining.Contracts.Options;
using NNTraining.DataAccess;
using Npgsql.Internal;
using Npgsql.Internal.TypeHandlers;
using Npgsql.Internal.TypeHandling;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<NNTrainingDbContext>(x =>
    x.UseNpgsql(builder.Configuration.GetConnectionString("Postgre")));

builder.Services.AddSingleton<IFileStorage, FileStorage>();
builder.Services.AddSingleton<MLContext>();
builder.Services.AddScoped<IModelStorage, ModelStorage>();

builder.Services.AddScoped<IBaseModelService, BaseModelService>();

builder.Services.AddScoped<IModelInteractionService, ModelInteractionService>();
builder.Services.AddSingleton<IModelTrainerFactory, ModelTrainerFactory>();

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    var converters = options.JsonSerializerOptions.Converters;
    converters.Add(new JsonStringEnumConverter());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLocalization();
builder.Services.AddControllersWithViews()
    .AddDataAnnotationsLocalization();

builder.Services.AddOptions<MinioOptions>();
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));

var app = builder.Build();


// Configure the HTTP request pipeline.
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

app.MapControllers();

var scope = app.Services.CreateScope();
await using var db = scope.ServiceProvider.GetRequiredService<NNTrainingDbContext>();
await db.Database.MigrateAsync();

await app.RunAsync();
