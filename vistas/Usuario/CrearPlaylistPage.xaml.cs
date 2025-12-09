using pasadena_vistas.Models.Playlist;
using pasadena_vistas.Models;
using pasadena_vistas.Services;

namespace pasadena_vistas.vistas.Usuario;

public partial class CrearPlaylistPage : ContentPage
{
    private FileResult _selectedImageFile;
    private readonly AuthService _authService;
    private readonly PlaylistService _playlistService;

    public CrearPlaylistPage()
    {
        InitializeComponent();
        _authService = new AuthService();
        _playlistService = new PlaylistService();
    }

    private async void AcceptButton_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NombreEntry.Text))
        {
            NombreErrorLabel.IsVisible = true;
            return;
        }

        NombreErrorLabel.IsVisible = false;

        try
        {
            bool publico = IsPublicSwitch.IsToggled;

            var owner = await _authService.ObtenerPerfilUsuarioAsync();

            if (owner == null)
            {
                await DisplayAlert("Error", "No se pudo obtener el usuario actual", "OK");
                return;
            }

            var solicitud = new PlaylistRegistro
            {
                name = NombreEntry.Text.Trim(),
                is_public = publico,
                owner_id = owner.id
            };

            PlaylistRespuesta playlistResgistrado = await _playlistService.RegistrarPlaylistAsync(solicitud);

            if (_selectedImageFile != null)
            {
                await _playlistService.SubirCoverPlaylistAsync(playlistResgistrado.id, _selectedImageFile);
            }

            await DisplayAlert("Éxito", "Playlist creada", "Aceptar");
            await Shell.Current.GoToAsync("..");

        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Error", "No hay conexion a internet", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un problema: {ex.Message}", "Aceptar");
        }

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

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}