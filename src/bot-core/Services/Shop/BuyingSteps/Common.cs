using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.Shop
{
    public static class BuyingStepsCommon
    {

        public static IEnumerable<DiscordButtonComponent> Generate(
            int[] nums, int mul) => nums.Select(x => new DiscordButtonComponent(ButtonStyle.Secondary,
             $"{(x > 0 ? "add" : "sub")}{Math.Abs(x * mul)}", (x > 0 ? "+" : "") + $"{x * mul}"));
        public static IEnumerable<IEnumerable<DiscordButtonComponent>> Generate(
         int[] nums, int[] muls) => muls.Select(x => Generate(nums, x));

        public static async Task<object> RunChain(IBuyingSteps chain, BuyStepResultHandler handler, ulong chId)
        {
            var instruction = chain.GetStartingInstruction();
            do
            {
                instruction = await chain[instruction.NextPosition](new(chId, instruction.Payload));
            }
            while (!await handler(instruction));
            return instruction.Payload;
        }
    }
}