using pasadena_vistas.Services;
using pasadena_vistas.Models.Recuperacion;

namespace pasadena_vistas.vistas.Usuario.RecuperarContrasena;

public partial class RecuperarContrasena : ContentPage
{
    private readonly AuthService _authService;
    private string _codigoRecuperacion;
	public RecuperarContrasena()
	{
		InitializeComponent();
        _authService = new AuthService();
	}

    private async void btnSolicitarCodigo(object sender, EventArgs e)
    {
        try
        {
            await _authService.IniciarRecuperacionAsync(
                new SolicitudCambioContrasena
                {
                    email = CorreoEntry.Text.Trim(),
                    username = UsuarioEntry.Text.Trim()
                });

            CodigoLayout.IsVisible = true;

            await DisplayAlert("Exito",
                "Se ha enviado un codigo a tu correo",
                "Aceptar");
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Error", "Parece haber un error con el servidor, intente mas tarde", "OK");
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Ocurrio un error al modificar tu contraseña, intenta mas tarde", "OK");
        }
    }

    private async void btnVerificarCodigo(object sender, EventArgs e)
    {
        try
        {
            await _authService.VerificarCodigoAsync(
                new VerificarCambioContrasena
                {
                    email = CorreoEntry.Text.Trim(),
                    code = CodigoEntry.Text.Trim()
                });

            _codigoRecuperacion = CodigoEntry.Text.Trim();

            ContrasenaLayout.IsVisible = true;

            await DisplayAlert("Exito",
                "Ahora puedes modificar tu contraseña",
                "Aceptar");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Código inválido", "OK");
        }
    }

    private async void btnModificarContrasena(object sender, EventArgs e)
    {
        try
        {
            await _authService.CambiarContrasenaAsync(
                new CambioContrasena
                {
                    email = CorreoEntry.Text.Trim(),
                    code = _codigoRecuperacion,
                    new_password = ContrasenaNuevaEntry.Text.Trim(),
                    confirm_password = ConfirmarContrasenaEntry.Text.Trim()
                });

            await DisplayAlert("Contraseña actualizada",
                "Ahora deberas iniciar sesión con tu nueva contraseña",
                "Aceptar");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error",
                "Contraseña invalida. 8 caracteres, 1 numero y una mayuscula.",
                "OK");
        }
    }
}