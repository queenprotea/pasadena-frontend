using Grpc.Net.Client;
using Metadata;
using pasadena_vistas.Models;
using pasadena_vistas.vistas.Administrador;
using System.Collections.ObjectModel;
using System.DirectoryServices;


namespace pasadena_vistas.vistas.Administrador;

public partial class GestionarCancionesPage : ContentPage
{
    public ObservableCollection<SearchResultClass> SearchResults { get; set; } = new();
    private CancellationTokenSource _cts = new();
    public GestionarCancionesPage()
    {
        InitializeComponent();
        BindingContext = this;
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
    private async void AgregarCancion_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AgregarCancionAdminPage));
    }

    private async void Eliminar_Clicked(object sender, EventArgs e)
    {
        bool confirmado = await DisplayAlert("Confirmar", "¿Estás seguro de que quieres eliminar esta canción?", "Sí, eliminar", "Cancelar");
        if (confirmado)
        {
            await DisplayAlert("Éxito", "Canción eliminada (simulación)", "Aceptar");
        }
    }

           
    private async void SearchSong_textChanged(object sender, TextChangedEventArgs e)
    {
        string text = e.NewTextValue;

        if (string.IsNullOrWhiteSpace(text))
        {
            SongsCollectionView.IsVisible = false;
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

                SongsCollectionView.IsVisible = SearchResults.Count > 0;
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

        // Cliente gRPC
        var client = GrpcClientProvider.Client;


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

        return results;
    }


}