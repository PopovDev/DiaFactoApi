using DiaFactoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DiaFactoApi;

public sealed class DiaFactoDbContext : DbContext
{
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<SubjectTime> SubjectTimes { get; set; } = null!;

    public DiaFactoDbContext(DbContextOptions<DiaFactoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>()
            .HasMany(g => g.Subjects)
            .WithOne(x => x.Group)
            .HasForeignKey(s => s.GroupId)
            .HasPrincipalKey(g => g.Id)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Group>().Navigation(g => g.Subjects).AutoInclude();

        modelBuilder.Entity<Group>()
            .HasMany(g => g.Students)
            .WithOne(x => x.Group)
            .HasForeignKey(s => s.GroupId)
            .HasPrincipalKey(g => g.Id)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Group>().Navigation(g => g.Students).AutoInclude();


        modelBuilder.Entity<Subject>()
            .HasMany(s => s.SubjectTimes)
            .WithOne(x => x.Subject)
            .HasForeignKey(s => s.SubjectId)
            .HasPrincipalKey(s => s.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}