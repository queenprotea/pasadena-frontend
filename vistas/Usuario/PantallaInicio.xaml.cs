using Grpc.Net.Client;
using Metadata;
using pasadena_vistas.Config;
using pasadena_vistas.Models;
using Streaming;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using pasadena_vistas.Models;
using pasadena_vistas.Services;

using Plugin.Maui.Audio;
using Grpc.Core;
using pasadena_vistas.vistas.Administrador;
using pasadena_vistas.vistas.Usuario;
using pasadena_vistas.Services;

namespace pasadena_vistas.vistas.Usuario;

public partial class PantallaInicio : ContentPage
{
    private IAudioPlayer? _currentPlayer;

    private readonly AuthService _authService;    
    private CancellationTokenSource _cts = new();
    public ObservableCollection<SearchResultClass> SearchResults { get; set; } = new();
    public ObservableCollection<pasadena_vistas.Models.Album> AlbumsRecomendados { get; set; } = new();
    public ObservableCollection<PlaylistItem> Playlists { get; } = new();
    private PlayerService _player;

    public PantallaInicio(PlayerService player)
    {
        InitializeComponent();
        BindingContext = this;

        _authService = new AuthService();
        Loaded += PantallaInicio_Loaded;

        _player = player;

        SizeChanged += OnSizeChanged;
        _player.OnSongChanged += song =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SongName.Text = song.title;
                SongArtist.Text = song.artist;
            });
        };
    }

    private async void PantallaInicio_Loaded(object sender, EventArgs e)
    {
        await CargarFotoUsuario();
        await CargarBotonCrearPlaylistr();
    }

    private void ToggleMenu_Clicked(object sender, EventArgs e)
    {
        LeftMenu.IsVisible = !LeftMenu.IsVisible;
    }

    private async Task CargarBotonCrearPlaylistr()
    {
        bool tokenValido = await _authService.ValidarTokenAsync();

        if (!tokenValido)
        {
            CreatePlaylistButton.IsVisible = false;
        }
        else
        {
            CreatePlaylistButton.IsVisible = true;
        }

    }

    private async Task CargarFotoUsuario()
    {
        try
        {
            bool tokenValido = await _authService.ValidarTokenAsync();

            if (!tokenValido)
            {
                _authService.LimpiarSesion();
                ProfileButton.Source = "default_user.png";
                return;
            }

            var fotoPerfil = await SecureStorage.GetAsync("profile_picture");

            if (string.IsNullOrWhiteSpace(fotoPerfil))
            {
                ProfileButton.Source = "user_profile_icon.png";
                return;
            }

            string fotoUrl = Config.Config.ProfilePic(fotoPerfil);

            ProfileButton.Source = ImageSource.FromUri(new Uri(fotoUrl));
        }
        catch
        {
            ProfileButton.Source = "user_profile_icon.png";
        }
    }


    private async void ProfileButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PerfilUsuarioPage));
    }
    private async void CrearPlaylist_Clicked(object sender, EventArgs e)
    {


        await Shell.Current.GoToAsync(nameof(CrearPlaylistPage));
    }
    private async void Playlist_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // CurrentSelection puede ser vacío → validamos
        var selectedPlaylist = e.CurrentSelection?.FirstOrDefault() as string;

        if (selectedPlaylist is not null && sender is CollectionView cv)
        {
            await Shell.Current.GoToAsync(nameof(EditarPlaylistPage));

            // limpiar selección
            cv.SelectedItem = null;
        }
    }

    private async void AdminUsuarios_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(vistas.Administrador.GestionarUsuariosPage));
    }
    private async void AdminCanciones_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(vistas.Administrador.GestionarCancionesPage));
    }

    private async void SearchSong_clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(vistas.Administrador.GestionarCancionesPage));
    }
    private async void SearchSong_textChanged(object sender, TextChangedEventArgs e)
    {
        string text = e.NewTextValue;

        if (string.IsNullOrWhiteSpace(text))
        {
            SearchResultsView.IsVisible = false;
            SearchResults.Clear();
            return;
        }

        _cts.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            await Task.Delay(350, token);

            var results = await BuscarTodoAsync(text);

            if (token.IsCancellationRequested)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                SearchResults.Clear();
                foreach (var r in results)
                    SearchResults.Add(r);

                SearchResultsView.IsVisible = SearchResults.Count > 0;
            });
        }
        catch (TaskCanceledException)
        {
            // ignorar
        }
    }




    private async void OnSearchResultSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
            return;

        var item = e.CurrentSelection[0] as SearchResultClass;
        if (item == null)
            return;

        switch (item.Tipo)
        {
            case "Canción":
                await ReproducirCancion(item.Id, item.Nombre);
                break;

            case "Album":
                if (int.TryParse(item.Id, out int albumId))
                {
                    await AbrirAlbum(albumId);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "El ID del álbum no es un número válido.",
                        "OK"
                    );
                }
                break;

            case "Playlist":
                
                break;

            default:
                break;
        }

    ((CollectionView)sender).SelectedItem = null;
    }



    private async Task ReproducirCancion(string songId, string title)
    {
        SongName.Text = title;
        // SongArtist.Text = artist;


        await StartStreaming(songId);
    }
    private async Task StartStreaming(string songId)
    {
        var client = Services.StreamingService.Client;

        using var call = client.StreamSong(new StreamRequest { SongId = songId });

        var ms = new MemoryStream();

        while (await call.ResponseStream.MoveNext())
        {
            var bytes = call.ResponseStream.Current.Chunk.ToByteArray();
            ms.Write(bytes, 0, bytes.Length);
        }

        ms.Position = 0;

        _currentPlayer?.Stop();
        _currentPlayer?.Dispose();

        _currentPlayer = AudioManager.Current.CreatePlayer(ms);
        _currentPlayer.Play();


    }



    private async Task<List<SearchResultClass>> BuscarTodoAsync(string query)
    {
        var results = new List<SearchResultClass>();
        ;
        // Crear cliente gRPC una sola vez
        var client =  Services.MetadataService.Client;

        // ===============================
        // 🔎 BUSCAR CANCIONES
        // ===============================
        var songResponse = await client.SearchSongsAsync(new SearchRequest { Query = query });

        foreach (var c in songResponse.Songs)
        {
            results.Add(new SearchResultClass
            {
                Id = c.SongId,
                Tipo = "Canción",
                Nombre = c.Title,
                Imagen = ImageSource.FromStream(() => new MemoryStream(c.AlbumCover.ToByteArray()))
            });
        }

        // ===============================
        // 🔎 BUSCAR ARTISTAS
        // ===============================
        var artistResponse = await client.SearchArtistsAsync(new SearchRequest { Query = query });

        foreach (var a in artistResponse.Artists)
        {
            results.Add(new SearchResultClass
            {
                Id = a.ArtistId.ToString(),
                Nombre = a.Name,
                Tipo = "Artista",
                Imagen = ImageSource.FromFile("default_artist.png")
            });
        }

        // ===============================
        // 🔎 BUSCAR ALBUMS
        // ===============================
        var albumResponse = await client.SearchAlbumsAsync(new SearchRequest { Query = query });

        foreach (var a in albumResponse.Albums)
        {
            results.Add(new SearchResultClass
            {
                Id = a.Id.ToString(),
                Nombre = a.Name,
                Tipo = "Album",
                Imagen = ImageSource.FromStream(() => new MemoryStream(a.Cover.ToByteArray()))
            });
        }


        // ===============================
        // 🔎 BUSCAR USUARIOS
        // ===============================

        var auth = new AuthService();
        var userResponse = await auth.GetUserByUsernameAsync(query.ToString());

        if (userResponse != null)
        {
            results.Add(new SearchResultClass
            {
                Id = userResponse.id.ToString(),
                Nombre = userResponse.full_name,
                Tipo = "Usuario",
                Imagen = ImageSource.FromFile("default_artist.png")
            });
        }
        else
        {
            Console.WriteLine("Usuario no encontrado o API devolvió error.");
        }

        return results;
    }

    void OnSizeChanged(object? sender, EventArgs e)
    {
        LeftMenu.IsVisible = this.Width > 600; // Desktop only
    }

    void ToggleMenu(object sender, EventArgs e)
    {
        LeftMenu.IsVisible = !LeftMenu.IsVisible;
    }
    private void PlayPause_Clicked(object sender, EventArgs e)
    {
        if (_currentPlayer == null) return;

        if (_currentPlayer.IsPlaying)
            _currentPlayer.Pause();
        else
            _currentPlayer.Play();
    }

    private void PreviousButton_Clicked(object sender, EventArgs e)
    {
        if (_currentPlayer == null) return;

        if (_currentPlayer.IsPlaying)
            _currentPlayer.Pause();
        else
            _currentPlayer.Play();
    }

    private void NextButton_Clicked(object sender, EventArgs e)
    {
        if (_currentPlayer == null) return;

        if (_currentPlayer.IsPlaying)
            _currentPlayer.Pause();
        else
            _currentPlayer.Play();
    }

    private async void Dashboard_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AdminDashboardPage));
    }

    

    private async void OnAlbumSelected(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AlbumPage));
    }

    private async Task AbrirAlbum(int albumId)
    {
        try
        {
            var client = Services.MetadataService.Client;

            var grpcResponse = await client.GetAlbumByIdAsync(
                new GetAlbumByIdRequest { Id = albumId }
            );

            if (grpcResponse.Album == null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "No se pudo obtener el álbum.",
                    "OK"
                );
                return;
            }

            // Convertir respuesta gRPC → Modelo local
            var albumModel = ConvertirAlbum(grpcResponse.Album);

            // Abrir la página
            await Application.Current.MainPage.Navigation
                .PushAsync(new AlbumPage(albumModel, _player));
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }


    private pasadena_vistas.Models.Album ConvertirAlbum(AlbumFull dto)
    {
        var album = new pasadena_vistas.Models.Album
        {
            Name = dto.Name,
            Artist = dto.ArtistName,        // viene directo del proto
            Year = dto.ReleaseDate ?? "",
            CoverUrl = 0,                    // tú luego lo cambias
            Songs = new ObservableCollection<pasadena_vistas.Models.Song>()
        };

        int index = 1;
        foreach (var s in dto.Songs)
        {
            album.Songs.Add(new pasadena_vistas.Models.Song
            {
                Id = s.SongId,                      // tu modelo usa INT, pero proto usa UUID → no compatible
                title = s.Title,
                artist = s.Artist,
                genre = s.Genre,
                duration = s.Duration,
                album = dto.Name,
                year = dto.ReleaseDate ?? "",
                album_cover = null,
                songNumber = index++
            });
        }

        return album;
    }

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

    private async void OnAlbumSelected(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault() as Models.Album;
        if (selected == null) return;

        if (sender is CollectionView cv)
            cv.SelectedItem = null;

        // Navegar a la página de detalle de álbum tipo Spotify
        await Shell.Current.GoToAsync(nameof(AlbumPage), new Dictionary<string, object>
        {
            { "Album", selected }
        });
    }

    private async void AdminStats_Clicked(object sender, EventArgs e)
    {
        // Navegar a la página de estadísticas
        await Shell.Current.GoToAsync(nameof(AdminDashboardPage));
    }



    public class AlbumResponse
    {
        public AlbumDTO album { get; set; }
    }

    public class AlbumDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public int artist_id { get; set; }
        public string artist_name { get; set; }
        public string cover { get; set; }
        public string release_date { get; set; }
        public List<SongDTO> songs { get; set; }
    }

    public class SongDTO
    {
        public string song_id { get; set; }
        public string title { get; set; }
        public string artist { get; set; }
        public string genre { get; set; }
        public double duration { get; set; }
        public int artist_id { get; set; }
        public int genre_id { get; set; }
    }


    public class PlaylistItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Emoji { get; set; } = "🎵";

        public string DisplayName => $"{Emoji} {Name}";
    }

}