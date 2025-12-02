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
        await camposIncompletos();

		try
            {
                var token = await _authService.LoginAsync(IdentifierEntry.Text, PasswordEntry.Text);

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
}