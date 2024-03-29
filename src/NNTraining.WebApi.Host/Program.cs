using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using NNTraining.App;
using NNTraining.App.Utils;
using NNTraining.Common;
using NNTraining.Common.Options;
using NNTraining.WebApi.Contracts;
using NNTraining.WebApi.DataAccess;
using NNTraining.WebApi.Host.Workers;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOptions<MinioOptions>();
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));

builder.Services.AddOptions<RabbitMqOptions>();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddDbContext<NNTrainingDbContext>(x =>
    x.UseNpgsql(builder.Configuration.GetConnectionString("Postgre")));

builder.Services.AddSignalR();

builder.Services.AddHostedService<ChangeStatusHostedListener>();
builder.Services.AddHostedService<ModelToTrainCrawler>();
builder.Services.AddHostedService<SaveModelHostedListener>();

builder.Services.AddSingleton<ICustomMinioClient, CustomMinioClient>();
builder.Services.AddSingleton<IFileStorage, FileStorage>();

builder.Services.AddScoped<IRabbitMqPublisherService, RabbitMqPublisherService>();
builder.Services.AddScoped<IWebAppPublisherService, WebAppPublisherService>();
builder.Services.AddScoped<IBaseModelService, BaseModelService>();
builder.Services.AddScoped<IModelInteractionService, ModelInteractionService>();

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

app.MapControllers();

var scope = app.Services.CreateScope();
await using var db = scope.ServiceProvider.GetRequiredService<NNTrainingDbContext>();
await db.Database.MigrateAsync();

await app.RunAsync();
