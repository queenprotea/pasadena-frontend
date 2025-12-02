using Grpc.Net.Client;
using Metadata;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using pasadena_vistas.Models;
using pasadena_vistas.Services;


namespace pasadena_vistas.vistas.Usuario;

    public partial class PantallaInicio : ContentPage
    {

    private readonly AuthService _authService;    
    private CancellationTokenSource _cts = new();
    public ObservableCollection<SearchResultClass> SearchResults { get; set; } = new();

   
    public PantallaInicio()
	    {
		    InitializeComponent();
            _authService = new AuthService();
            Loaded += PantallaInicio_Loaded;
	    }

    private async void PantallaInicio_Loaded(object sender, EventArgs e)
    {
        await CargarFotoUsuario();
    }

    public static class GrpcClientProvider
    {
        private static MetadataService.MetadataServiceClient? _client;

        public static MetadataService.MetadataServiceClient Client
        {
            get
            {
                if (_client == null)
                {
                    var channel = GrpcChannel.ForAddress("http://localhost:50051");
                    _client = new MetadataService.MetadataServiceClient(channel);
                }

                return _client;
            }
        }
    }

    private async Task CargarFotoUsuario()
    {
        bool tokenValido = await _authService.ValidarTokenAsync();

        if (!tokenValido)
        {
            await _authService.LimpiarSesionAsync();
            ProfileButton.Source = "user_profile_icon.png";
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


    private async void ProfileButton_Clicked(object sender, EventArgs e)
	    {
		    await Shell.Current.GoToAsync(nameof(PerfilUsuarioPage));
	    }
        private async void CrearPlaylist_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(CrearPlaylistPage));
        }

        private async void Playlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPlaylist = e.CurrentSelection.FirstOrDefault() as string;

            if (selectedPlaylist != null)
            {
                await Shell.Current.GoToAsync(nameof(EditarPlaylistPage));
                (sender as CollectionView).SelectedItem = null;
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

        try
        {
            await Task.Delay(350, _cts.Token);

            var results = await BuscarTodoAsync(text);

            SearchResults.Clear();
            foreach (var r in results)
                SearchResults.Add(r);

            SearchResultsView.IsVisible = SearchResults.Count > 0;
        }
        catch (TaskCanceledException)
        {
        }
    }



    private async void OnSearchResultSelected(object sender, SelectionChangedEventArgs e)
    {
            await Shell.Current.GoToAsync(nameof(vistas.Administrador.GestionarCancionesPage));
    }

    private async Task<List<SearchResultClass>> BuscarTodoAsync(string query)
{
    var results = new List<SearchResultClass>();

    // Crear cliente gRPC una sola vez
    var client = GrpcClientProvider.Client;

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
            Id = a.AlbumId.ToString(),
            Nombre = a.Name,
            Tipo = "Álbum",
            Imagen = ImageSource.FromStream(() => new MemoryStream(a.Cover.ToByteArray()))
        });
    }

    return results;
}



}