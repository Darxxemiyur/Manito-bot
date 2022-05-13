using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Manito.Discord.Client;

namespace Manito.Discord.Economy
{

    public class ServerEconomy : IModule
    {
        private ulong _emojiId => 964951871435468810;
        private string _emoji => $"<:{_emojiId}:{_emojiId}>";
        public ulong CurrencyEmojiId => _emojiId;
        public string CurrencyEmoji => _emoji;
        private Dictionary<ulong, PlayerEconomyDeposit> _deposits;
        private EconomyLogging _logger;
        public Task RunModule() => _logger.RunModule();
        public ServerEconomy(MyDomain service)
        {
            _logger = new(service);
            _deposits = new();

        }

        /// <summary>
        /// Get user deposit
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>User's deposit</returns>
        public PlayerEconomyDeposit GetDeposit(ulong id) => _deposits.ContainsKey(id) ?
        _deposits[id] : _deposits[id] = new()
        {
            DiscordID = id,
            Currency = 5000
        };

        public async Task<long> TransferFunds(ulong from, ulong to, long amount)
        {
            amount = await DoWithdraw(from, amount);
            amount = await DoDeposit(to, amount);
            await _logger.ReportTransaction($"Транзакция: Перевод {to} от {from} на сумму {amount} {_emoji}");
            return amount;
        }
        public async Task<long> Withdraw(ulong from, long amount)
        {
            amount = await DoWithdraw(from, amount);
            await _logger.ReportTransaction($"Транзакция: Снятие {amount} {_emoji} у {from}");
            return amount;
        }
        private async Task<long> DoWithdraw(ulong from, long amount)
        {
            amount = Math.Clamp(amount, 0, GetDeposit(from).Currency);
            GetDeposit(from).Currency -= amount;
            return amount;
        }
        public async Task<long> Deposit(ulong to, long amount)
        {
            amount = await DoDeposit(to, amount);
            await _logger.ReportTransaction($"Транзакция: Зачисление {amount} {_emoji} у {to}");
            return amount;
        }
        private async Task<long> DoDeposit(ulong to, long amount)
        {
            amount = Math.Clamp(amount, 0, long.MaxValue - GetDeposit(to).Currency);
            GetDeposit(to).Currency += amount;
            return amount;
        }

    }

}
