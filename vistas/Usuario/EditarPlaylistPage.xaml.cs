namespace pasadena_vistas.vistas.Usuario;

public partial class EditarPlaylistPage : ContentPage
{
    public EditarPlaylistPage()
    {
        InitializeComponent();
    }

    private async void AgregarCanciones_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AgregarCancionPage));
    }

    private void Guardar_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Guardado", "Cambios guardados (simulación)", "OK");
    }

    private void EliminarCancion_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Eliminado", "Canción eliminada (simulación)", "OK");
    }
}
