using Grpc.Net.Client;
using Metadata;
using pasadena_vistas.Models;
using pasadena_vistas.vistas.Administrador;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using pasadena_vistas.Services;
using pasadena_vistas.Models;


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
    
    private async void AgregarCancion_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AgregarCancionAdminPage));
    }

    private async void Eliminar_Clicked(object sender, EventArgs e)
    {
        bool confirmado = await DisplayAlert("Confirmar", "¿Estás seguro de que quieres eliminar esta canción?", "Sí, eliminar", "Cancelar");
        var boton = sender as Button;
        var item = boton?.CommandParameter;

        if (item == null)
            return;

        // CAST del item al tipo real
        var modelo = item as SearchResultClass; // Cambia al tipo que uses

        if (modelo == null)
            return;

        if (confirmado)
        {
            var client = Services.MetadataService.Client;
            await client.DeleteSongAsync(new SongRequest {SongId = modelo.Id});
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
                    Tipo = "Canción",
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



}