using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Would_You_Rather
{
    internal class PlayerPick
    {
        public string date { get; set; } // Date this answer to a question was picked
        public string pick { get; set; } // The answer itself

        public PlayerPick(string date, string pick)
        {
            this.date = date;
            this.pick = pick;
        }   
    }
}
