namespace Dinomopabot.Model
{
    using System.Collections.Generic;

    public class DuplicateSet
    {
        public string CheckSum { get; set; }
        public long FileSize { get; set; }
        public List<string> Locations { get; set; }
    }
}
