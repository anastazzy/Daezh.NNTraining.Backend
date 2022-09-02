using Innofactor.EfCoreJsonValueConverter;
using Microsoft.EntityFrameworkCore;
using NNTraining.Domain;
using NNTraining.Domain.Models;
using File = NNTraining.Domain.Models.File;

namespace NNTraining.DataAccess;

public class NNTrainingDbContext : DbContext
{
    public NNTrainingDbContext(DbContextOptions<NNTrainingDbContext> options): base(options)
    {
    }

    public DbSet<Model> Models { get; set; }
    public DbSet<File> Files { get; set; }
    public DbSet<ModelFile> ModelFiles { get; set; }
    public DbSet<ModelFieldNameType> ModelFieldNameTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Model>()
            .Property(x => x.Parameters)
            .HasJsonValueConversion();
        modelBuilder.Entity<ModelFile>().HasKey(x => new {
            x.ModelId, 
            x.FileId
        });
        modelBuilder.Entity<ModelFieldNameType>().HasKey(x => new
        {
            x.IdPair,
            x.IdModel
        });
    }
}