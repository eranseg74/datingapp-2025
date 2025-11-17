using System;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
{
  // public DbSet<AppUser> Users { get; set; } // This table is provided by the Identity package
  public DbSet<Member> Members { get; set; }
  public DbSet<Photo> Photos { get; set; }
  public DbSet<MemberLike> Likes { get; set; }
  public DbSet<Message> Messages { get; set; }

  // The OnModelCreating method is a method derived from the DbContext class. The method allows us to override or add configuration to the entity framework functionality. We want to add functionality so we keep the base.OnModelCreating(modelBuilder); and add what we need
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Seeding data into the DB using Identity
    modelBuilder.Entity<IdentityRole>().HasData(
new IdentityRole { Id = "member-id", Name = "Member", NormalizedName = "MEMBER" },
new IdentityRole { Id = "moderator-id", Name = "Moderator", NormalizedName = "MODERATOR" },
new IdentityRole { Id = "admin-id", Name = "Admin", NormalizedName = "ADMIN" }
    );

    // Explicitly defining the relation between entities:
    // Since we did not defined a primary key in the MemberLike table the primary key will be a combination of both the source member ID and the target member ID, using the HasKey method
    modelBuilder.Entity<MemberLike>().HasKey(x => new { x.SourceMemberId, x.TargetMemberId });

    modelBuilder.Entity<Message>()
      .HasOne(s => s.Recipient)
      .WithMany(m => m.MessagesReceived)
      .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Message>()
      .HasOne(s => s.Sender)
      .WithMany(m => m.MessagesSent)
      .OnDelete(DeleteBehavior.Restrict);

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

    // Nullable DateTime converter. The same as above but for nullable DateTime properties. We need to check if the value has value (is not null) before converting it to UTC. If it is null we return null.
    // We need to use the Vaalue property of the nullable type to get the actual DateTime value. If we don't do that we get an error that we cannot convert from Nullable<DateTime> to DateTime
    var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
      v => v.HasValue ? v.Value.ToUniversalTime() : null,
      v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null
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
        else if (property.ClrType == typeof(DateTime?))
        {
          property.SetValueConverter(nullableDateTimeConverter);
        }
      }
    }
  }
}
