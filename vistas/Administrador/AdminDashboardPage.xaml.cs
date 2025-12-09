using Metadata;
using MisDatosGrpc;
using pasadena_vistas.Models;
using pasadena_vistas.Services;
using System.Diagnostics;

namespace pasadena_vistas.vistas.Administrador;

public partial class AdminDashboardPage : ContentPage
{
    public AdminDashboardPage()
    {
        InitializeComponent();
        CargarEstadisticas();
    }

    private async void CargarEstadisticas()
    {
        try
        {
            var auth = new AuthService();
            var usuario = await auth.ObtenerPerfilUsuarioAsync();

            if (usuario == null || string.IsNullOrWhiteSpace(usuario.id.ToString()))
            {
                SetPlaceholders();
                return;
            }

            var client = MetadataService.Client;

            if (client == null)
            {
                Debug.WriteLine("MetadataService.Client es null");
                SetPlaceholders();
                return;
            }

            var request = new UserStatisticsRequest
            {
                UserId = usuario.id.ToString()
            };

            var response = await client.GetUserStatisticsAsync(request);


            // totalTime  -> Tiempo total reproducido (min)
            lblTotalUsuarios.Text = response.TotalTime.ToString("N0");

            // totalSongs -> Total de canciones escuchadas
            lblTotalCanciones.Text = response.TotalSongs.ToString("N0");

            // topGenres[0].name -> Género más escuchado
            lblTopGenero.Text = response.TopGenres.Count > 0
                ? response.TopGenres[0].Name
                : "-";

            // topSongs.Count -> Canciones en tu Top
            lblTotalPlaylists.Text = response.TopSongs.Count.ToString("N0");

            // topArtists.Count -> Artistas en tu Top
            lblTotalArtistas.Text = response.TopArtists.Count.ToString("N0");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar estadísticas: {ex}");
            SetPlaceholders();
        }
    }

    private void SetPlaceholders()
    {
        lblTotalUsuarios.Text = "-";
        lblTotalCanciones.Text = "-";
        lblTopGenero.Text = "-";
        lblTotalPlaylists.Text = "-";
        lblTotalArtistas.Text = "-";
    }

    private async void Volver_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
