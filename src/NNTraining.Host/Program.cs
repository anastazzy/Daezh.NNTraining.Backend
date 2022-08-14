using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minio;
using NNTraining.Contracts;
using NNTraining.Contracts.Options;
using NNTraining.DataAccess;
using NNTraining.Host;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<NNTrainingDbContext>(x =>
    x.UseNpgsql(builder.Configuration.GetConnectionString("Postgre")));
builder.Services.AddScoped<ICrudForModelService, CrudForModelService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<MinioOptions>();
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));

builder.Services.AddSingleton(x =>
{
    var options = x.GetRequiredService<IOptions<MinioOptions>>().Value;
    var minio = new MinioClient()
        .WithEndpoint(options.Endpoint)
        .WithCredentials(options.AccessKey, options.SecretKey)
        //.WithSSL()
        .Build();
    return minio;
});


var app = builder.Build();
var minio = app.Services.GetRequiredService<MinioClient>();
await minio.BucketExistsAsync("gyu");

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