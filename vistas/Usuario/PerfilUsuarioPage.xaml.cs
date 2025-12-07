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
            bool tokenValido = await _authService.ValidarTokenAsync();

            if (!tokenValido)
            {
                MostrarVistaNoAutenticado();
                return;
            }

            var usuario = await _authService.ObtenerPerfilUsuarioAsync();

            if (usuario == null)
            {
                MostrarVistaNoAutenticado();
                return;
            }

            NoAuthLayout.IsVisible = false;
            AuthLayout.IsVisible = true;

            NombreUsuarioLabel.Text = usuario.username;
            CorreoLabel.Text = usuario.email;

            var foto = await SecureStorage.GetAsync("profile_picture");
            if (!string.IsNullOrEmpty(foto))
            {
                var fullUrl = $"{Config.Config.URL_BASE}/profiles/static/avatars/{foto}";

                if (Uri.TryCreate(fullUrl, UriKind.Absolute, out var uri))
                    FotoPerfil.Source = ImageSource.FromUri(uri);
            }
        }
        catch (HttpRequestException)
        {
            MostrarVistaNoAutenticado();
            await DisplayAlert("Sin conexion", "No hay conexion a internet", "OK");
        }
        catch (TaskCanceledException)
        {
            MostrarVistaNoAutenticado();
        }
        catch (Exception ex)
        {
            MostrarVistaNoAutenticado();
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private void MostrarVistaNoAutenticado()
    {
        AuthLayout.IsVisible = false;
        NoAuthLayout.IsVisible = true;
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

        _authService.LimpiarSesion();

        await Shell.Current.GoToAsync($"///{nameof(PantallaInicio)}");
        MostrarVistaNoAutenticado();
    }
}