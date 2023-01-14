using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Innofactor.EfCoreJsonValueConverter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using NNTraining.Domain;
using NNTraining.Domain.Models;
using NNTraining.Domain.Tools;
using File = NNTraining.Domain.Models.File;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NNTraining.DataAccess;

public class NNTrainingDbContext : DbContext
{
    public NNTrainingDbContext(DbContextOptions<NNTrainingDbContext> options) : base(options)
    {
    }

    public DbSet<Model> Models { get; set; }
    public DbSet<File> Files { get; set; }
    public DbSet<ModelFile> ModelFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Model>()
            .Property(x => x.Parameters)
            .HasConversion(new JsonValueConverter<NNParameters>());
        
        modelBuilder.Entity<ModelFile>().HasIndex(x => new
        {
            x.ModelId,
            x.FileId
        }).IsUnique();
    }

    public class JsonValueConverter<T> : ValueConverter<T, string> where T : class
    {
        public JsonValueConverter()
            : base(
                (v => JsonHelper.Serialize(v)),
                (v => JsonHelper.Deserialize<T>(v)))
        {
        }
    }
    
    private static class JsonHelper
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            Converters = { new CustomModelParametersConverter() }
        };
        
        public static T Deserialize<T>(string json) where T : class => 
            string.IsNullOrWhiteSpace(json) ? default (T) : JsonSerializer.Deserialize<T>(
                json,
                options);

        public static string Serialize<T>(T obj) where T : class =>
            obj == null ? null : JsonSerializer.Serialize(obj, options);
    }
}