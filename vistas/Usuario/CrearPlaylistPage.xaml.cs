namespace pasadena_vistas.vistas.Usuario;

public partial class CrearPlaylistPage : ContentPage
{
    public CrearPlaylistPage()
    {
        InitializeComponent();
    }

    private async void AcceptButton_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NombreEntry.Text))
        {
            NombreErrorLabel.IsVisible = true;
            return; 
        }

        NombreErrorLabel.IsVisible = false;

        await DisplayAlert("Éxito", "Playlist creada", "Aceptar");
        await Shell.Current.GoToAsync("..");
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}