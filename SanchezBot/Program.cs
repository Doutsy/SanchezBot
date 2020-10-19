using System;
using System.Collections.Generic;
using System.Net.Http;
using LiteDB;

namespace SanchezBot
{
    public class Membre
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public string Username { get; set; }
        public int Experience { get; set; }
        public int Prestige { get; set; }
        public int DevoirRendu { get; set; }
        public int Or { get; set; }
        public int Niveau { get; set; }
        
        [BsonRef("Quetes")]
        public List<Quete> ActiveQuest { get; set; }
        public int QuestCompleted { get; set; }
        public int DefiReussi { get; set; }
        public int DefiRate { get; set; }
        public int DonjonReussi { get; set; }
        
    }

    public class Quete
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SpecialReward { get; set; }
        public int ExperienceReward { get; set; }
        
        public ulong MessageId { get; set; }

        public Quete()
        {
            
        }

        public Quete(string _name, string _specialReward, int _experienceReward, ulong _messageId)
        {
            Name = _name;
            SpecialReward = _specialReward;
            ExperienceReward = _experienceReward;
            MessageId = _messageId;
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Bot();
            
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}