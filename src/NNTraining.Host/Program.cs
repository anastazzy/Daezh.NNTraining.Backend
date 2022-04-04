using Microsoft.EntityFrameworkCore;
using NNTraining.Api.Controllers;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Host;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<NNTrainingDbContext>(x =>
    x.UseNpgsql(builder.Configuration.GetConnectionString("Postgre")));
builder.Services.AddScoped<ICrudForModelService, CrudForModelService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var scope = app.Services.CreateScope();
await using var db = scope.ServiceProvider.GetRequiredService<NNTrainingDbContext>();
await db.Database.MigrateAsync();

app.Run();