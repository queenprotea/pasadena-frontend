using pasadena_vistas.Models;
using pasadena_vistas.Services;
using System.ComponentModel;
using System.Globalization;

namespace pasadena_vistas.vistas.Usuario;

public partial class AlbumPage : ContentPage
{
    private readonly PlayerService _player;
    private readonly AuthService _authService = new AuthService();
    private int _playlistId;
    private int idUsuarioActual;
    private Album album;

    // Constructor que recibe el álbum seleccionado

    public AlbumPage(Album albumSeleccionado, PlayerService player, int playlistId, int ownerId)
    {
        InitializeComponent();
        _player = player;
        _playlistId = playlistId;
        album = albumSeleccionado;


        SizeChanged += OnSizeChanged;
        _player.OnPlayStateChanged += (isPlaying) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PlayPauseButton.Source = isPlaying ? "icon_pause.png" : "icon_play.png";
            });
        };

        // Conectamos los datos a la vista
        BindingContext = albumSeleccionado;
        _player.OnSongChanged += Player_OnSongChanged;
        _player.OnSongChanged += song =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PlayPauseButton.Source = "icon_pause.png";
                SongName.Text = song.title;
                SongArtist.Text = song.artist;
            });
        };

        if(albumSeleccionado.Songs.Count > 0)
        {
            PlayButton.IsVisible = true;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_playlistId > 0)
        {
            try
            {
                // 1) Refrescar datos de la playlist
                var servicio = new PlaylistService();
                var playlist = await servicio.ObtenerPlaylistPorId(_playlistId);
                var coverUrl = await servicio.ObtenerCoverPlaylistAsync(playlist.playlist_cover);

                NameLabel.Text = playlist.name;
                coverImage.Source = coverUrl;

                // 2) Verificar si el usuario actual es el dueño
                var usuarioActual = await _authService.ObtenerPerfilUsuarioAsync();
                if (usuarioActual != null && playlist.owner_id == usuarioActual.id)
                {
                    EditarButton.IsVisible = true;
                    
                    album.IsPlaylistAndOwner = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo refrescar la playlist: {ex.Message}", "OK");
            }
        }
    }


    void OnSizeChanged(object? sender, EventArgs e)
    {

        double width = this.Width;

        if (width < 600) // Móvil
        {
            ButtomStack.HorizontalOptions = LayoutOptions.End;
            SongName.LineBreakMode = LineBreakMode.TailTruncation;
            SongArtist.LineBreakMode = LineBreakMode.TailTruncation;
        }
        else // Escritorio
        {

            ButtomStack.HorizontalOptions = LayoutOptions.Center;
            SongName.LineBreakMode = LineBreakMode.NoWrap;
            SongArtist.LineBreakMode = LineBreakMode.NoWrap;
        }
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

    private async void EditarButton_Clicked(object sender, EventArgs e)
    {
        var album = BindingContext as Album;
        if (album == null) return;
        // Navegar a la página de edición de la playlist
        await Application.Current.MainPage.Navigation
                .PushAsync(new EditarPlaylistPage(album, _playlistId));
    }

    private async void RemoverButton_Clicked(object sender, EventArgs e) // pendiente
    {
        var boton = sender as ImageButton;
        var song = boton?.BindingContext as Song;
        if (song == null) return;

        bool confirm = await DisplayAlert("Confirmar", $"¿Estás seguro de que deseas remover {song.title} de la playlist?", "Sí", "No");
        if (!confirm) return;
        try
        {
            var servicio = new PlaylistService();
            await servicio.RemoverCancionDePlaylistAsync(_playlistId, song.Id);
            // Quitar de la lista local para refrescar la UI
            var album = BindingContext as Album;
            if (album != null)
            {
                album.Songs.Remove(song);
            }
            await DisplayAlert("Éxito", "Cancion removida correctamente.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo remover la cancion: {ex.Message}", "OK");
        }
    }
}

