namespace Dinomopabot.Model
{
    using System;
    using System.IO;

    class Settings
    {
        public Settings()
        {
            Path = Environment.CurrentDirectory;
            Subdirectories = SearchOption.AllDirectories;
            Filter = "*.*";
            All = false;
        }

        public string Path { get; set; }
        public SearchOption Subdirectories { get; set; }
        public string Filter { get; set; }
        public bool All { get; set; }
    }
}
