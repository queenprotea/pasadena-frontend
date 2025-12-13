namespace pasadena_vistas.vistas.Usuario;

using Metadata;
using Microsoft.Maui.Storage;
using pasadena_vistas.Models;
using pasadena_vistas.Models.Playlist;
using pasadena_vistas.Services;
using System.Collections.ObjectModel;

public partial class AgregarCancionPage : ContentPage
{
    private readonly PlaylistService _playlistService;
    private List<PlaylistSongs> cancionesEnPlaylist;
    private int playlistId;

    private CancellationTokenSource _cts = new();
    public ObservableCollection<SearchResultClass> SearchResults { get; set; } = new();

    public AgregarCancionPage(int playlistId)
    {
        InitializeComponent();
        _playlistService = new PlaylistService(); // inicializar aquí
        this.playlistId = playlistId;
        BindingContext = this;
        _ = ObtenerCancionesEnPlaylistAsync(playlistId); // fire-and-forget controlado
        ActualizarBotones();
    }

    private void ActualizarBotones()
    {
        foreach (var item in SearchResults)
        {
            // Verificar si el Id del resultado ya está en la playlist
            bool existe = cancionesEnPlaylist.Any(c => c.song_id == item.Id);

            // Buscar el contenedor visual del item
            var container = ResultadosBusquedaCollection
                .FindByName<Button>("AgregarButton"); // si le pones x:Name en el DataTemplate

            if (container != null)
            {
                if (existe)
                {
                    container.Text = "Remover";
                    container.BackgroundColor = Colors.Red;
                }
                else
                {
                    container.Text = "+";
                    container.BackgroundColor = Color.FromArgb("#28a745");
                }
            }
        }
    }


    private async Task ObtenerCancionesEnPlaylistAsync(int playlistId)
    {
        try
        {
            cancionesEnPlaylist = await _playlistService.ObtenerCancionesDePlaylistAsync(playlistId);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void AgregarCancion_Clicked(object sender, EventArgs e)
    {
        var boton = sender as Button;
        var modelo = boton?.BindingContext as SearchResultClass;
        if (modelo == null) return;

        bool existe = cancionesEnPlaylist.Any(c => c.song_id == modelo.Id);

        if (existe)
        {
            // Remover
            await _playlistService.RemoverCancionDePlaylistAsync(playlistId, modelo.Id);
            await ObtenerCancionesEnPlaylistAsync(playlistId); // refrescar lista
            boton.Text = "+";
            boton.BackgroundColor = Color.FromArgb("#28a745");
        }
        else
        {
            // Agregar
            int nuevaPosicion = cancionesEnPlaylist.Count > 0
                ? cancionesEnPlaylist.Max(c => c.position) + 1
                : 1;
            
            await _playlistService.AgregarCancionAPlaylistAsync(playlistId, modelo.Id, nuevaPosicion);
            await ObtenerCancionesEnPlaylistAsync(playlistId); // refrescar lista
            boton.Text = "-";
            boton.BackgroundColor = Colors.Red;
        }
        
    }


    private async void SearchSong_textChanged(object sender, TextChangedEventArgs e)
    {
        string text = e.NewTextValue;

        if (string.IsNullOrWhiteSpace(text))
        {
            ResultadosBusquedaCollection.IsVisible = false;
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

                ResultadosBusquedaCollection.IsVisible = SearchResults.Count > 0;

            });
        }
        catch (TaskCanceledException)
        {
            // ignorar
        }


    }
    private async Task<List<SearchResultClass>> BuscarTodoAsync(string query)
    {
        var results = new List<SearchResultClass>();

        try
        {
            var client = Services.MetadataService.Client;

            // Intentar hacer la llamada gRPC
            var songResponse = await client.SearchSongsAsync(
                new SearchRequest { Query = query }
            );

            foreach (var c in songResponse.Songs)
            {
                results.Add(new SearchResultClass
                {
                    Id = c.SongId,
                    Tipo = c.Artist,
                    Nombre = c.Title,
                    Imagen = ImageSource.FromStream(
                        () => new MemoryStream(c.AlbumCover.ToByteArray())
                    )
                });
            }
        }
        catch (Grpc.Core.RpcException rpcEx)
        {

        }
        catch (Exception ex)
        {

        }

        return results; // Devuelve la lista (vacía si hubo error)
    }


    private async void BtnVolver_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PantallaInicio");
    }
}