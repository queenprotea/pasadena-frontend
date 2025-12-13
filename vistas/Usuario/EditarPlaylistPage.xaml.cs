
using pasadena_vistas.Models;
using pasadena_vistas.Models.Playlist;
using pasadena_vistas.Services;
using System.Threading.Tasks;

namespace pasadena_vistas.vistas.Usuario;

public partial class EditarPlaylistPage : ContentPage
{
    private FileResult _selectedImageFile;
    private readonly AuthService _authService;
    private readonly PlaylistService _playlistService;
    private int playlistId;
    private PlaylistRespuesta playlistActual;


    public EditarPlaylistPage(Album albumSeleccionado, int _playlistId)
    {
        InitializeComponent();

        _authService = new AuthService();
        _playlistService = new PlaylistService();
        playlistId = _playlistId;

        BindingContext = albumSeleccionado;

        ingresarInformacion();

        
    }

    private async Task ingresarInformacion()
    {

        playlistActual = await _playlistService.ObtenerPlaylistPorId(playlistId);
        IsPublicSwitch.IsToggled = playlistActual.is_public;

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
        await Application.Current.MainPage.Navigation
                .PushAsync(new AgregarCancionPage(playlistId));
    }

    private async void Guardar_Clicked(object sender, EventArgs e)
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

            var editado = new PlaylistRegistro
            {
                name = NombreEntry.Text.Trim(),
                is_public = publico,
                owner_id = owner.id
            };

            PlaylistRespuesta playlistActualizado = await _playlistService.ActualizarPlaylistAsync(editado, playlistId);

            if (_selectedImageFile != null)
            {
                await _playlistService.SubirCoverPlaylistAsync(playlistActualizado.id, _selectedImageFile);
            }

            await DisplayAlert("Éxito", "Playlist actualizada", "Aceptar");
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

    private async void BtnVolver_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void Eliminar_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirmar", $"¿Estás seguro de que deseas eliminar la playlist {playlistActual.name}?", "Sí", "No");
        if (!confirm) return;
        try
        {
            var servicio = new PlaylistService();
            await servicio.EliminarPlaylistAsync(playlistId);
            await DisplayAlert("Éxito", "Playlist eliminada.", "OK");
            await Shell.Current.GoToAsync("//PantallaInicio");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo eliminar la playlist: {ex.Message}", "OK");
        }
    }
}