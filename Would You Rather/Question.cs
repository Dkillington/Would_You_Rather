using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks; 

namespace Would_You_Rather
{
    internal class Question
    {
        public string category { get; set; } 
        public string option1 { get; set; }
        public string option2 { get; set; }

        public List<PlayerPick> playerPicks { get; set; } = new List<PlayerPick>();

        [JsonConstructor]
        public Question(List<PlayerPick> playerPicks, string option1, string option2, string category)
        {
            this.playerPicks = playerPicks;

            if (string.IsNullOrEmpty(option1))
            {
                throw new ArgumentException("The 1st option cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(option2))
            {
                throw new ArgumentException("The 2nd option cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(category))
            {
                throw new ArgumentException("Category cannot be null or empty.");
            }


            // Create the question
            this.option1 = option1;
            this.option2 = option2;
            this.category = category;
        }

        public void Display()
        {
            Console.WriteLine("Would you rather . . .\n");
            Console.WriteLine("[1] " + option1);
            Console.WriteLine("[2] " + option2 + "\n");
        }
        public void ShowPlayerChoices()
        {
            Console.WriteLine($"Topic: {category}\n");
            Display();

            if(playerPicks.Count() <= 0)
            {
                Console.WriteLine("UNANSWERED");
            }
            else
            {
                Console.WriteLine("You chose...");
                foreach(PlayerPick pick in playerPicks)
                {
                    Console.WriteLine($"'{pick.pick}' [{pick.date}]");
                }
                Console.WriteLine();
            }
        }
    }
}
