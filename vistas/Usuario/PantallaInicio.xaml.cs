using Grpc.Core;
using Grpc.Net.Client;
using Metadata;
using pasadena_vistas.Config;
using pasadena_vistas.Models;

using pasadena_vistas.Models.Playlist;
using pasadena_vistas.Services;

using pasadena_vistas.vistas.Administrador;
using pasadena_vistas.vistas.Usuario;
using Plugin.Maui.Audio;
using Streaming;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Threading.Tasks;


namespace pasadena_vistas.vistas.Usuario;

public partial class PantallaInicio : ContentPage
{
    private IAudioPlayer? _currentPlayer;

    private readonly AuthService _authService;    
    private CancellationTokenSource _cts = new();
    public ObservableCollection<SearchResultClass> SearchResults { get; set; } = new();
    public ObservableCollection<pasadena_vistas.Models.Album> AlbumsRecomendados { get; set; } = new();
    public ObservableCollection<pasadena_vistas.Models.Song> SongsRecomendadas { get; set; } = new();
    public ObservableCollection<pasadena_vistas.Models.Song> TopSongsInicio { get; set; } = new();


    public ObservableCollection<PlaylistItem> Playlists { get; } = new();
    private readonly PlayerService _player;


    public PantallaInicio(PlayerService player)
    {
        InitializeComponent();
        BindingContext = this;

        _authService = new AuthService();
        Loaded += PantallaInicio_Loaded;

        _player = player;

        SizeChanged += OnSizeChanged;

        _player.OnPlayStateChanged += (isPlaying) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PlayPauseButton.Source = isPlaying ? "icon_pause.png" : "icon_play.png";
            });
        };


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
       

        if (MenuLateral.IsVisible)
        {
            LeftMenu.IsVisible = false;
            MenuLateral.IsVisible = false;
            MainGrid.ColumnDefinitions[0].Width = new GridLength(0);
            MainGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        }
        else
        {
            LeftMenu.IsVisible = true;
            MenuLateral.IsVisible = true;
            MainGrid.ColumnDefinitions[0].Width = new GridLength(220);
            MainGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        }
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
            await DisplayAlert("Error", "Hubo un error al cargar tus datos de perfil.", "OK");
            ProfileButton.Source = "user_profile_icon.png";
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await CargarUltimosAlbumesAsync();
            await CargarUltimasCancionesAsync();
            await CargarTopCancionesInicioAsync();

            bool tokenValido = await _authService.ValidarTokenAsync();

            if (tokenValido)
            {
                var servicio = new PlaylistService();
                // Aquí debes pasar el owner_id del usuario autenticado
                var owner = await _authService.ObtenerPerfilUsuarioAsync();

                if (owner != null)
                {
                    EstadisticasButton.IsVisible = true;
                    GestionCancionesButton.IsVisible = owner.role_id == 1;
                }


                var playlists = await servicio.ObtenerPlaylistsActivasPorOwnerAsync(owner.id);

                Playlists.Clear();

                foreach (var p in playlists)
                {
                    // Mapear PlaylistRespuesta -> PlaylistItem
                    Playlists.Add(new PlaylistItem
                    {
                        Id = p.id.ToString(),   
                        Name = p.name,          
                        Emoji = "🎵",
                        Cover = p.playlist_cover ?? ""
                    });
                }

                PlaylistsCollection.ItemsSource = Playlists;
            }
            else
            {
                // limpia la UI cuando no hay sesión
                EstadisticasButton.IsVisible = false;
                GestionCancionesButton.IsVisible = false;
                Playlists.Clear();
                PlaylistsCollection.ItemsSource = Playlists;
            }

            
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
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
        string text = SearchBar.Text;

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
               
                var client = Services.MetadataService.Client;
                var searchResponse = await client.GetSongByIdAsync(new GetSongByIdRequest { SongId = item.Id });

                if (searchResponse == null)
                {
                    Debug.WriteLine($"Canción no encontrada: {item.Nombre}");
                    return;
                }
                var songData = searchResponse.Song;

                // Creamos modelo local de Song
                var songToPlay = new pasadena_vistas.Models.Song
                {
                    Id = songData.SongId,
                    title = songData.Title,
                    artist = songData.Artist,
                    album = songData.Album,
                    genre = songData.Genre,
                    duration = songData.Duration
                };

                // Reproducimos la canción usando PlayerService
                await _player.PlaySongAsync(songToPlay);

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
                if (int.TryParse(item.Id, out int playlistId))
                {
                    await AbrirPlaylistComoAlbum(playlistId, item.Nombre, item.Detalles);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "El ID de la playlist no es un número válido.",
                        "OK"
                    );
                }
                break;

            case "Usuario":

                await Application.Current.MainPage.Navigation
               .PushAsync(new ConsultarUsuario(item.Nombre, _player));

                break;
            default:
                break;
        }

    ((CollectionView)sender).SelectedItem = null;
    }

    private async Task<List<SearchResultClass>> BuscarTodoAsync(string query)
    {
        var results = new List<SearchResultClass>();
        var client = Services.MetadataService.Client;

        // ========== BUSCAR CANCIONES ==========
        try
        {
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
        }
        catch (Grpc.Core.RpcException ex)
        {
           
            // opcional: DisplayAlert("Error", "No se pudo conectar al servidor.", "OK");
        }

        // ========== BUSCAR ARTISTAS ==========
        try
        {
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
        }
        catch (Grpc.Core.RpcException ex)
        {
            
        }

        // ========== BUSCAR ALBUMS ==========
        try
        {
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
        }
        catch (Grpc.Core.RpcException ex)
        {
            
        }

        // ========== BUSCAR PLAYLISTS ==========
        try
        {
            var playlist = new PlaylistService();
            var playlistResponse = await playlist.ObtenerPlaylistsActivasPublicasPorNombreAsync(query);

            foreach (var a in playlistResponse)
            {
                results.Add(new SearchResultClass
                {
                    Id = a.id.ToString(),
                    Nombre = a.name,
                    Tipo = "Playlist",
                    Detalles = a.playlist_cover
                });
            }
        }
        catch (Exception ex)
        {
            
        }

        // ========== BUSCAR USUARIOS (REST API) ==========
        try
        {
            var auth = new AuthService();
            var userResponse = await auth.GetUserByUsernameAsync(query);

            if (userResponse != null)
            {
                results.Add(new SearchResultClass
                {
                    Id = userResponse.id.ToString(),
                    Nombre = userResponse.username,
                    Tipo = "Usuario",
                    Imagen = ImageSource.FromFile("default_artist.png")
                });
            }
        }
        catch (Exception ex)
        {
            
        }

        return results;
    }


    void OnSizeChanged(object? sender, EventArgs e)
    {
        LeftMenu.IsVisible = this.Width > 600; // Desktop only

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

    void ToggleMenu(object sender, EventArgs e)
    {
        LeftMenu.IsVisible = !LeftMenu.IsVisible;
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

    private async void Dashboard_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AdminDashboardPage));
    }

    

    private async void OnAlbumSelected(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault() as Models.Album;
        if (selected == null) return;

        if (sender is CollectionView cv)
            cv.SelectedItem = null;
        await AbrirAlbum(selected.Id);
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
                .PushAsync(new AlbumPage(albumModel, _player, -1, -1));
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
            CoverUrl = dto.Cover != null
            ? ImageSource.FromStream(() => new MemoryStream(dto.Cover.ToByteArray()))
            : null,                  // tú luego lo cambias
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

        await AbrirPlaylistComoAlbum(int.Parse(selected.Id), selected.Name, selected.Cover);
    }

    private async void AdminStats_Clicked(object sender, EventArgs e)
    {
        // Navegar a la página de estadísticas
        await Shell.Current.GoToAsync(nameof(AdminDashboardPage));
    }


    private async Task<List<pasadena_vistas.Models.Song>> ConvertirPlaylistSongsAsync(
    List<PlaylistSongs> lista)
    {
        var grpc = Services.MetadataService.Client;
        var canciones = new List<pasadena_vistas.Models.Song>();

        foreach (var item in lista)
        {
            var resp = await grpc.GetSongByIdAsync(
                new GetSongByIdRequest { SongId = item.song_id }
            );

            if (resp.Song == null)
                continue;

            canciones.Add(new pasadena_vistas.Models.Song
            {
                Id = resp.Song.SongId,
                title = resp.Song.Title,
                artist = resp.Song.Artist,
                album = resp.Song.Album,
                year = "",
                genre = resp.Song.Genre,
                duration = resp.Song.Duration,
                album_cover = null,
                file_data = null
            });
        }

        return canciones;
    }

    private async Task<pasadena_vistas.Models.Album> ConvertirPlaylistEnAlbumModel(
        string nombrePlaylist,
        string cover,
        List<pasadena_vistas.Models.Song> canciones)
    {
        var servicio = new PlaylistService();

        var album = new pasadena_vistas.Models.Album
        {
            Name = nombrePlaylist,
            Artist = "Varios artistas",
            Year = "",
            CoverUrl = await servicio.ObtenerCoverPlaylistAsync(cover) ,
            Songs = new ObservableCollection<pasadena_vistas.Models.Song>()
        };

        int index = 1;
        foreach (var s in canciones)
        {
            s.songNumber = index++;   // numeración visual
            album.Songs.Add(s);
        }

        return album;
    }
    

    private async Task AbrirPlaylistComoAlbum(int playlistId, string playlistName, string cover)
    {
        try
        {
            var service = new PlaylistService();

            int idUsuarioActual; 

            if (await _authService.ObtenerPerfilUsuarioAsync() == null)
                idUsuarioActual = -1;
            else
                idUsuarioActual = (await _authService.ObtenerPerfilUsuarioAsync()).id;

            var listaIds = await service.ObtenerCancionesDePlaylistAsync(playlistId);

            var canciones = await ConvertirPlaylistSongsAsync(listaIds);

            var albumModel = await ConvertirPlaylistEnAlbumModel(playlistName, cover, canciones);

            await Application.Current.MainPage.Navigation
                .PushAsync(new AlbumPage(albumModel, _player, playlistId, idUsuarioActual));
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task CargarUltimosAlbumesAsync()
    {
        try
        {
            var client = Services.MetadataService.Client;

            var response = await client.GetLatestAlbumsAsync(
                new LatestAlbumsRequest { Limit = 5 }
            );

            AlbumsRecomendados.Clear();

            foreach (var a in response.Albums)
            {
                AlbumsRecomendados.Add(new pasadena_vistas.Models.Album
                {
                    Id = a.Id,
                    Name = a.Name,
                    Artist = "", // si luego quieres mostrar artista, lo agregamos
                    CoverUrl = a.Cover != null && a.Cover.Length > 0
                        ? ImageSource.FromStream(() => new MemoryStream(a.Cover.ToByteArray()))
                        : ImageSource.FromFile("default_album.png")
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("❌ Error cargando álbumes recientes: " + ex.Message);
        }
    }

    private async Task CargarUltimasCancionesAsync()
    {
        try
        {
            var client = Services.MetadataService.Client;

            var response = await client.GetLatestSongsAsync(
                new LatestSongsRequest { Limit = 10 }
            );

            SongsRecomendadas.Clear();

            foreach (var s in response.Songs)
            {
                SongsRecomendadas.Add(new pasadena_vistas.Models.Song
                {
                    Id = s.SongId,
                    title = s.Title,
                    artist = s.Artist,
                    album = s.Album,
                    genre = s.Genre,
                    duration = s.Duration
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("❌ Error cargando canciones recomendadas: " + ex.Message);
        }
    }

    private async void OnSongRecommendationSelected(object sender, SelectionChangedEventArgs e)
    {
        var song = e.CurrentSelection.FirstOrDefault() as pasadena_vistas.Models.Song;
        if (song == null)
            return;

        if (sender is CollectionView cv)
            cv.SelectedItem = null;

        await _player.PlaySongAsync(song);
    }

    private async Task CargarTopCancionesInicioAsync()
    {
        try
        {
            TopSongsSection.IsVisible = false;
            TopSongsInicio.Clear();

            var auth = new AuthService();
            var usuario = await auth.ObtenerPerfilUsuarioAsync();

            // ❌ No hay usuario
            if (usuario == null)
                return;

            var client = Services.MetadataService.Client;
            if (client == null)
                return;

            var request = new UserStatisticsRequest
            {
                UserId = usuario.id.ToString()
            };

            var response = await client.GetUserStatisticsAsync(request);

            if (response.TopSongs == null || response.TopSongs.Count == 0)
                return;
            // 4) TOP 5 canciones
            foreach (var s in response.TopSongs.Take(5))
            {
                TopSongsInicio.Add(new pasadena_vistas.Models.Song
                {
                    Id = s.SongId,
                    title = s.Title,
                   
                });
            }
            TopSongsSection.IsVisible = true;
        }
        catch (Exception ex)
        {
            
        }
    }

    private async void OnTopSongSelected(object sender, SelectionChangedEventArgs e)
    {
        var song = e.CurrentSelection.FirstOrDefault() as pasadena_vistas.Models.Song;
        if (song == null) return;

        if (sender is CollectionView cv)
            cv.SelectedItem = null;

        await _player.PlaySongAsync(song);
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
        public ImageSource cover { get; set; }
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

        public string Cover { get; set; } = string.Empty;

        public string DisplayName => $"{Emoji} {Name}";
    }


}
