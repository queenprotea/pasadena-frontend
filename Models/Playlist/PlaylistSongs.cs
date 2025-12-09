using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pasadena_vistas.Models.Playlist
{
    internal class PlaylistSongs
    {
        public int playlist_id { get; set; }
        public string song_id { get; set; }
        public int position { get; set; }
    }
}
