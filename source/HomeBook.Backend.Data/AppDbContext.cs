using HomeBook.Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HomeBook.Backend.Data;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IEnumerable<SaveChangesInterceptor>? saveChangesInterceptors)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Configuration> Configurations { get; set; } = null!;
    public DbSet<UserPreference> UserPreferences { get; set; } = null!;
    public DbSet<SavingGoal> SavingGoals { get; set; } = null!;
    public DbSet<Recipe> Recipes { get; set; } = null!;
    public DbSet<Recipe2RecipeIngredient> Recipe2RecipeIngredients { get; set; } = null!;
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; } = null!;
    public DbSet<RecipeStep> RecipeSteps { get; set; } = null!;
    public DbSet<StorageScopeRegistration> StorageScopeRegistrations { get; set; } = null!;
    public DbSet<MediaItem> MediaItems { get; set; } = null!;

    private readonly IEnumerable<SaveChangesInterceptor>? _saveChangesInterceptors = saveChangesInterceptors;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_saveChangesInterceptors is not null)
            optionsBuilder.AddInterceptors(_saveChangesInterceptors);
    }
}
