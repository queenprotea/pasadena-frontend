using System.Collections.ObjectModel;

namespace pasadena_vistas.Models
{
    public class Album
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Year { get; set; }
        public string CoverUrl { get; set; } // imagen random por ahora
        public string PrimaryColor { get; set; } = "#582018"; // Para el fondo degradado
        public ObservableCollection<Song> Songs { get; set; } = new ObservableCollection<Song>();
    }
}