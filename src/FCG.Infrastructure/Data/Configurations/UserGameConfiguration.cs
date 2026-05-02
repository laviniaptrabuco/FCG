using FCG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Infrastructure.Data.Configurations;

public class UserGameConfiguration : IEntityTypeConfiguration<UserGame>
{
    public void Configure(EntityTypeBuilder<UserGame> builder)
    {
        builder.ToTable("UserGames");

        builder.HasKey(ug => new { ug.UserId, ug.GameId });

        builder.Property(ug => ug.AcquiredAt).IsRequired();

        builder.HasOne(ug => ug.User)
            .WithMany(u => u.Games)
            .HasForeignKey(ug => ug.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ug => ug.Game)
            .WithMany()
            .HasForeignKey(ug => ug.GameId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
