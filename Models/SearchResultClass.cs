using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pasadena_vistas.Models
{
    public class SearchResultClass
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }   // Canción, Artista, Álbum, Género
        public ImageSource Imagen { get; set; }
        public string Detalles { get; set; } 

        public SearchResultClass(){
        
        }
    }

    
}
