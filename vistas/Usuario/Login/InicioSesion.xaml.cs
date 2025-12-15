using pasadena_vistas.Services;
using pasadena_vistas.Models.Login;
using pasadena_vistas.vistas.Usuario.Registro;
using System.Threading.Tasks;

namespace pasadena_vistas.vistas.Usuario.Login;

public partial class InicioSesion : ContentPage
{
	private readonly AuthService _authService;

    public InicioSesion()
	{
		InitializeComponent();
		_authService = new AuthService();
	}

    private async void btnLoginClick(object sender, EventArgs e)
    {
        await camposIncompletos();

		try
        {
            var token = await _authService.LoginAsync(IdentifierEntry.Text, PasswordEntry.Text);

            await SecureStorage.SetAsync("auth_token", token.access_token);

            var usuario = await _authService.ObtenerPerfilUsuarioAsync();

            if (!string.IsNullOrWhiteSpace(usuario.profile_picture))
                await SecureStorage.SetAsync("profile_picture", usuario.profile_picture);
            else
                SecureStorage.Remove("profile_picture");

            await DisplayAlert("Éxito", "Iniciaste sesión", "Aceptar");

            await Shell.Current.GoToAsync($"///{nameof(PantallaInicio)}");
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Error", "Parece haber un error con el servidor, intente mas tarde", "OK");
        }
        catch (Exception)
        {
            await DisplayAlert("Error al iniciar sesion", "Intenta mas tarde", "Aceptar");
        }
    }

    private async void btnRegistro_Clicked(object sender, EventArgs e)
    {
		await Shell.Current.GoToAsync(nameof(RegistrarUsuarioPage));
    }

	private async Task<bool> camposIncompletos()
	{
		bool resultado = true;

		if (string.IsNullOrWhiteSpace(IdentifierEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
			await DisplayAlert("Error", "Llena todos los campos", "Aceptar");
            resultado = false;
        }

		return resultado;
    }

    private async void btnModificarContrasena(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RecuperarContrasena));
    }
}