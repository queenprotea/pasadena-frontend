namespace pasadena_vistas.vistas.Usuario;

public partial class CrearPlaylistPage : ContentPage
{
    // Variable para guardar temporalmente la imagen seleccionada
    private FileResult _selectedImageFile;

    public CrearPlaylistPage()
    {
        InitializeComponent();
    }

    private async void SelectImageButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Abre la galería del dispositivo
            var result = await MediaPicker.Default.PickPhotoAsync();

            if (result != null)
            {
                _selectedImageFile = result;

                // Muestra la imagen seleccionada en la vista
                var stream = await result.OpenReadAsync();
                PlaylistImage.Source = ImageSource.FromStream(() => stream);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudo cargar la imagen: " + ex.Message, "OK");
        }
    }

    private async void AcceptButton_Clicked(object sender, EventArgs e)
    {
        // 1. Validación del nombre
        if (string.IsNullOrWhiteSpace(NombreEntry.Text))
        {
            NombreErrorLabel.IsVisible = true;
            return;
        }
        NombreErrorLabel.IsVisible = false;

        // 2. Obtener los valores (Aquí iría tu lógica de Backend)
        string nombrePlaylist = NombreEntry.Text;
        bool esPublica = IsPublicSwitch.IsToggled;

        // La imagen está en la variable: _selectedImageFile
        if (_selectedImageFile != null)
        {
            // Lógica para subir la imagen a tu servidor/API
        }

        // 3. Confirmación
        string privacidad = esPublica ? "Pública" : "Privada";
        await DisplayAlert("Éxito", $"Playlist '{nombrePlaylist}' ({privacidad}) creada.", "Aceptar");

        await Shell.Current.GoToAsync("..");
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}