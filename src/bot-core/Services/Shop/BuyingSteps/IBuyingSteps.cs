using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.Shop
{
    public interface IBuyingSteps : IReadOnlyDictionary<string, BuyStep>
    {
        IReadOnlyDictionary<string, BuyStep> GetBuySteps();
        NextInstruction GetStartingInstruction();
    }
    public delegate Task<NextInstruction> BuyStep(InstructionArguments args);
    public delegate Task<bool> BuyStepResultHandler(NextInstruction args);
    public struct InstructionArguments
    {
        public readonly ulong ChannelId;
        public readonly object Payload;

        public InstructionArguments(ulong channelId, object payload)
        {
            ChannelId = channelId;
            Payload = payload;
        }
    }
    public struct NextInstruction
    {
        public readonly string NextPosition;
        public readonly NextActions NextAction;
        public readonly object Payload;
        public NextInstruction(string nextPosition, NextActions nextAction, object payload = null)
        {
            NextPosition = nextPosition;
            NextAction = nextAction;
            Payload = payload;
        }
    }
    public enum NextActions
    {
        Continue,
        Success,
        Error,
    }
}