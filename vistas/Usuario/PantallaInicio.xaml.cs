using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using pasadena_vistas.Models;
using pasadena_vistas.vistas.Administrador;
using Plugin.Maui.Audio;

namespace pasadena_vistas.vistas.Usuario;

public partial class PantallaInicio : ContentPage
{
    // Audio (por ahora solo preparado, sin lógica real de streaming)
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _currentPlayer;

    // Colecciones para binding
    public ObservableCollection<SearchResultClass> SearchResults { get; } = new();
    public ObservableCollection<Album> AlbumsRecomendados { get; } = new();
    public ObservableCollection<PlaylistItem> Playlists { get; } = new();

    public PantallaInicio()
    {
        InitializeComponent();

        _audioManager = AudioManager.Current;
        BindingContext = this;

        CargarPlaylistsDePrueba();
        CargarAlbumsDePrueba();

        // Ocultamos resultados de búsqueda al inicio
        SearchResultsView.IsVisible = false;
    }

    // ===================== DATOS DE PRUEBA =====================

    private void CargarPlaylistsDePrueba()
    {
        Playlists.Clear();
        Playlists.Add(new PlaylistItem { Id = "1", Name = "Tus me gusta", Emoji = "🎵" });
        Playlists.Add(new PlaylistItem { Id = "2", Name = "Canciones tristes", Emoji = "🎧" });
        Playlists.Add(new PlaylistItem { Id = "3", Name = "Rock clásico", Emoji = "🎸" });
    }

    private void CargarAlbumsDePrueba()
    {
        AlbumsRecomendados.Clear();

        var album1 = new Album
        {
            Name = "Lo-fi para estudiar",
            Artist = "Varios artistas",
            CoverUrl = "default_cover.jpg"
        };
        album1.Songs.Add(new Song { title = "Beat 1", artist = "Lo-fi Artist", songNumber = 1 });
        album1.Songs.Add(new Song { title = "Beat 2", artist = "Lo-fi Artist", songNumber = 2 });

        var album2 = new Album
        {
            Name = "Rock clásico",
            Artist = "Varios artistas",
            CoverUrl = "default_cover.jpg"
        };
        album2.Songs.Add(new Song { title = "Highway Song", artist = "The Classics", songNumber = 1 });

        var album3 = new Album
        {
            Name = "Pop latino",
            Artist = "Varios artistas",
            CoverUrl = "default_cover.jpg"
        };

        AlbumsRecomendados.Add(album1);
        AlbumsRecomendados.Add(album2);
        AlbumsRecomendados.Add(album3);
    }

    // ===================== MENÚ / PERFIL =====================

    private void ToggleMenu_Clicked(object sender, EventArgs e)
    {
        LeftMenu.IsVisible = !LeftMenu.IsVisible;
    }

    private async void ProfileButton_Clicked(object sender, EventArgs e)
    {
        // Ir a la página de perfil de usuario
        await Shell.Current.GoToAsync(nameof(PerfilUsuarioPage));
    }

    // ===================== BÚSQUEDA =====================

    private void SearchSong_Clicked(object sender, EventArgs e)
    {
        var query = SearchBar.Text?.Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            SearchResults.Clear();
            SearchResultsView.IsVisible = false;
            return;
        }

        // Por ahora, resultados de prueba
        SearchResults.Clear();
        SearchResults.Add(new SearchResultClass
        {
            Nombre = query,
            Tipo = "Resultado de ejemplo"
        });

        SearchResultsView.IsVisible = true;
    }

    private void SearchSong_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            SearchResults.Clear();
            SearchResultsView.IsVisible = false;
        }
    }

    private async void OnSearchResultSelected(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault() as SearchResultClass;
        if (selected == null) return;

        if (sender is CollectionView cv)
            cv.SelectedItem = null;

        // Aquí después puedes llamar a la lógica de reproducción de tu compa
        await DisplayAlert("Canción seleccionada", selected.Nombre, "OK");
    }

    // ===================== PLAYLISTS =====================

    // Botón "+ Crear Playlist" → ir a CrearPlaylistPage
    private async void CrearPlaylist_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CrearPlaylistPage));
    }

    // Al seleccionar una playlist del menú lateral → ir a EditarPlaylistPage (Ver Playlist)
    private async void PlaylistsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault() as PlaylistItem;
        if (selected == null) return;

        if (sender is CollectionView cv)
            cv.SelectedItem = null;

        await Shell.Current.GoToAsync(nameof(EditarPlaylistPage), new Dictionary<string, object>
        {
            { "PlaylistId", selected.Id },
            { "PlaylistName", selected.Name }
        });
    }

    // ===================== ÁLBUMES =====================

    private async void OnAlbumSelected(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault() as Album;
        if (selected == null) return;

        if (sender is CollectionView cv)
            cv.SelectedItem = null;

        // Navegar a la página de detalle de álbum tipo Spotify
        await Shell.Current.GoToAsync(nameof(AlbumPage), new Dictionary<string, object>
        {
            { "Album", selected }
        });
    }

    // ===================== ADMIN =====================

    private async void AdminStats_Clicked(object sender, EventArgs e)
    {
        // Navegar a la página de estadísticas
        await Shell.Current.GoToAsync(nameof(AdminDashboardPage));
    }

    private async void AdminCanciones_Clicked(object sender, EventArgs e)
    {
        // Navegar a la página de gestionar canciones
        await Shell.Current.GoToAsync(nameof(GestionarCancionesPage));
    }

    // ===================== REPRODUCTOR =====================

    private void PlayPause_Clicked(object sender, EventArgs e)
    {
        if (_currentPlayer == null)
        {
            DisplayAlert("Reproductor", "No hay canción cargada aún.", "OK");
            return;
        }

        if (_currentPlayer.IsPlaying)
        {
            _currentPlayer.Pause();
            PlayPauseButton.Source = "icon_play.png";
        }
        else
        {
            _currentPlayer.Play();
            PlayPauseButton.Source = "icon_pause.png";
        }
    }

    private void PreviousButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Reproductor", "Anterior (lógica pendiente).", "OK");
    }

    private void NextButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Reproductor", "Siguiente (lógica pendiente).", "OK");
    }

    // ===================== CLASE AUXILIAR PARA PLAYLISTS =====================

    public class PlaylistItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Emoji { get; set; } = "🎵";

        public string DisplayName => $"{Emoji} {Name}";
    }
}
