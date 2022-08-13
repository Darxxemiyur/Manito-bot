using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Manito.Discord.Shop;
using Manito.Discord.Economy;
using Manito.Discord.PermanentMessage;
using Emzi0767.Utilities;
using System.Linq;

namespace Manito.Discord.Database
{

    public class DbContextImplementation : DbContext
    {
        public DbContextImplementation(DbContextOptions options) : base(options) { }

        public DbSet<ShopItem> ShopItems { get; set; }
        public DbSet<PlayerEconomyDeposit> PlayerEconomyDeposits { get; set; }
        public DbSet<MessageWallTranslator> MessageWallTranslators { get; set; }
        public DbSet<MessageWall> MessageWalls { get; set; }
        public DbSet<MessageWallLine> MessageWallLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessageWallTranslator>()
                .HasKey(x => x.ID);

            modelBuilder.Entity<MessageWallTranslator>()
                .Property(x => x.ID)
                .HasColumnType("integer")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<MessageWallTranslator>()
                .Ignore(x=>x.Translation);

            modelBuilder.Entity<MessageWall>()
                .HasKey(x => x.ID);

            modelBuilder.Entity<MessageWall>()
                .Property(x => x.ID)
                .HasColumnType("integer")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<MessageWallLine>()
                .HasKey(x => x.ID);

            modelBuilder.Entity<MessageWallLine>()
                .Property(x => x.ID)
                .HasColumnType("integer")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<PlayerEconomyDeposit>()
                .HasKey(x => x.DiscordID);

            modelBuilder.Entity<ShopItem>()
                .HasNoKey();
        }
    }

}
