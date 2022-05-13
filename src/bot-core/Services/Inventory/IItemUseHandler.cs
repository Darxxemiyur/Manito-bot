using System;
using System.Threading.Tasks;

namespace Manito.Services.Inventory
{
    public interface IItemUseHandler
    {
        Task UseItem(IItem item, object args);
    }
}