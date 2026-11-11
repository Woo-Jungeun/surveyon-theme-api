using Microsoft.EntityFrameworkCore;
using CoreApi.Data.Entities;

namespace CoreApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ThemeBrand> ThemeBrands => Set<ThemeBrand>();
    public DbSet<ThemePreset> ThemePresets => Set<ThemePreset>();
    public DbSet<ThemeHistory> ThemeHistories => Set<ThemeHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ThemeBrand>(entity =>
        {
            entity.ToTable("theme_brand", "public");
            entity.HasKey(e => e.ProjectNum);

            entity.Property(e => e.ProjectNum).HasColumnName("project_num").HasMaxLength(50);
            entity.Property(e => e.ProjectName).HasColumnName("project_name").HasMaxLength(200);
            entity.Property(e => e.PrimaryColor).HasColumnName("primary_color").HasMaxLength(20).HasDefaultValue("#2563EB");
            entity.Property(e => e.SecondaryColor).HasColumnName("secondary_color").HasMaxLength(20).HasDefaultValue("#3B82F6");
            entity.Property(e => e.SurfaceColor).HasColumnName("surface_color").HasMaxLength(20).HasDefaultValue("#EFF6FF");
            entity.Property(e => e.IsDefault).HasColumnName("is_default").HasDefaultValue(false);
            entity.Property(e => e.ExtractedPalette).HasColumnName("extracted_palette").HasColumnType("jsonb");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<ThemePreset>(entity =>
        {
            entity.ToTable("theme_preset", "public");
            entity.HasKey(e => e.PresetId);

            entity.Property(e => e.PresetId).HasColumnName("preset_id").ValueGeneratedOnAdd();
            entity.Property(e => e.PresetCode).HasColumnName("preset_code").HasMaxLength(50);
            entity.Property(e => e.PresetName).HasColumnName("preset_name").HasMaxLength(100);
            entity.Property(e => e.PrimaryColor).HasColumnName("primary_color").HasMaxLength(20);
            entity.Property(e => e.SecondaryColor).HasColumnName("secondary_color").HasMaxLength(20);
            entity.Property(e => e.SurfaceColor).HasColumnName("surface_color").HasMaxLength(20);
            entity.Property(e => e.IsDefault).HasColumnName("is_default").HasDefaultValue(false);
            entity.Property(e => e.IsSystem).HasColumnName("is_system").HasDefaultValue(false);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(100);
            entity.Property(e => e.UseYn).HasColumnName("use_yn").HasMaxLength(1).HasDefaultValue("Y");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.PresetCode).IsUnique();
        });

        modelBuilder.Entity<ThemeHistory>(entity =>
        {
            entity.ToTable("theme_history", "public");
            entity.HasKey(e => e.HistoryId);

            entity.Property(e => e.HistoryId).HasColumnName("history_id").ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectNum).HasColumnName("project_num").HasMaxLength(50);
            entity.Property(e => e.ActionType).HasColumnName("action_type").HasMaxLength(20).HasDefaultValue("SAVE");
            entity.Property(e => e.PrimaryColor).HasColumnName("primary_color").HasMaxLength(20);
            entity.Property(e => e.SecondaryColor).HasColumnName("secondary_color").HasMaxLength(20);
            entity.Property(e => e.SurfaceColor).HasColumnName("surface_color").HasMaxLength(20);
            entity.Property(e => e.IsDefault).HasColumnName("is_default").HasDefaultValue(false);
            entity.Property(e => e.ExtractedPalette).HasColumnName("extracted_palette").HasColumnType("jsonb");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by").HasMaxLength(50);
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
        });
    }
}
