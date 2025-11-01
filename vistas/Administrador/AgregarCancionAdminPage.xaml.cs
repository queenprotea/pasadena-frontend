namespace pasadena_vistas.vistas.Administrador;

public partial class AgregarCancionAdminPage : ContentPage
{
    public AgregarCancionAdminPage()
    {
        InitializeComponent();
    }

    private async void Guardar_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Éxito", "Canción guardada (simulación)", "Aceptar");
        await Shell.Current.GoToAsync(".."); // Volver a la página anterior
    }

    private async void Cancelar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(".."); // Volver a la página anterior
    }
}