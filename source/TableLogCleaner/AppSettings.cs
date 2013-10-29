using System;
using System.Collections.Generic;
using System.Drawing;

namespace TableLogCleaner
{
    [Serializable]
    public class Coord
    {
        public int Id { get; set; }
        public Rectangle Value { get; set; }
    }

    public class AppSettings
    {
        public string Key { get; set; }

        
        public string TemplateFile { get; set; }
        
        public List<Coord> AllCoords { get; set; }

        public string TestFolder { get; set; }
    }
}