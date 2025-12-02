using pasadena_vistas.Services;
using System.Threading.Tasks;

namespace pasadena_vistas.vistas.Usuario;

public partial class PerfilUsuarioPage : ContentPage
{
    private readonly AuthService _authService;
	public PerfilUsuarioPage()
	{
		InitializeComponent();
        _authService = new AuthService();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarPerfil();
    }

    private async Task CargarPerfil()
    {
        try
        {
            var usuario = await _authService.ObtenerPerfilUsuarioAsync();

            if (usuario == null)
            {
                DatosUsuario.IsVisible = false;
                return;
            }

            DatosUsuario.IsVisible = true;

            NombreUsuarioLabel.Text = usuario.username;
            CorreoLabel.Text = usuario.email;

            if (!string.IsNullOrEmpty(usuario.profile_picture))
            {
                FotoPerfil.Source =
                    $"{Config.Config.URL_BASE}/profiles/static/avatars/{usuario.profile_picture}";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    private async void EditarPerfil_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(EditarPerfilPage));
    }
    private async void Social_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SocialPage));
    }
    private async void AdminPanel_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(vistas.Administrador.GestionarUsuariosPage));
    }

    private async void LoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(vistas.Usuario.Login.InicioSesion));
    }

    private async void BtnCerrarSesion(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert(
            "Cerrar sesión",
            "¿Seguro que deseas cerrar sesión?\nTendrás que iniciar sesión de nuevo",
            "Sí",
            "No"
        );

        if (!confirmar)
            return;

        await _authService.LimpiarSesionAsync();

        await Shell.Current.GoToAsync($"///{nameof(PantallaInicio)}");
    }
}