using Microsoft.EntityFrameworkCore.Design;

using System.Threading.Tasks;

namespace Manito.Discord.Database
{
	/// <summary>
	/// Basic interface for DbFactory
	/// </summary>
	public interface IMyDbFactory
	{
		IDesignTimeDbContextFactory<DbContextImplementation> OriginalFactory {
			get;
		}

		IMyDatabase CreateMyDbContext();

		Task<IMyDatabase> CreateMyDbContextAsync();

		void SetUpFactory();

		Task SetUpFactoryAsync();
	}
}