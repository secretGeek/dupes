using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dinomopabot.Model
{
    public class DuplicateSet
    {
        public string CheckSum { get; set; }
        public long FileSize { get; set; }
        public List<string> Locations { get; set; }
    }
}
