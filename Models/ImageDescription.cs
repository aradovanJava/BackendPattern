using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Back.Models
{
    public class ImageDescription
    {
        public ImageDescription(string description, List<Hashtag> hashtags)
        {
            Description = description;
            Hashtags = hashtags;
        }

        public string Description { get; set; }
        public List<Hashtag> Hashtags { get; set; }
    }
}
