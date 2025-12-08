using pasadena_vistas.Models;
using pasadena_vistas.Services;

namespace pasadena_vistas.vistas.Usuario;

public partial class AlbumPage : ContentPage
{
    private readonly PlayerService _player;
    // Constructor que recibe el álbum seleccionado
    public AlbumPage(Album albumSeleccionado, PlayerService player)
    {
        InitializeComponent();
        _player = player;
        // Conectamos los datos a la vista
        BindingContext = albumSeleccionado;
    }

    private async void BtnVolver_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void PlayButton_Clicked(object sender, EventArgs e)
    {
        var album = BindingContext as Album;
        if (album == null) return;

        await _player.PlayAlbumAsync(album.Songs);

       
    
    }

    private async void Song_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var song = e.CurrentSelection.FirstOrDefault() as Song;
        if (song == null) return;

        // Aquí iría la lógica para reproducir la canción
        // DisplayAlert("Reproducir", $"Tocando: {song.title}", "OK");
        await _player.PlaySongAsync(song);

        ((CollectionView)sender).SelectedItem = null;
    }
}