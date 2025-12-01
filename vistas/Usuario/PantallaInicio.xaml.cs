using Grpc.Net.Client;
using Metadata;

using pasadena_vistas.Models;
using Streaming;
using System.Collections.ObjectModel;
using System.DirectoryServices;

using Plugin.Maui.Audio;
using Grpc.Core;

namespace pasadena_vistas.vistas.Usuario;

public partial class PantallaInicio : ContentPage
{
    private IAudioPlayer? _currentPlayer;

    private CancellationTokenSource _cts = new();
    public ObservableCollection<SearchResultClass> SearchResults { get; set; } = new();


    public PantallaInicio()
    {
        InitializeComponent();
        BindingContext = this;



        SizeChanged += OnSizeChanged;
    }

    public static class GrpcClientProvider
    {
        private static MetadataService.MetadataServiceClient? _client;
        private static StreamingService.StreamingServiceClient? _streamingService;

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

        public static StreamingService.StreamingServiceClient ClientStream
        {
            get
            {
                if (_streamingService == null)
                {
                    var channel = GrpcChannel.ForAddress("http://localhost:50052");
                    _streamingService = new StreamingService.StreamingServiceClient(channel);
                }

                return _streamingService;
            }
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

        // Solo reproducir si es canción
        if (item.Tipo == "Canción")
        {
            await ReproducirCancion(item.Id, item.Nombre);
        }

     // Quitar selección
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
        var client = GrpcClientProvider.ClientStream;

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
                Tipo = "Álbum",
                Imagen = ImageSource.FromStream(() => new MemoryStream(a.Cover.ToByteArray()))
            });
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


}