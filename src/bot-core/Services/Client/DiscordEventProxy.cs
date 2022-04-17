using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;

namespace Manito.Discord
{
    public class DiscordEventProxy<T> : IDisposable
    {
        private TaskEventProxy<(DiscordClient, T)> _facade;
        public DiscordEventProxy() => _facade = new();
        public Task Handle(DiscordClient client, T stuff) => _facade.Handle((client, stuff));
        public Task<Boolean> HasAny() => _facade.HasAny();
        public async Task Cancel() => await _facade.Cancel();
        public async Task<(DiscordClient, T)> GetData() => await _facade.GetData();

        private Boolean disposedValue;
        protected virtual void Dispose(Boolean disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
               ((IDisposable)_facade).Dispose();
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TaskEventProxy()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~DiscordEventProxy()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            this.Dispose(false);
        }

    }
}
