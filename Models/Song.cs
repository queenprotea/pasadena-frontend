using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pasadena_vistas.Models
{
    public class Song
    {
        public string Id { get; set; }
        public string title { get; set; }
        public string artist { get; set; }
        public string album { get; set; }
        public string year { get; set; }
        public string genre { get; set; }
        public double duration { get; set; }
       public byte[] album_cover { get; set; }
        public int songNumber { get; set; }
        public byte[] file_data { get; set; }

        public Song()
        {

        }

    }
    
    
}
