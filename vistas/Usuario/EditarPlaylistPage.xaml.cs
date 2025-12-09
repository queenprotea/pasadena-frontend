using pasadena_vistas.Services;

namespace pasadena_vistas.vistas.Usuario;

public partial class EditarPlaylistPage : ContentPage
{
    private FileResult _selectedImageFile;
    private readonly AuthService _authService;
    private readonly PlaylistService _playlistService;


    public EditarPlaylistPage()
    {
        InitializeComponent();

        _authService = new AuthService();
        _playlistService = new PlaylistService();

    }

    private void mostrarDatosOriginales()
    {
        
    }

    private async void SelectImageButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Abre la galería del dispositivo
            var result = await MediaPicker.Default.PickPhotoAsync();

            if (result != null)
            {
                // Validar extensión
                var ext = Path.GetExtension(result.FileName).ToLower();
                if (ext != ".png")
                {
                    await DisplayAlert("Formato inválido", "Solo se permiten imágenes PNG.", "OK");
                    return;
                }

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