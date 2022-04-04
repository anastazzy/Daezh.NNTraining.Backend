using Innofactor.EfCoreJsonValueConverter;
using Microsoft.EntityFrameworkCore;
using NNTraining.Domain;

namespace NNTraining.DataAccess;

public class NNTrainingDbContext : DbContext
{
    public NNTrainingDbContext(DbContextOptions<NNTrainingDbContext> options): base(options)
    {
    }

    public DbSet<Model> Models { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Model>()
            .Property(x => x.Parameters)
            .HasJsonValueConversion();
    }
}