using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.Chat.DialogueNet
{

    public delegate Task<NextNetInstruction> Node(InstructionArguments args);
    public delegate Task<bool> NodeResultHandler(NextNetInstruction args);


}