using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using LiteDB;
using Newtonsoft.Json;
using SanchezBot.Commands;

namespace SanchezBot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        
        public InteractivityExtension Interactivity { get; private set; }
        public async Task RunAsync()
        {
            var json = string.Empty;
            
            using(var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);
            
            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };
            Client = new DiscordClient(config);
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(5)
            });
            
            Client.Ready += OnClientReady;

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] {configJson.Prefix},
                EnableMentionPrefix = true,
                EnableDms = true,
                DmHelp = true,
            };
            
            Commands = Client.UseCommandsNext(commandsConfig);
            
            Commands.RegisterCommands<LevelingCommands>();
            Commands.RegisterCommands<UtilsCommands>();

            await Client.ConnectAsync();
            
            await Task.Delay(-1);
        }

        private async Task<Task> OnMessageReactionAdd(MessageReactionAddEventArgs args)
        { 
            var okHandEmoji = DiscordEmoji.FromName(Client, ":ok_hand:");
            using var db = new LiteDatabase(@"Sanchez.db");
            var questCollection = db.GetCollection<Quete>("Quetes");
            var userCollection = db.GetCollection<Membre>("Utilisateurs");

            try
            {
                if (questCollection.Exists(Query.Where("MessageId",
                    _value => (ulong) _value.AsInt64 == args.Message.Id)))
                {

                    var newQuest = questCollection.FindOne(x => x.MessageId == args.Message.Id);

                    if (args.Emoji == okHandEmoji && args.User != Client.CurrentUser)
                    {
                        Membre member;
                        if (userCollection.Exists(Query.Where("DiscordId",
                            _member => (ulong) _member.AsInt64 == args.User.Id)))
                        {
                            member = userCollection
                                .Include(x => x.ActiveQuest)
                                .FindOne(x => x.DiscordId == args.User.Id);

                            if (member.ActiveQuest.Count < 5)
                            {
                                member.ActiveQuest.Add(newQuest);
                            }    
                            else
                            {
                                await args.Channel.SendMessageAsync(
                                        "Tu ne peux pas avoir plus de 5 quêtes non terminées.\n" +
                                        "Annule ou accomplie une quête avant d'en prendre une nouvelle.")
                                    .ConfigureAwait(false);
                                await args.Message.DeleteReactionAsync(okHandEmoji, args.User).ConfigureAwait(false);
                            }
                        }
                        else
                        {

                            member = new Membre
                            {
                                DiscordId = args.User.Id,
                                Username = args.User.Username,
                                Experience = 0,
                                Prestige = 0,
                                DevoirRendu = 0,
                                Niveau = 1,
                                Or = 0,
                                ActiveQuest = new List<Quete>(),
                                QuestCompleted = 0,
                                DefiReussi = 0,
                                DefiRate = 0,
                                DonjonReussi = 0
                            };
                            member.ActiveQuest.Add(newQuest);
                        }
                        userCollection.Upsert(member);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            return Task.CompletedTask;
        }

        private Task OnMessageReactionRemove(MessageReactionRemoveEventArgs args)
        {
            var okHandEmoji = DiscordEmoji.FromName(Client, ":ok_hand:");
            using var db = new LiteDatabase(@"Sanchez.db");
            var questCollection = db.GetCollection<Quete>("Quetes");
            var userCollection = db.GetCollection<Membre>("Utilisateurs");

            try
            {
                if (questCollection.Exists(Query.Where("MessageId",
                    _value => (ulong) _value.AsInt64 == args.Message.Id)))
                {
                    var newQuest = questCollection.FindOne(x => x.MessageId == args.Message.Id);

                    if (args.Emoji == okHandEmoji && args.User != Client.CurrentUser)
                    {
                        if (userCollection.Exists(Query.Where("DiscordId",
                            _member => (ulong) _member.AsInt64 == args.User.Id)))
                        {
                            var member = userCollection
                                .Include(x => x.ActiveQuest)
                                .FindOne(x => x.DiscordId == args.User.Id);

                            member.ActiveQuest.RemoveAll(quest => quest.Id == newQuest.Id);
                            userCollection.Update(member);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return Task.CompletedTask;
        }
        
        private Task OnClientReady(ReadyEventArgs e)
        {
            e.Client.MessageReactionAdded += OnMessageReactionAdd;
            e.Client.MessageReactionRemoved += OnMessageReactionRemove;
            
            return Task.CompletedTask;
        }
    }
}