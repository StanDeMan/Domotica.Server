using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

namespace Domotica.Server.Hubs
{
    public sealed class MainHub : Hub
    {
        public async IAsyncEnumerable<DateTime> Streaming([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (true)
            {
                yield return DateTime.Now;

                try 
                {
                    await Task.Delay(1000, cancellationToken);
                } 
                catch (TaskCanceledException) 
                {
                    yield break;
                }       
            }
        }
    }
}