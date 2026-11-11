using Microsoft.EntityFrameworkCore;
using CoreApi.Data.Entities;

namespace CoreApi.Data;

public class PostgresHoDbContext : DbContext
{
    public PostgresHoDbContext(DbContextOptions<PostgresHoDbContext> options) : base(options) { }

    public DbSet<ProErrorCode> ProErrorCodes => Set<ProErrorCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ProErrorCode>(entity =>
        {
            entity.ToTable("pro_error_code");
            entity.HasKey(e => e.ErrorCode);
            entity.Property(e => e.ErrorCode).HasColumnName("errorcode");
            entity.Property(e => e.ErrorText).HasColumnName("errortext");
        });
    }
}
