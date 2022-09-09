using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;

namespace Manito.Discord.Economy
{
    public class PlayerWallet
    {
        private readonly ServerEconomy _economy;
        private readonly ulong _userId;
        public ulong CurrencyEmojiId => _economy.CurrencyEmojiId;
        public string CurrencyEmoji => _economy.CurrencyEmoji;
        public PlayerWallet(ServerEconomy economy, ulong userId)
        {
            _economy = economy;
            _userId = userId;
        }
        public PlayerEconomyDeposit GetDeposit() => _economy.GetDeposit(_userId);
        public Task<long> TransferFunds(ulong to, long amount, string msg = null) =>
         _economy.TransferFunds(_userId, to, amount, msg);
        public Task<long> Withdraw(long amount, string msg = null) => _economy.Withdraw(_userId, amount, msg);
        public Task<bool> CanAfford(long amount) => _economy.CanAfford(_userId, amount);
        public Task<long> Deposit(long amount, string msg = null) => _economy.Deposit(_userId, amount, msg);
    }
    public class ServerEconomy : IModule
    {
        private ulong _emojiId => 997272231384207470;
        private string _emoji => $"<:{_emojiId}:{_emojiId}>";
        public ulong CurrencyEmojiId => _emojiId;
        public string CurrencyEmoji => _emoji;
        private IEconomyDbFactory _dbFactory;
        private Dictionary<ulong, PlayerEconomyDeposit> _deposits;
        private EconomyLogging _logger;
        public Task RunModule() => _logger.RunModule();
        public ServerEconomy(MyDomain service, IEconomyDbFactory factory)
        {
            _dbFactory = factory;
            _logger = new(service);
            _deposits = new();
        }

        /// <summary>
        /// Get user deposit
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>User's deposit</returns>
        public PlayerEconomyDeposit GetDeposit(ulong id) =>
         _deposits.ContainsKey(id) ? _deposits[id] : _deposits[id] = new()
        {
            DiscordID = id,
            Currency = 5000
        };
        public PlayerWallet GetPlayerWallet(ulong id) => new PlayerWallet(this, id);
        public PlayerWallet GetPlayerWallet(DiscordUser user) => GetPlayerWallet(user.Id);
        private Task ReportTransaction(string msg) => _logger.ReportTransaction($"Транзакция: {msg}");
        public async Task<long> TransferFunds(ulong from, ulong to, long amount, string msg = null)
        {
            amount = await DoWithdraw(from, amount);
            amount = await DoDeposit(to, amount);
            await ReportTransaction($"Перевод {to} от {from} на сумму {amount} {_emoji}\n{msg}");
            return amount;
        }
        public async Task<long> Withdraw(ulong from, long amount, string msg = null)
        {
            amount = await DoWithdraw(from, amount);
            await ReportTransaction($"Снятие {amount} {_emoji} у {from}\n{msg}");
            return amount;
        }
        private async Task<long> DoWithdraw(ulong from, long amount)
        {
            amount = Math.Clamp(amount, 0, GetDeposit(from).Currency);
            GetDeposit(from).Currency -= amount;
            return amount;
        }
        public async Task<bool> CanAfford(ulong who, long amount)
        {
            return GetDeposit(who).Currency >= amount;
        }
        public async Task<long> Deposit(ulong to, long amount, string msg = null)
        {
            amount = await DoDeposit(to, amount);
            await ReportTransaction($"Зачисление {amount} {_emoji} у {to}\n{msg}");
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
