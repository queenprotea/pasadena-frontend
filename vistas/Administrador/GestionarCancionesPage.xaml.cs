using pasadena_vistas.vistas.Administrador;

namespace pasadena_vistas.vistas.Administrador;

public partial class GestionarCancionesPage : ContentPage
{
    public GestionarCancionesPage()
    {
        InitializeComponent();
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
}