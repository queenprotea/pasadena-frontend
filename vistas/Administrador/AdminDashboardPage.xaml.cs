using Metadata;
using pasadena_vistas.Models;
using pasadena_vistas.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace pasadena_vistas.vistas.Administrador;

public partial class AdminDashboardPage : ContentPage
{

    public ObservableCollection<TopSongItem> TopSongs { get; } = new();
    public ObservableCollection<TopArtistItem> TopArtists { get; } = new();
    public ObservableCollection<string> TopGenres { get; } = new();
    public ObservableCollection<LastPlayedItem> LastPlayed { get; } = new();

    public AdminDashboardPage()
    {
        BindingContext = this;

        InitializeComponent();
        CargarEstadisticas();
    }


    private async void CargarEstadisticas()
    {
        try
        {
            // 1) Usuario actual
            var auth = new AuthService();
            var usuario = await auth.ObtenerPerfilUsuarioAsync();

            if (usuario == null || string.IsNullOrWhiteSpace(usuario.id.ToString()))
            {
                SetPlaceholders();
                return;
            }

            // 2) Cliente gRPC
            var client = pasadena_vistas.Services.MetadataService.Client;
            if (client == null)
            {
                Debug.WriteLine("MetadataService.Client es null");
                SetPlaceholders();
                return;
            }

            // 3) Request
            var request = new UserStatisticsRequest
            {
                UserId = usuario.id.ToString()
            };

            var response = await client.GetUserStatisticsAsync(request);
            // ==============================
            // TOP CANCIONES
            // ==============================
            TopSongs.Clear();
            foreach (var song in response.TopSongs)
            {
                TopSongs.Add(new TopSongItem
                {
                    Title = song.Title,
                    PlayCount = song.PlayCount
                });
            }

            // ==============================
            // TOP ARTISTAS
            // ==============================
            TopArtists.Clear();
            foreach (var artist in response.TopArtists)
            {
                TopArtists.Add(new TopArtistItem
                {
                    Name = artist.Name,
                    PlayCount = artist.PlayCount
                });
            }

            // ==============================
            // ÚLTIMAS REPRODUCCIONES
            // ==============================
            LastPlayed.Clear();
            foreach (var lp in response.LastPlayed)
            {
                LastPlayed.Add(new LastPlayedItem
                {
                    Title = lp.Title,
                    LastPlay = DateTime.Parse(lp.LastPlay)
                });
            }

            // ==============================
            // TIEMPO TOTAL ? MIN : SEG
            // ==============================
            var totalSeconds = (int)Math.Round(response.TotalTime);
            var time = TimeSpan.FromSeconds(totalSeconds);

            // Formato: mm:ss
            lblTotalUsuarios.Text = $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}";

            // ==============================
            // RESTO DE ESTADÍSTICAS
            // ==============================

            // Total de canciones escuchadas
            lblTotalCanciones.Text = response.TotalSongs.ToString("N0");

            // Género más escuchado
            lblTopGenero.Text = response.TopGenres.Count > 0
                ? response.TopGenres[0].Name
                : "-";

            // Canciones en tu Top
            lblTotalPlaylists.Text = response.TopSongs.Count > 0
    ? response.TopSongs[0].Title
    : "-";

            // Artistas en tu Top
            lblTotalArtistas.Text = response.TopArtists.Count > 0
     ? response.TopArtists[0].Name
     : "-";
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

public class TopSongItem
{
    public string Title { get; set; }
    public int PlayCount { get; set; }
}

public class TopArtistItem
{
    public string Name { get; set; }
    public int PlayCount { get; set; }
}

public class LastPlayedItem
{
    public string Title { get; set; }
    public DateTime LastPlay { get; set; }
}

