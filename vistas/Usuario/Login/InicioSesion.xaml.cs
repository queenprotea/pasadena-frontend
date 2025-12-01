using pasadena_vistas.Services;
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
		string identifier = IdentifierEntry.Text;
		string password = PasswordEntry.Text;

		if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
		{
			await DisplayAlert("Error", "Llena todos los campos", "Aceptar");
			return;
		}

		try
		{
			var token = await _authService.LoginAsync(identifier, password);

			await SecureStorage.SetAsync("auth_token", token.access_token);

			await DisplayAlert("Exito", "Iniciaste sesion", "Aceptar");

			await Shell.Current.GoToAsync($"///{nameof(PantallaInicio)}");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error al iniciar sesion", ex.Message, "Aceptar");
		}
    }

    private async void btnRegistro_Clicked(object sender, EventArgs e)
    {
		await Shell.Current.GoToAsync(nameof(PantallaInicio));
    }
}