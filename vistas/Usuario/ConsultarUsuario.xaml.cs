using Metadata;
using pasadena_vistas.Models.Login;
using pasadena_vistas.Models.Playlist;
using pasadena_vistas.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace pasadena_vistas.vistas.Usuario;

public partial class ConsultarUsuario : ContentPage, INotifyPropertyChanged
{
    private readonly UserService _userService = new();
    private readonly AuthService _authService = new();
    private readonly PlayerService _player;
    public ObservableCollection<PlaylistItem> Playlists { get; } = new();

    private pasadena_vistas.Models.Login.Usuario _usuarioConsultado;
    public pasadena_vistas.Models.Login.Usuario UsuarioConsultado
    {
        get => _usuarioConsultado;
        set
        {
            _usuarioConsultado = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FotoPerfil));
        }
    }

    private pasadena_vistas.Models.Login.Usuario _usuarioActual;
    public pasadena_vistas.Models.Login.Usuario UsuarioActual
    {
        get => _usuarioActual;
        set
        {
            _usuarioActual = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MostrarSeguir));
            OnPropertyChanged(nameof(MostrarDejarSeguir));
        }
    }

    private bool _isFollowing;
    public bool IsFollowing
    {
        get => _isFollowing;
        set
        {
            _isFollowing = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MostrarSeguir));
            OnPropertyChanged(nameof(MostrarDejarSeguir));
        }
    }

    // ===== VISIBILIDAD =====
    public bool MostrarSeguir =>
        UsuarioActual != null && !IsFollowing && UsuarioActual.id != _usuarioConsultado.id;

    public bool MostrarDejarSeguir =>
        UsuarioActual != null && IsFollowing && UsuarioActual.id != _usuarioConsultado.id;

    // ===== AVATAR LOCAL =====
    public string FotoPerfil =>
        string.IsNullOrWhiteSpace(UsuarioConsultado?.profile_picture)
            ? "avatar1.png"
            : UsuarioConsultado.profile_picture;

    public ConsultarUsuario(string username, PlayerService player)
    {
        InitializeComponent();
        BindingContext = this;
        _player = player;

        Loaded += async (_, __) =>
        {
            await CargarUsuarioAsync(username);
        };
    }

    private async Task CargarUsuarioAsync(string username)
    {
        // Usuario consultado
        UsuarioConsultado = await _authService.GetUserByUsernameAsync(username);

        // Usuario logueado (puede ser null)
        UsuarioActual = await _authService.ObtenerPerfilUsuarioAsync();

        // ?? USAMOS is_following
        if (UsuarioActual != null)
        {
            IsFollowing = await _userService.IsFollowingAsync(
                UsuarioActual.id,
                UsuarioConsultado.id
            );
        }

        if (UsuarioConsultado != null)
        {
            var servicio = new PlaylistService();
            var playlists = await servicio.ObtenerPlaylistsActivasPublicasPorOwnerAsync(UsuarioConsultado.id);

            Playlists.Clear();
            foreach (var p in playlists)
            {
                Playlists.Add(new PlaylistItem
                {
                    Id = p.id.ToString(),
                    Name = p.name,
                    Emoji = "🎵",
                    CoverName = p.playlist_cover,
                    Cover = await servicio.ObtenerCoverPlaylistAsync(p.playlist_cover) ?? "defaul_cover.jpg"
                });
            }
            PlaylistsCollection.ItemsSource = Playlists;
        }
    }

    private async void Seguir_Clicked(object sender, EventArgs e)
    {
        await _userService.SeguirUsuarioAsync(UsuarioConsultado.id);
        IsFollowing = true;
    }

    private async void DejarSeguir_Clicked(object sender, EventArgs e)
    {
        await _userService.DejarDeSeguirUsuarioAsync(UsuarioConsultado.id);
        IsFollowing = false;
    }

    public new event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));



    private async void PlaylistsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault() as PlaylistItem;
        if (selected == null) return;

        if (sender is CollectionView cv)
            cv.SelectedItem = null;

        await AbrirPlaylistComoAlbum(int.Parse(selected.Id), selected.Name, selected.CoverName);
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
            CoverUrl = await servicio.ObtenerCoverPlaylistAsync(cover),
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

    public class PlaylistItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Emoji { get; set; } = "🎵";

        public ImageSource Cover { get; set; } = string.Empty;
        public string CoverName { get; set; } = string.Empty;

        public string DisplayName => $"{Emoji} {Name}";
    }
}
