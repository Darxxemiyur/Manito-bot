using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.Chat.DialogueNet
{
    public interface IDialogueNet
    {
        NextNetInstruction GetStartingInstruction();
        NodeResultHandler StepResultHandler { get => Common.DefaultNodeResultHandler; }
    }
}