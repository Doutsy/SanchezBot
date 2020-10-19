

using System;
using System.Linq;
using DSharpPlus.Entities;
using LiteDB;

namespace SanchezBot.Commands
{
    public class DataBaseCommands
    {
        public Membre AddMembre(DiscordMember _member, int _experience)
        {
            var membre = new Membre();



            using (var db = new LiteDatabase(@"Sanchez.db"))
            {
                var collection = db.GetCollection<Membre>("Utilisateurs");
                
                collection.EnsureIndex(x => x.DiscordId);

                if (collection.Exists(x => x.DiscordId.Equals(_member.Id)))
                {
                    var r = collection.FindOne(x => x.DiscordId == _member.Id);
                    membre.Experience = r.Experience + _experience;
                    membre.DevoirRendu = r.DevoirRendu + 1;
                }
                else
                {
                    membre.DiscordId = _member.Id;
                    membre.Username = _member.Username;
                    membre.Experience = _experience;
                    membre.Prestige = 0;
                    membre.Or = 0;
                    membre.Niveau = 1;
                    membre.DevoirRendu = 1;
                }
                
                collection.Upsert(membre);
                
            }

            return membre;
        }

        public Membre FindMember(ulong _id)
        {
            Membre membre;
            using (var db = new LiteDatabase(@"Users.db"))
            {
                var membres = db.GetCollection<Membre>();
                membre = membres.Find(Query.EQ("DiscordId", _id)) as Membre;
            }
            Console.Write("Return");
            return membre;
        }
    }
}