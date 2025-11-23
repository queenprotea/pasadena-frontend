namespace pasadena_vistas.vistas.Usuario;
using Microsoft.Maui.Storage;

public partial class AgregarCancionPage : ContentPage
{
    public AgregarCancionPage()
    {
        InitializeComponent();
    }

    private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
    {
        DisplayAlert("Buscando", $"Buscando: {SearchBar.Text} (simulación)", "OK");
    }

    private void AgregarCancion_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Agregada", "Canción agregada a la playlist (simulación)", "OK");
    }


}