
using System.Collections.Generic;
using System.Linq;
using DisCatSharp.EventArgs;

namespace Name.Bayfaderix.Darxxemiyur.Node.Linkable
{
    public class DescriptioningData<T>
    {
        public T Data { get; }
        public DescriptioningData(T data) => Data = data;
    }
    /// <summary>
    /// Type to describe input for a node that will result in giving different DescriptioningData<T>
    /// depending on this type.
    /// </summary>
    /// <typeparam name="T">Contained argument</typeparam>
    public class DescriptioningInput<T>
    {
        public T Data { get; }
        public DescriptioningInput(T data) => Data = data;
    }
    /// <summary>
    /// Container for nodes to utilize only single real type that has other types stacked in it.
    /// Like a converyor belt that carries one box per time unit that can has anything in it.
    /// </summary>
    public interface INodeContainer
    {
        /// <summary>
        /// Event's internal bot's ID for sorting.
        /// </summary>
        ulong ContainerID { get; }
        string ItemType { get; }
        object Custom { get; }
        DescriptioningData<object> GetContainedData(DescriptioningInput<object> payload);
        IEnumerable<INodeContainer> PreviousContainers { get; }
        INodeContainer ReInvent(ulong newId);
        INodeContainer<CItem> TryCast<CItem>();
    }
    public interface INodeContainer<TItem> : INodeContainer
    {
        DescriptioningData<TItem> GetContainedTypedData<InT>(DescriptioningInput<InT> payload);
    }
    public class NodeContainer<TItem> : INodeContainer<TItem>
    {
        private readonly TItem _containedItem;
        public ulong ContainerID { get; }
        public string ItemType => typeof(TItem).FullName;
        public object Custom { get; }
        public IEnumerable<INodeContainer> PreviousContainers { get; }
        public NodeContainer(ulong containerId, TItem tItem, object custom = null)
        {
            ContainerID = containerId;
            _containedItem = tItem;
            PreviousContainers = new INodeContainer[] { this };
            Custom = custom;
        }
        private NodeContainer(ulong containerId, TItem tItem,
         IEnumerable<INodeContainer> previousContainers, object custom = null)
        {
            ContainerID = containerId;
            _containedItem = tItem;
            PreviousContainers = previousContainers;
            Custom = custom;
        }
        public INodeContainer ReInvent(ulong newId)
        {
            return new NodeContainer<TItem>(newId, _containedItem, new[] { this }, Custom);
        }
        public override string ToString() => _containedItem.ToString();

        public DescriptioningData<TItem> GetContainedTypedData<InT>(DescriptioningInput<InT> payload)
        {
            return new(_containedItem);
        }

        public DescriptioningData<object> GetContainedData(DescriptioningInput<object> payload)
        {
            return new(_containedItem);
        }

        public INodeContainer<CItem> TryCast<CItem>() => this as INodeContainer<CItem>;
    }
}