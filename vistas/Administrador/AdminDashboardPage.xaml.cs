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
        // SIMULACIÓN DE DATOS (AQUÍ CONECTARÁS EL BACKEND LUEGO)
        // Efecto visual: esperamos un poco para simular carga
        await Task.Delay(500);

        // Asignamos valores a los Labels que creamos en el XAML
        lblTotalUsuarios.Text = "1,245";
        lblTotalCanciones.Text = "12.4k";
        lblTopGenero.Text = "Alt. Pop";
        lblTotalPlaylists.Text = "890";
        lblTotalArtistas.Text = "415";
    }

    private async void Volver_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}