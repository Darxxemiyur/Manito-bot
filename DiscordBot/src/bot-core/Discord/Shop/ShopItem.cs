namespace Manito.Discord.Shop
{
	public class ShopItem
	{
		/// <summary>
		/// Name of the Item
		/// </summary>
		public string Name;

		/// <summary>
		/// Category of the Item
		/// </summary>
		public ItemCategory Category;

		/// <summary>
		/// Spawn command
		/// </summary>
		public string SpawnCommand;

		/// <summary>
		/// Price for unit of Item
		/// </summary>
		public int Price;

		public struct InCart
		{
			public readonly ShopItem Item;
			public readonly int Amount;
			public int Price => Item.Price * Amount;
			public string SpawnCommand => string.Format(Item.SpawnCommand, Amount);

			public InCart(ShopItem item, int amount) => (Item, Amount) = (item, amount);
		}
	}
}