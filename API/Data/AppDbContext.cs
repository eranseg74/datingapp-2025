using System;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
  public DbSet<AppUser> Users { get; set; }
  public DbSet<Member> Members { get; set; }
  public DbSet<Photo> Photos { get; set; }
  public DbSet<MemberLike> Likes { get; set; }

  // The OnModelCreating method is a method derived from the DbContext class. The method allows us to override or add configuration to the entity framework functionality. We want to add functionality so we keep the base.OnModelCreating(modelBuilder); and add what we need
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Explicitly defining the relation between entities:
    // Since we did not defined a primary key in the MemberLike table the primary key will be a combination of both the source member ID and the target member ID, using the HasKey method
    modelBuilder.Entity<MemberLike>().HasKey(x => new { x.SourceMemberId, x.TargetMemberId });

    // The following means that any source member can have many members that he likes, and the foreign key will be the liked member Id (source member id), and also, on deletion, all the likes from members that the deleted member likes will also be deleted
    modelBuilder.Entity<MemberLike>()
      .HasOne(s => s.SourceMember)
      .WithMany(t => t.LikedMembers)
      .HasForeignKey(s => s.SourceMemberId)
      .OnDelete(DeleteBehavior.Cascade);

    // Implementing the other side of the relation - all the members that like the source member
    modelBuilder.Entity<MemberLike>()
      .HasOne(s => s.TargetMember)
      .WithMany(t => t.LikedByMembers)
      .HasForeignKey(s => s.TargetMemberId)
      .OnDelete(DeleteBehavior.NoAction);

    // Creating a datetime converter to overcome the UTC problem. The problem is that we save the dat to the DB in UTC but no db (exept PostgreSQL) saves the date in utc format so even if the format is UTC the browser treats it as local time
    // The value converter takes a function / expression that defines to what format the data is converted, and the second parameter is also a function / expression that defines from what format the data is converted. In this example we are converting from date to date. We are converting from any datetime kind to UTC. ToUniversalTime converts dates to UTC  
    var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
      v => v.ToUniversalTime(),
      v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
    );

    // Getting all the entity types we have in the DB and looping on each kind
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
      // For each entity type, looping on all of its properties. If the property is of type DateTime - run the converter on it and set the converted value as the value of this property
      foreach (var property in entityType.GetProperties())
      {
        if (property.ClrType == typeof(DateTime))
        {
          property.SetValueConverter(dateTimeConverter);
        }
      }
    }
  }
}
