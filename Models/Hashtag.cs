using Back.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Back.Models
{
    public partial class Hashtag
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public virtual ICollection<ImageHashtag> ImageHashtags { get; set; } = new List<ImageHashtag>();
    }
}
