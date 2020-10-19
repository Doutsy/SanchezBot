using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using LiteDB;

namespace SanchezBot.Commands
{
    public class UtilsCommands : BaseCommandModule
    {
        [Command("purge"), Description("Supprime un certain nombre de message")]
        public async Task Bulk(CommandContext ctx,[Description("Nombre de messages à supprimer [1 - 100]")] int _number)
        {
            if (_number <= 0 || _number > 100)
            {
                await ctx.Channel.SendMessageAsync(
                    "Le nombre de messages à supprimer ne doit ni être nul, ni être négatif, ni être supérieur à 100.");
            }
            else
            {
                var messages = await ctx.Channel.GetMessagesAsync(_number).ConfigureAwait(false);
                await ctx.Channel.DeleteMessagesAsync(messages);
            }
        }

        [Command("talk"), Hidden]
        public async Task Talk(CommandContext ctx,params string[] _message)
        {
            try
            {
                if (ctx.User.Id == 169125133447987200)
                {
                    var newString = string.Join(' ', _message);
                    await ctx.Client.GetGuildAsync(651182161931665451).Result.GetChannel(651182163311460355)
                        .SendMessageAsync(newString);
                }
                else
                {
                    await ctx.RespondAsync("tg");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static async Task LevelUp(CommandContext ctx, Membre _membre)
        {
            using var db = new LiteDatabase(@"Sanchez.db");
            var userCollection = db.GetCollection<Membre>("Utilisateurs");
            var Lepreux = ctx.Guild.GetRole(651197287636402222);
            var Villageois = ctx.Guild.GetRole(651197741300580382);
            var Chevalier = ctx.Guild.GetRole(651197832346468359);
            var Mercenaire = ctx.Guild.GetRole(651197958070468610);

            var oldLevel = _membre.Niveau;
            
            _membre.Niveau = _membre.Experience / 100 + 1;
            if (oldLevel < _membre.Niveau)
            {
                switch (_membre.Niveau)
                {
                    case 1:
                        await ctx.Member.GrantRoleAsync(Lepreux);
                        await ctx.Member.RevokeRoleAsync(Villageois);
                        await ctx.Member.RevokeRoleAsync(Chevalier);
                        await ctx.Member.RevokeRoleAsync(Mercenaire);

                        await ctx.Channel.SendMessageAsync(
                            $"@everyone {_membre.Username} vient de devenir un {Lepreux.Name} !")
                            .ConfigureAwait(false);
                        break;
                    case 5:
                        await ctx.Member.RevokeRoleAsync(Lepreux);
                        await ctx.Member.GrantRoleAsync(Villageois);
                        await ctx.Member.RevokeRoleAsync(Chevalier);
                        await ctx.Member.RevokeRoleAsync(Mercenaire);
                        
                        await ctx.Channel.SendMessageAsync(
                                $"@everyone {_membre.Username} vient de devenir un {Villageois.Name} !")
                            .ConfigureAwait(false);
                        break;
                    case 10:
                        await ctx.Member.RevokeRoleAsync(Lepreux);
                        await ctx.Member.RevokeRoleAsync(Villageois);
                        await ctx.Member.GrantRoleAsync(Chevalier);
                        await ctx.Member.RevokeRoleAsync(Mercenaire);
                        
                        await ctx.Channel.SendMessageAsync(
                                $"@everyone {_membre.Username} vient de devenir un {Chevalier.Name} !")
                            .ConfigureAwait(false);
                        break;
                    case 15:
                        await ctx.Member.RevokeRoleAsync(Lepreux);
                        await ctx.Member.RevokeRoleAsync(Villageois);
                        await ctx.Member.RevokeRoleAsync(Chevalier);
                        await ctx.Member.GrantRoleAsync(Mercenaire);
                        
                        await ctx.Channel.SendMessageAsync(
                                $"@everyone {_membre.Username} vient de devenir un {Mercenaire.Name} !")
                            .ConfigureAwait(false);
                        break;
                    default:
                        await ctx.Channel
                            .SendMessageAsync("@everyone " + _membre.Username + " vient de passer niveau " +
                                              _membre.Niveau + " !")
                            .ConfigureAwait(false);
                        break;
                }
            }

            userCollection.Update(_membre);
        }
    }
}