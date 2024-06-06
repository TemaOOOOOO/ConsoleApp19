using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineQueryResults;

namespace ConsoleApp19
{
    
    public class Artists
    {
        public List<Item> items { get; set; }
        public int totalCount { get; set; }
    }

    public class CoverArt
    {
        public List<Source> sources { get; set; }
    }

    public class Data
    {
        public string uri { get; set; }
        public string name { get; set; }
        public Artists artists { get; set; }
        public CoverArt coverArt { get; set; }
        public Date date { get; set; }
        public object profile { get; set; }
        public object visuals { get; set; }
        public object duration { get; set; }
        public object releaseDate { get; set; }
        public object podcast { get; set; }
        public object description { get; set; }
        public object contentRating { get; set; }
        public object images { get; set; }
        public object owner { get; set; }
        public object type { get; set; }
        public object publisher { get; set; }
        public object mediaType { get; set; }
        public object id { get; set; }
        public object albumOfTrack { get; set; }
        public object playability { get; set; }
        public object displayName { get; set; }
        public object username { get; set; }
        public object image { get; set; }
    }

    public class Date
    {
        public int year { get; set; }
    }

    public class Item
    {
        public Data data { get; set; }
        public object sources { get; set; }
        public object uri { get; set; }
        public object profile { get; set; }
    }

    public class Profile
    {
        public string name { get; set; }
    }

    public class Result
    {
        public int totalCount { get; set; }
        public List<Item> items { get; set; }

        public override string ToString()
        {
            List<string> str = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                str.Add( items[i].data.name);


            }
            str.Distinct().ToList();
            string result = string.Empty;
            foreach (string item in str)
            {
                result += item + "\n";
            }
            return result;
        }
    }

    public class Source
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }


}
