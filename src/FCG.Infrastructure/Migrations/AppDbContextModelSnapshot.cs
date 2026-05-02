using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using FCG.Infrastructure.Data;

#nullable disable

namespace FCG.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("FCG.Domain.Entities.Game", b =>
            {
                b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                b.Property<string>("Description").IsRequired().HasMaxLength(2000).HasColumnType("nvarchar(2000)");
                b.Property<string>("Genre").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<bool>("IsActive").HasDefaultValue(true).HasColumnType("bit");
                b.Property<decimal>("Price").HasColumnType("decimal(18,2)");
                b.Property<decimal?>("PromotionalPrice").HasColumnType("decimal(18,2)");
                b.Property<string>("Title").IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.HasKey("Id");
                b.HasIndex("Title").IsUnique();
                b.ToTable("Games");
            });

            modelBuilder.Entity("FCG.Domain.Entities.User", b =>
            {
                b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                b.Property<string>("Email").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.Property<bool>("IsActive").HasDefaultValue(true).HasColumnType("bit");
                b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<string>("PasswordHash").IsRequired().HasColumnType("nvarchar(max)");
                b.Property<string>("Role").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
                b.HasKey("Id");
                b.HasIndex("Email").IsUnique();
                b.ToTable("Users");
            });

            modelBuilder.Entity("FCG.Domain.Entities.UserGame", b =>
            {
                b.Property<Guid>("UserId").HasColumnType("uniqueidentifier");
                b.Property<Guid>("GameId").HasColumnType("uniqueidentifier");
                b.Property<DateTime>("AcquiredAt").HasColumnType("datetime2");
                b.HasKey("UserId", "GameId");
                b.HasIndex("GameId");
                b.ToTable("UserGames");
            });

            modelBuilder.Entity("FCG.Domain.Entities.UserGame", b =>
            {
                b.HasOne("FCG.Domain.Entities.Game", "Game")
                    .WithMany()
                    .HasForeignKey("GameId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
                b.HasOne("FCG.Domain.Entities.User", "User")
                    .WithMany("Games")
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.Navigation("Game");
                b.Navigation("User");
            });

            modelBuilder.Entity("FCG.Domain.Entities.User", b =>
            {
                b.Navigation("Games");
            });
#pragma warning restore 612, 618
        }
    }
}
