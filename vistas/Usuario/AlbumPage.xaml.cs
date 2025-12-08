using pasadena_vistas.Models;

namespace pasadena_vistas.vistas.Usuario;

public partial class AlbumPage : ContentPage
{
    // Constructor que recibe el álbum seleccionado
    public AlbumPage(Album albumSeleccionado)
    {
        InitializeComponent();

        // Conectamos los datos a la vista
        BindingContext = albumSeleccionado;
    }

    private async void BtnVolver_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void Song_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var song = e.CurrentSelection.FirstOrDefault() as Song;
        if (song == null) return;

        // Aquí iría la lógica para reproducir la canción
        // DisplayAlert("Reproducir", $"Tocando: {song.title}", "OK");

        ((CollectionView)sender).SelectedItem = null;
    }
}