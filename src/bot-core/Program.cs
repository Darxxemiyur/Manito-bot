﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Manito.Discord;
using Manito.Discord.Client;

namespace Manito
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var service = await MyDomain.Create();

            await service.StartBot();
        }
    }
}
