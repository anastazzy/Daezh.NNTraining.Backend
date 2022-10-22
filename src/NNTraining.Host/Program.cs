using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Minio;
using NNTraining.App;
using NNTraining.Contracts;
using NNTraining.Contracts.Options;
using NNTraining.DataAccess;
using NNTraining.Domain.Tools;
using Npgsql;
using Npgsql.Internal;
using Npgsql.Internal.TypeHandlers;
using Npgsql.Internal.TypeHandling;
using NpgsqlTypes;

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

builder.Services.AddOptions<MinioOptions>();
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));

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

await app.RunAsync();

class JsonOverrideTypeHandlerResolverFactory : TypeHandlerResolverFactory
{
    private readonly JsonSerializerOptions _options;

    public JsonOverrideTypeHandlerResolverFactory(JsonSerializerOptions options)
        => _options = options;

    public override TypeHandlerResolver Create(NpgsqlConnector connector)
        => new JsonOverrideTypeHandlerResolver(connector, _options);

    public override string? GetDataTypeNameByClrType(Type clrType)
        => null;

    public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
        => null;

    class JsonOverrideTypeHandlerResolver : TypeHandlerResolver
    {
        readonly JsonHandler _jsonbHandler;

        internal JsonOverrideTypeHandlerResolver(NpgsqlConnector connector, JsonSerializerOptions options)
            => _jsonbHandler ??= new JsonHandler(
                connector.DatabaseInfo.GetPostgresTypeByName("jsonb"),
                connector.TextEncoding,
                isJsonb: true,
                options);

        public override NpgsqlTypeHandler? ResolveByDataTypeName(string typeName)
            => typeName == "jsonb" ? _jsonbHandler : null;

        public override NpgsqlTypeHandler? ResolveByClrType(Type type)
            // You can add any user-defined CLR types which you want mapped to jsonb
            => type == typeof(JsonDocument)
                ? _jsonbHandler
                : null;

        public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
            => null; // Let the built-in resolver do this
    }
}