using System;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Database;
using System.Threading.Tasks;
using Manito.Discord.Client;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus;
using Manito.Discord.Chat.DialogueNet;
using Name.Bayfaderix.Darxxemiyur.Node.Network;

namespace Manito.Discord.PermanentMessage
{

    public class MessageController : IModule
    {
        private List<MessageWallTranslator> _translators;
        private IPermMessageDbFactory _dbFactory;
        private MyDomain _domain;
        public MessageController(MyDomain domain)
        {
            _translators = new();
            _domain = domain;
            _dbFactory = domain.DbFactory;
        }
        public async Task RunModule()
        {
            try
            {
                using (var context = await _dbFactory.CreateMyDbContextAsync())
                {
                    await context.ImplementedContext.Database.EnsureDeletedAsync();
                    await context.ImplementedContext.Database.EnsureCreatedAsync();

                    for (int i = 0; i < 40; i++)
                    {
                        var wall = new MessageWall($"{i}");
                        var wallTrs = new MessageWallTranslator(wall, 0);
                        for (int g = 0; g < 40; g++)
                            wallTrs.Translation.Add((uint)new Random().Next(1, int.MaxValue), $"{g}");

                        await context.MessageWalls.AddAsync(wall);
                        await context.MessageWallTranslators.AddAsync(wallTrs);
                    }

                    await context.SaveChangesAsync();
                }

                while (true)
                {
                    using var context = await _dbFactory.CreateMyDbContextAsync();
                    var walls = context.MessageWalls.ToList();

                    Console.WriteLine(walls.Count);

                    var wall = new MessageWall($"{walls.Count}");
                    var wallTrs = new MessageWallTranslator(wall, 0);

                    for (int i = 0; i < 40; i++)
                        wallTrs.Translation.Add((uint)new Random().Next(1, int.MaxValue), $"{i}");

                    await context.MessageWalls.AddAsync(wall);
                    await context.MessageWallTranslators.AddAsync(wallTrs);
                    await context.SaveChangesAsync();
                    await Task.Delay(500);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }
    }
}
