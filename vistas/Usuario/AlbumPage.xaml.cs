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
        _player.OnSongChanged += Player_OnSongChanged;
        _player.OnSongChanged += song =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
               
                SongName.Text = song.title;
                SongArtist.Text = song.artist;
            });
        };
    }

    private async void BtnVolver_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void PlayButton_Clicked(object sender, EventArgs e)
    {
        var album = BindingContext as Album;
        if (album == null) return;

        var auth = new AuthService();
        var usuario = await auth.ObtenerPerfilUsuarioAsync();

        if (usuario != null)
            _player.CurrentUserId = usuario.id.ToString();
        else
            _player.CurrentUserId = null;

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
    private void PlayPause_Clicked(object sender, EventArgs e)
    {
        _player.TogglePlayPause();
    }

    private async void NextButton_Clicked(object sender, EventArgs e)
    {
        await _player.PlayNextAsync();
    }

    private async void PreviousButton_Clicked(object sender, EventArgs e)
    {
        await _player.PlayPreviousAsync();
    }
    private void Player_OnSongChanged(pasadena_vistas.Models.Song song)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SongName.Text = song.title;
            SongArtist.Text = song.artist;

            
        });
    }

}