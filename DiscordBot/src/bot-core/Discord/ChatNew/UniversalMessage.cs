using DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	/// <summary>
	/// Universal message container that is converted to respected message update call's data!
	/// </summary>
	public class UniversalMessageBuilder
	{
		private List<List<DiscordComponent>> _components;
		private List<DiscordEmbedBuilder> _embeds;
		private Dictionary<string, Stream> _files;
		private List<IMention> _mentions;
		private string _content;
		public UniversalMessageBuilder(DiscordMessage msg)
		{
			_components = msg.Components.Select(x => x.Components.ToList()).ToList();
			_embeds = msg.Embeds.Select(x => new DiscordEmbedBuilder(x)).ToList();
			_mentions = new();
			_content = msg.Content;
		}
		public UniversalMessageBuilder(params UniversalMessageBuilder[] um)
		{

			_embeds = um.SelectMany(x => x._embeds).ToList();

			_components = um.SelectMany(x => x._components).ToList();

			_files = um.SelectMany(x => x._files).ToDictionary(x => x.Key, x => x.Value);

			_mentions = um.SelectMany(x => x._mentions).ToList();

			_content = string.Concat(um.Select(x => x._content));
		}
		public UniversalMessageBuilder(DiscordWebhookBuilder builder)
		{
			_components = builder.Components.Select(x => x.Components.ToList()).ToList();
			_embeds = builder.Embeds.Select(x => new DiscordEmbedBuilder(x)).ToList();
			_files = builder.Files.ToDictionary(x => x.FileName, x => x.Stream);
			_mentions = builder.Mentions.ToList();
			_content = builder.Content;
		}
		public UniversalMessageBuilder(DiscordMessageBuilder builder)
		{
			_components = builder.Components.Select(x => x.Components.ToList()).ToList();
			_embeds = builder.Embeds.Select(x => new DiscordEmbedBuilder(x)).ToList();
			_files = builder.Files.ToDictionary(x => x.FileName, x => x.Stream);
			_mentions = builder.Mentions.ToList();
			_content = builder.Content;
		}
		public UniversalMessageBuilder(DiscordInteractionResponseBuilder builder)
		{
			_components = builder.Components.Select(x => x.Components.ToList()).ToList();
			_embeds = builder.Embeds.Select(x => new DiscordEmbedBuilder(x)).ToList();
			_files = builder.Files.ToDictionary(x => x.FileName, x => x.Stream);
			_mentions = builder.Mentions.ToList();
			_content = builder.Content;
		}
		public UniversalMessageBuilder() => ResetBuilder();
		public UniversalMessageBuilder SetContent(string content)
		{
			_content = content;
			return this;
		}
		public UniversalMessageBuilder AddContent(string content) => SetContent(_content + content);
		public UniversalMessageBuilder AddComponents(params DiscordComponent[] components)
		{
			_components.Add(components.ToList());
			return this;
		}
		public UniversalMessageBuilder AddComponents(params DiscordComponent[][] components)
		{
			foreach (var row in components)
				AddComponents(row);

			return this;
		}
		public UniversalMessageBuilder AddEmbed(DiscordEmbedBuilder embed)
		{
			_embeds.Add(embed);
			return this;
		}
		public UniversalMessageBuilder AddEmbeds(params DiscordEmbedBuilder[] components)
		{
			foreach (var row in components)
				AddEmbed(row);

			return this;
		}
		public UniversalMessageBuilder SetFile(string name, Stream file)
		{
			_files[name] = file;

			return this;
		}
		public UniversalMessageBuilder AddEmbeds(Dictionary<string, Stream> files)
		{
			foreach (var file in files)
				_files[file.Key] = file.Value;

			return this;
		}
		public UniversalMessageBuilder AddMention(IMention mention)
		{
			_mentions.Add(mention);

			return this;
		}
		public UniversalMessageBuilder AddMentions(IEnumerable<IMention> mentions)
		{
			_mentions.AddRange(mentions);

			return this;
		}
		public UniversalMessageBuilder ResetBuilder()
		{
			_components = new();
			_embeds = new();
			_files = new();
			_mentions = new();
			_content = "";

			return this;
		}
		public static implicit operator DiscordWebhookBuilder(UniversalMessageBuilder msg)
		{
			var wbh = new DiscordWebhookBuilder();
			foreach (var row in msg._components)
				wbh.AddComponents(row);

			wbh.AddEmbeds(msg._embeds.Select(x => x.Build()));
			wbh.WithContent(msg._content);
			wbh.AddFiles(msg._files);
			wbh.AddMentions(msg._mentions);

			return wbh;
		}
		public static implicit operator DiscordMessageBuilder(UniversalMessageBuilder msg)
		{
			var wbh = new DiscordMessageBuilder();
			foreach (var row in msg._components)
				wbh.AddComponents(row);

			wbh.AddEmbeds(msg._embeds.Select(x => x.Build()));
			wbh.WithContent(msg._content);
			wbh.WithFiles(msg._files);
			wbh.WithAllowedMentions(msg._mentions);

			return wbh;
		}
		public static implicit operator DiscordInteractionResponseBuilder(UniversalMessageBuilder msg)
		{
			var dirb = new DiscordInteractionResponseBuilder();
			foreach (var row in msg._components)
				dirb.AddComponents(row);

			dirb.AddEmbeds(msg._embeds.Select(x => x.Build()));
			dirb.WithContent(msg._content);
			dirb.AddFiles(msg._files);
			dirb.AddMentions(msg._mentions);

			return dirb;
		}
	}
}
