using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Would_You_Rather
{
    internal class PlayerPick
    {
        public string date { get; set; }
        public string pick { get; set; }

        public PlayerPick(string date, string pick)
        {
            this.date = date;
            this.pick = pick;
        }   
    }
}
