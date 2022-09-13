
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Name.Bayfaderix.Darxxemiyur.Common;

namespace Name.Bayfaderix.Darxxemiyur.Node.Linkable
{
    public interface IRunnable
    {
        Task Run();
    }
    /// <summary>
    /// Pipe that can act both as a sink and a source
    /// </summary>
    public interface IBiNode : ISinkNode, ISourceNode, INode { }
    public interface INode : IRunnable
    {
        bool IsSink { get; }
        bool IsSource { get; }
        bool IsBi { get; }
    }
    /// <summary>
    /// Pipe that can act as a sink.
    /// </summary>
    public interface ISinkNode : INode
    {
        INodeReceiver ItemReceiver { get; }
    }
    /// <summary>
    /// Pipe that can act as a source
    /// </summary>
    public interface ISourceNode : INode
    {
        INodeTranceiver ItemTranceiver { get; }
    }
    public interface ILinkable
    {
        Task Link(INodeLink link);
        Task UnLink(INodeLink link);
    }
    public interface INodeReceiver : ILinkable
    {
        Task Push(INodeContainer item);
        Task<INodeContainer> Retrieve();
        Task Link(INodeTranceiver source);
        Task UnLink(INodeTranceiver source);
    }
    public interface INodeTranceiver : ILinkable
    {
        Task Propogate(INodeContainer item);
        Task Link(INodeReceiver sink);
        Task UnLink(INodeReceiver sink);
    }
    public class NodeTranceiver : INodeTranceiver
    {
        private readonly List<INodeLink> _outputLinks;
        public NodeTranceiver() => _outputLinks = new();
        public async Task Link(INodeReceiver sink)
        {
            var link = new ItemInstantTransferLink(this, sink);
            await sink.Link(link);
            await Link(link);
        }

        public Task Link(INodeLink link)
        {
            _outputLinks.Add(link);
            return Task.CompletedTask;
        }
        public async Task UnLink(INodeReceiver sink)
        {
            if (_outputLinks.Find(x => x.IsThisPair(this, sink)) is var link == default)
                return;
            await UnLink(link);
            await sink.UnLink(link);
        }
        public Task UnLink(INodeLink link) => Task.FromResult(_outputLinks.Remove(link));
        public Task Propogate(INodeContainer item) => Task.WhenAll(_outputLinks.Select(x => x.Propogate(item)));

    }
    public class NodeReceiver : INodeReceiver
    {
        private readonly List<INodeLink> _inputLinks;
        private readonly TaskEventProxy<INodeContainer> _itemList;
        public NodeReceiver() => (_inputLinks, _itemList) = (new(), new());
        public async Task Link(INodeTranceiver source)
        {
            var link = new ItemInstantTransferLink(source, this);
            await source.Link(link);
            await Link(link);
        }
        public Task Link(INodeLink link)
        {
            _inputLinks.Add(link);
            return Task.CompletedTask;
        }
        public Task<INodeContainer> Retrieve() => _itemList.GetData();
        public Task Push(INodeContainer item) => _itemList.Handle(item);
        public async Task UnLink(INodeTranceiver source)
        {
            if (_inputLinks.Find(x => x.IsThisPair(source, this)) is var link == default)
                return;
            await UnLink(link);
            await source.UnLink(link);
        }
        public Task UnLink(INodeLink link) => Task.FromResult(_inputLinks.Remove(link));
    }

    public interface INodeLink
    {
        Task Propogate(INodeContainer item);
        Task Invalidate();
        bool IsThisPair(INodeTranceiver tr, INodeReceiver re);
        Task<INodeContainer> Retrieve();
    }
    public class ItemQueuedTransferLink : INodeLink
    {
        private readonly TaskEventProxy<INodeContainer> _itemList;
        private readonly INodeTranceiver _from;
        private readonly INodeReceiver _to;
        public ItemQueuedTransferLink(INodeTranceiver from, INodeReceiver to)
         => (_from, _to, _itemList) = (from, to, new());
        public bool IsThisPair(INodeTranceiver tr, INodeReceiver re) => _from == tr && _to == re;
        public Task Propogate(INodeContainer item) => _itemList.Handle(item);
        public Task<INodeContainer> Retrieve() => _itemList.GetData();
        public Task Invalidate() => throw new NotImplementedException();
    }
    public class ItemInstantTransferLink : INodeLink
    {
        private readonly INodeTranceiver _from;
        private readonly INodeReceiver _to;
        public ItemInstantTransferLink(INodeTranceiver from, INodeReceiver to)
         => (_from, _to) = (from, to);
        public Task Propogate(INodeContainer item) => _to.Push(item);
        public bool IsThisPair(INodeTranceiver tr, INodeReceiver re) => _from == tr && _to == re;
        public Task<INodeContainer> Retrieve() => throw new NotImplementedException();
        public Task Invalidate() => throw new NotImplementedException();
    }
}