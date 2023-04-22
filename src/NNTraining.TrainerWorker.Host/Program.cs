using Microsoft.ML;
using NNTraining.Common;
using NNTraining.Common.Options;
using NNTraining.TrainerWorker.App;
using NNTraining.TrainerWorker.Contracts;
using NNTraining.TrainerWorker.Host;
using NNTraining.TrainerWorker.Host.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOptions<MinioOptions>();
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));

builder.Services.AddOptions<RabbitMqOptions>();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSignalR();

builder.Services.AddHostedService<TrainHostedListener>();
builder.Services.AddHostedService<PredictHostedListener>();

builder.Services.AddSingleton<IModelTrainingHubContext, ModelTrainingHubContext>();
builder.Services.AddSingleton<IModelTrainerFactory, ModelTrainerFactory>();
builder.Services.AddSingleton<ICustomMinioClient, CustomMinioClient>();
builder.Services.AddSingleton<MLContext>();
builder.Services.AddSingleton<IRabbitMqPublisherService, RabbitMqPublisherService>();

builder.Services.AddSingleton<IModelStorage, ModelStorage>();
builder.Services.AddSingleton<INotifyService, NotifyService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapHub<ModelTrainingHub>("/training");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

var origins = builder.Configuration.GetSection("Cors").GetSection("Hosts").Get<string[]>();
app.UseCors(corsPolicyBuilder =>
    corsPolicyBuilder.WithOrigins(origins)
        .AllowCredentials()
        .AllowAnyMethod()
        .AllowAnyHeader());

app.MapControllers();

app.Run();