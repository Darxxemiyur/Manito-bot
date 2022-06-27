using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.Chat.DialogueNet
{
    public struct InstructionArguments
    {
        public readonly object Payload;

        public InstructionArguments(object payload)
        {
            Payload = payload;
        }
        public InstructionArguments(NextNetInstruction payload)
        {
            Payload = payload.Payload;
        }
    }
}