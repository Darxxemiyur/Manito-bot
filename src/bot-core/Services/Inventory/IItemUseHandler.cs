using System;
using System.Threading.Tasks;

namespace Manito.Discord.Inventory
{
    public interface IItemUseHandler
    {
        Task UseItem(IItem item, object args);
    }
}