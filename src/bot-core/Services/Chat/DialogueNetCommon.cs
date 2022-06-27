using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.Chat.DialogueNet
{
    public static class Common
    {

        public static IEnumerable<IEnumerable<DiscordButtonComponent>> Generate(
         int[] nums, int[] muls) => muls.Select(y => nums.Select(x => new DiscordButtonComponent(
            ButtonStyle.Secondary, $"{(x > 0 ? "add" : "sub")}{Math.Abs(x * y)}",
             (x > 0 ? "+" : "") + $"{x * y}")));

        public static Task<object> RunNetwork(IDialogueNet chain) =>
            RunNetwork(chain, DefaultNodeResultHandler);
        public static async Task<object> RunNetwork(IDialogueNet chain, NodeResultHandler handler)
        {
            var instruction = chain.GetStartingInstruction();
            do
            {
                instruction = await instruction.NextStep(new(instruction));
            }
            while (!await handler(instruction));
            return instruction.Payload;
        }
        public static NodeResultHandler DefaultNodeResultHandler =>
            (x) => Task.FromResult(x.NextAction != NextNetActions.Continue);
    }
}