using ABI.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace pasadena_vistas.Models.Playlist
{
    internal class PlaylistRespuesta
    {

        public int id { get; set; }
        public string name { get; set; }
        public string playlist_cover { get; set; }
        public bool is_public { get; set; }
        public int owner_id { get; set; }


    }
}
