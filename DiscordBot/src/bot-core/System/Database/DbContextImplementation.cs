using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Manito.Discord.Shop;
using Manito.System.Economy;
using Manito.Discord;
using Manito.Discord.PermanentMessage;
using DisCatSharp.Common.Utilities;
using System.Linq;
using Manito.System.Logging;
using Manito.System.Economy;
using Manito.Discord;

namespace Manito.Discord.Database
{

	public class DbContextImplementation : DbContext
	{
		public DbContextImplementation(DbContextOptions options) : base(options) { }

		public DbSet<ShopItem> ShopItems {
			get; set;
		}
		public DbSet<PlayerEconomyDeposit> PlayerEconomyDeposits {
			get; set;
		}
		public DbSet<MessageWallTranslator> MessageWallTranslators {
			get; set;
		}
		public DbSet<MessageWall> MessageWalls {
			get; set;
		}
		public DbSet<MessageWallLine> MessageWallLines {
			get; set;
		}
		public DbSet<LogLine> LogLines {
			get; set;
		}
		public DbSet<PlayerEconomyWork> PlayerWorks {
			get; set;
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<MessageWallTranslator>().HasKey(x => x.ID);
			modelBuilder.Entity<MessageWallTranslator>().Property(x => x.ID).UseIdentityByDefaultColumn();

			modelBuilder.Entity<MessageWallTranslator>().Ignore(x => x.Translation);

			modelBuilder.Entity<MessageWall>().HasKey(x => x.ID);
			modelBuilder.Entity<MessageWall>().Property(x => x.ID).UseIdentityByDefaultColumn();

			modelBuilder.Entity<MessageWallLine>().HasKey(x => x.ID);
			modelBuilder.Entity<MessageWallLine>().Property(x => x.ID).UseIdentityByDefaultColumn();


			modelBuilder.Entity<PlayerEconomyDeposit>().HasKey(x => x.DiscordID);

			modelBuilder.Entity<PlayerEconomyWork>(x => {
				x.HasKey(x => x.DiscordID);
				x.Property(x => x.LastWork);
			});

			modelBuilder.Entity<ShopItem>().HasNoKey();

			modelBuilder.Entity<LogLine>(x => {
				x.HasKey(x => x.ID);
				x.Property(x => x.ID).UseIdentityByDefaultColumn();
				x.Property(b => b.Data).HasColumnType("jsonb");
			});
		}
	}

}
