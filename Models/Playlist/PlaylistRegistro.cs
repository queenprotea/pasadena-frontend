using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pasadena_vistas.Models.Playlist
{
    internal class PlaylistRegistro
    {
        public string name { get; set; }
        public int owner_id { get; set; }
        public bool is_public { get; set; }
    }
}
