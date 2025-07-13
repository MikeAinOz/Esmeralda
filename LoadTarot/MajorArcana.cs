using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LoadTarot
{
   
    public class MajorArcana
    {
        public Item[]? items { get; set; }
        public static MajorArcana LoadJson()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "data", "MajorArcana.json");
            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                MajorArcana majorArcana = JsonSerializer.Deserialize<MajorArcana>(json, JsonOptions.TheseOptions);
                return majorArcana;
            }
        }
    }

    public class Item
    {
        public int number { get; set; }
        public string name { get; set; }
        public string filename { get; set; }
        public string divination { get; set; }
    }

}

