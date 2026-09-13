using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using AtelierTest.Repositories.Model;

namespace AtelierTest.Repositories;

public class PlayerContext : DbContext
{
    public PlayerContext(DbContextOptions<PlayerContext> options) : base(options)
    {
    }

    public DbSet<PlayerModel> Players => Set<PlayerModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var lastComparer = new ValueComparer<List<int>>(
            (a, b) => a!.SequenceEqual(b!),
            v => v.Aggregate(17, (hash, x) => HashCode.Combine(hash, x)),
            v => v.ToList());

        modelBuilder.Entity<PlayerModel>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.OwnsOne(p => p.Country, country =>
            {
                country.Property(c => c.Code).HasColumnName("CountryCode");
                country.Property(c => c.Picture).HasColumnName("CountryPicture");
            });

            builder.OwnsOne(p => p.Data, data =>
            {
                data.Property(d => d.Rank).HasColumnName("Rank");
                data.Property(d => d.Points).HasColumnName("Points");
                data.Property(d => d.Weight).HasColumnName("Weight");
                data.Property(d => d.Height).HasColumnName("Height");
                data.Property(d => d.Age).HasColumnName("Age");

                data.Property(d => d.Last)
                    .HasColumnName("Last")
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Length == 0
                            ? new List<int>()
                            : v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList())
                    .Metadata.SetValueComparer(lastComparer);
            });
        });
    }
}
