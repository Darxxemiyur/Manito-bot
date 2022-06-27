using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.Chat.DialogueNet
{
    public struct NextNetInstruction
    {
        public readonly Node NextStep;
        public readonly NextNetActions NextAction;
        public readonly object Payload;
        public NextNetInstruction(Node nextStep, NextNetActions nextAction, object payload = null)
        {
            NextStep = nextStep;
            NextAction = nextAction;
            Payload = payload;
        }
    }
    public enum NextNetActions
    {
        Continue,
        Success,
        Error,
    }
}
