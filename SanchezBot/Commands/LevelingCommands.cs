using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using LiteDB;
using Microsoft.VisualBasic.CompilerServices;

namespace SanchezBot.Commands
{
    public class Devoir
    {
        public string Name { get; }
        public int Xp { get; }
        public int Bonus { get; }

        public Devoir(string _name, int _xp, int _bonus)
        {
            Name = _name;
            Xp = _xp;
            Bonus = _bonus;
        }
    }

    public class LevelingCommands : BaseCommandModule
    {

        private static readonly Dictionary<string, Devoir> homeworks = new Dictionary<string, Devoir>
        {
            {"study", new Devoir("Study", 20, 0)},
            {"fanart", new Devoir("Fan art", 50, 0)},
            {"persopainting", new Devoir("Perso painting", 50, 0)},
            {"environnementpainting", new Devoir("Environnement painting", 50, 0)},
            {"wallpaperdigitalpainting", new Devoir("Wallpaper Digital Painting", 80, 0)},
            {"portrait", new Devoir("Portait", 20, 0)},
            {"plancheconcept", new Devoir("Planche Concept", 30, 0)},
            {"croquis", new Devoir("Croquis", 20, 0)},
            {"animationcroquis", new Devoir("Animation croquis", 60, 0)},
            {"animationcouleur", new Devoir("Animation couleur", 110, 0)},
            {"objet", new Devoir("Objet", 30, 0)},
            {"objettexture", new Devoir("Objet Texture", 80, 0)},
            {"personnage", new Devoir("Personnage", 50, 0)},
            {"personnagetexture", new Devoir("Personnage Texture", 120, 0)},
            {"decor", new Devoir("Decor", 60, 0)},
            {"decortexture", new Devoir("Decor Texture", 150, 0)},
            {"animation", new Devoir("Animation", 50, 0)},
            {"posing", new Devoir("Posing", 20, 0)},
            {"rigging", new Devoir("Rigging", 80, 0)},
            {"plancheanatomie", new Devoir("Planche anatomie", 40, 0)},
            {"reproduction", new Devoir("Reproduction", 40, 0)},
            {"aquarelle", new Devoir("Aquarelle", 60, 0)},
            {"peinture", new Devoir("Peinture", 60, 0)},
            {"fx", new Devoir("FX", 40, 0)},
            {"particules", new Devoir("Particules", 40, 0)},
            {"cours", new Devoir("Cours", 20, 0)},
        };

