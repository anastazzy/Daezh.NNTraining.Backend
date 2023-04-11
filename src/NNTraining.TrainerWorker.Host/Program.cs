using NNTraining.Common.Options;
using NNTraining.TrainerWorker.App;
using NNTraining.TrainerWorker.Contracts;
using NNTraining.TrainerWorker.Host;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOptions<RabbitMqOptions>();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddScoped<TrainHostedListener>();

builder.Services.AddSingleton<IModelTrainingHubContext, ModelTrainingHubContext>();
builder.Services.AddScoped<INotifyService, NotifyService>();

builder.Services.AddSingleton<IModelTrainerFactory, ModelTrainerFactory>();

builder.Services.AddScoped<IModelStorage, ModelStorage>();

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

app.MapControllers();

app.Run();