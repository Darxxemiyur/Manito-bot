using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;
using Name.Bayfaderix.Darxxemiyur.Node.Network;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.Chat.DialogueNet
{
    public interface IDialogueNet : INodeNetwork
    {
        new NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
    }
}