using System.Diagnostics;
using pasadena_vistas.Services;
using pasadena_vistas.Models;
using Metadata;

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
            // 1?? Obtener usuario actual
            var auth = new AuthService();
            var usuario = await auth.ObtenerPerfilUsuarioAsync();

            if (usuario == null || string.IsNullOrWhiteSpace(usuario.id.ToString()))
            {
                lblTotalUsuarios.Text = "-";
                lblTotalCanciones.Text = "-";
                lblTopGenero.Text = "-";
                lblTotalPlaylists.Text = "-";
                lblTotalArtistas.Text = "-";
                return;
            }

            // 2?? Crear request gRPC
            var client = Services.MetadataService.Client; // tu cliente gRPC
            if (client == null)
                throw new InvalidOperationException("MetadataService.Client no inicializado");

            var request = new UserStatisticsRequest
            {
                UserId = usuario.id.ToString() // <-- string
            };

            var response = await client.GetUserStatisticsAsync(request);

            // 3?? Asignar valores a los labels
            lblTotalUsuarios.Text = response.TotalTime.ToString(); // opcional, si quieres mostrar nombre
            lblTotalCanciones.Text = response.TotalSongs.ToString("N0"); // ej: 12,345
            lblTopGenero.Text = response.TopGenres.Count > 0 ? response.TopGenres[0].Name : "-";
            lblTotalPlaylists.Text = response.TopSongs.Count.ToString("N0"); // si quieres contar top canciones
            lblTotalArtistas.Text = response.TopArtists.Count.ToString("N0");

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar estadísticas: {ex}");
            lblTotalUsuarios.Text = "-";
            lblTotalCanciones.Text = "-";
            lblTopGenero.Text = "-";
            lblTotalPlaylists.Text = "-";
            lblTotalArtistas.Text = "-";
        }
    }


    private async void Volver_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}