        [Command("completeQuest")]
        public async Task CompleteQuest(CommandContext ctx)
        {
            try
            {
                using var db = new LiteDatabase(@"Sanchez.db");
                var userCollection = db.GetCollection<Membre>("Utilisateurs");
                var author = ctx.Message.Author;
                var embed = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Gold,
                    Description = "Ajoute la réaction correspondant à la quête que tu as accomplie",
                    ThumbnailUrl = ctx.Message.Author.AvatarUrl,
                    Title = "QUETE ACCOMPLIE !",
                };

                if (userCollection.Exists(Query.Where("DiscordId",
                    _value => (ulong) _value.AsInt64 == author.Id)))
                {
                    var membre = userCollection
                        .Include((x => x.ActiveQuest))
                        .FindOne(x => x.DiscordId == ctx.Message.Author.Id);

                    if (membre.ActiveQuest.Count != 0)
                    {
                        var questId = new int[5];
                        var iterator = 0;
                        foreach (var quete in membre.ActiveQuest)
                        {
                            embed.AddField((iterator + 1).ToString(), quete.Name, false);
                            questId[iterator] = quete.Id;
                            iterator++;
                        }

                        var embedMessage = await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                        DiscordEmoji[] emojis =
                        {
                            DiscordEmoji.FromName(ctx.Client, ":one:"),
                            DiscordEmoji.FromName(ctx.Client, ":two:"),
                            DiscordEmoji.FromName(ctx.Client, ":three:"),
                            DiscordEmoji.FromName(ctx.Client, ":four:"),
                            DiscordEmoji.FromName(ctx.Client, ":five:"),
                        };

                        for (var i = 0; i < iterator; i++)
                        {
                            await embedMessage.CreateReactionAsync(emojis[i]).ConfigureAwait(false);
                        }

                        var interactivity = ctx.Client.GetInteractivity();

                        var result = await interactivity.WaitForReactionAsync(
                            x => x.Message == embedMessage
                                 && x.User == ctx.User).ConfigureAwait(false);

                        for (var i = 0; i < iterator; i++)
                        {
                            if (result.Result.Emoji == emojis[i])
                            {
                                membre.Experience += membre.ActiveQuest[i].ExperienceReward;
                                membre.QuestCompleted += 1;

                                var completionEmbed = new DiscordEmbedBuilder
                                {
                                    Color = DiscordColor.Gold,
                                    ThumbnailUrl = ctx.Member.AvatarUrl,
                                    Title = "Félicitations " + ctx.Member.Username + " !",
                                };

                                completionEmbed.AddField("Nom de la quête", membre.ActiveQuest[i].Name, true);
                                completionEmbed.AddField("Expérience gagnée",
                                    membre.ActiveQuest[i].ExperienceReward.ToString(), true);
                                completionEmbed.AddField("Récompense spéciale", membre.ActiveQuest[i].SpecialReward, true);

                                await embedMessage.DeleteAsync().ConfigureAwait(false);
                                await ctx.Channel.SendMessageAsync(embed: completionEmbed).ConfigureAwait(false);
                                membre.ActiveQuest.RemoveAll(quest => quest.Id == questId[i]);

                                await UtilsCommands.LevelUp(ctx, membre);
                                
                                userCollection.Update(membre);
                            }
                        }
                    }
                    else
                    {
                        await ctx.Channel
                            .SendMessageAsync("Tu n'as aucune quête en cours !")
                            .ConfigureAwait(false);
                    }

                }
                else
                {
                    await ctx.Channel
                        .SendMessageAsync("Tu dois d'abord accepter une quête " +
                                          "ou rendre un devoir pour avoir accès à cette commande !")
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Command("getQuest")]
        public async Task GetQuest(CommandContext ctx)
        {
            using var db = new LiteDatabase(@"Sanchez.db");
            var questCollection = db.GetCollection<Quete>("Quetes");
            var userCollection = db.GetCollection<Membre>("Utilisateurs");
            var author = ctx.Message.Author;
            var embed = new DiscordEmbedBuilder
            {
                Title = "Quête(s) actives :",
                Color = DiscordColor.Red,
            };
            try
            {
                Membre member;
                if (userCollection.Exists(Query.Where("DiscordId",
                    _value => (ulong) _value.AsInt64 == author.Id)))
                {
                    member = userCollection
                        .Include(x => x.ActiveQuest)
                        .FindOne(x => x.DiscordId == author.Id);

                    foreach (var quest in member.ActiveQuest)
                    {
                        embed.AddField("Nom :", quest.Name, true);
                        embed.AddField("Récompense :", quest.SpecialReward, true);
                        embed.AddField("Expérience :", quest.ExperienceReward.ToString(), true);
                    }

                    if (embed.Fields.Count != 0)
                    {
                        await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.Channel.SendMessageAsync("Tu n'as aucune quête.").ConfigureAwait(false);
                    }
                }
                else
                {
                    await ctx.Channel.SendMessageAsync(
                        "Tu n'as pas encore de compte.\n" +
                        "Rends un devoir ou acceptes une quête pour pouvoir utiliser cette commande").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        [Command("quest")]
        [Description("Créer une quête")]
        public async Task CreateQuest(CommandContext ctx,[Description("Récompense en xp")] int _experience,[Description("La quête suivi d'une virgule suivi de la récompense")] params string[] _quest)
        {
            var testString = string.Empty;
            foreach (var str in _quest)
            {
                testString += str;
                testString += " ";
            }
            var strlist = testString.Split(",", StringSplitOptions.RemoveEmptyEntries);
            var nameQuest = strlist[0];
            var rewardQuest = strlist[1];
            using var db = new LiteDatabase(@"Sanchez.db");
            

            var questCollection = db.GetCollection<Quete>("Quetes");


            var embed = new DiscordEmbedBuilder
            {
                Title = "Nouvelle quête !",
                Color = DiscordColor.Chartreuse,
            };
            embed.AddField("Nom", nameQuest);
            embed.AddField("Expérience", _experience.ToString());
            embed.AddField("Récompense", rewardQuest);
            
            var questMessage = await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            var newQuest = new Quete(nameQuest, rewardQuest, _experience, questMessage.Id);
            var okHandEmoji = DiscordEmoji.FromName(ctx.Client, ":ok_hand:");

            await questMessage.CreateReactionAsync(okHandEmoji).ConfigureAwait(false);
            
            questCollection.Upsert(newQuest);
        }

        [Command("stats"), Description("Affiche vos statistiques")]
        public async Task GetStats(CommandContext ctx)
        {
            using var db = new LiteDatabase(@"Sanchez.db");
            try
            {
                var messageAuthor = ctx.Message.Author;

                var userCollection = db.GetCollection<Membre>("Utilisateurs");
                if (userCollection.Exists(Query.Where("DiscordId", _value => (ulong) _value.AsInt64 == messageAuthor.Id)))
                {
                    var membre = userCollection.FindOne(x => x.DiscordId == messageAuthor.Id);

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"Statistiques de {messageAuthor.Username}",
                        Color = new DiscordColor(255,127,0),
                        ThumbnailUrl = messageAuthor.AvatarUrl
                    };
                    embed.AddField("Experience", membre.Experience.ToString(), true);
                    embed.AddField("Prestige", membre.Prestige.ToString(), true);
                    embed.AddField("Devoir rendu", membre.DevoirRendu.ToString(), true);
                    embed.AddField("Or", membre.Or.ToString(), true);
                    embed.AddField("Niveau", membre.Niveau.ToString(), true);
                    embed.AddField("Quetes actives", membre.ActiveQuest.Count.ToString(), true);
                    embed.AddField("Quetes terminées", membre.QuestCompleted.ToString(), true);
                    embed.AddField("Défis réussis", membre.DefiReussi.ToString(), true);
                    embed.AddField("Défis ratés", membre.DefiRate.ToString(), true);
                    embed.AddField("Donjons réussis", membre.DonjonReussi.ToString(), true);

                    await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("Tu dois rendre un devoir pour débloquer cette commande.");
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }
        

        [Command("farm")]
        [Description("Permet d'envoyer son devoir et de récolter de l'expérience.")]
        public async Task Farm(CommandContext ctx,
            [Description("Type de votre devoir.\n Y'a une grande liste épinglée dans le channel #farm")]
            params string[] _type
        )
        {
            var homeworkName = _type.Aggregate(string.Empty, (current, mot) => current + mot).ToLower();

            if (!homeworks.ContainsKey(homeworkName))
            {
                var otherString = string.Join(' ', _type);
                
                var m = await ctx.Channel.SendMessageAsync(
                    $"{otherString} ne correspond à aucun type de devoir !\nUtilises ``s!help farm`` pour obtenir de l'aide !");

                await Task.Delay(5000);
                await ctx.Message.DeleteAsync();
                await m.DeleteAsync();

                return;
            }

            using var db = new LiteDatabase(@"Sanchez.db");
            try
            {
                var _member = ctx.Message.Author;
                var collection = db.GetCollection<Membre>("Utilisateurs");

                var devoir = homeworks[homeworkName];
                    
                var xpGained = devoir.Xp;
                    
                    
                collection.EnsureIndex("DiscordId", true);

                Membre membre;
                if (collection.Exists(Query.Where("DiscordId", _value => (ulong) _value.AsInt64 == _member.Id)))
                {
                    membre = collection.FindOne(x => x.DiscordId == _member.Id);
                    membre.Experience += xpGained;
                    membre.DevoirRendu++;
                    await UtilsCommands.LevelUp(ctx, membre);
                }
                else
                {
                    membre = new Membre
                    {
                        DiscordId = _member.Id,
                        Username = _member.Username,
                        Experience = xpGained,
                        Prestige = 0,
                        DevoirRendu = 1,
                        Niveau = 1,
                        Or = 0,
                        ActiveQuest = new List<Quete>(),
                        QuestCompleted = 0,
                        DefiReussi = 0,
                        DefiRate = 0,
                        DonjonReussi = 0
                    };
                }
                    
                collection.Upsert(membre);
                await ctx.Channel
                    .SendMessageAsync($"Bien joué {ctx.Message.Author.Username} ! Tu viens de gagner {xpGained} xp !\nTu es maintenant à {membre.Experience} !");
            }
            catch (LiteDB.LiteException ex)
            {
                Console.Write(ex.ToString());
            }
        }
    }
}