using pasadena_vistas.Models.Registro;
using pasadena_vistas.Services;
using System.Threading.Tasks;

namespace pasadena_vistas.vistas.Usuario.Registro;

public partial class RegistrarUsuarioPage : ContentPage
{
    private readonly AuthService _authService;
	public RegistrarUsuarioPage()
	{
		InitializeComponent();
        _authService = new AuthService();
        FotoPerfilDefecto();
	}

    private async void btnRegistrarse_Clicked(object sender, EventArgs e)
    {
        if (!await ValidarFormularioAsync())
            return;

        try
        {
            var solicitud = new SolicitudRegistro
            {
                email = CorreoEntry.Text.Trim(),
                full_name = NombreCompletoEntry.Text.Trim(),
                username = UsuarioEntry.Text.Trim(),
                password = ContrasenaEntry.Text,
                role_id = 2
            };

            await _authService.RegistrarUsuarioAsync(solicitud);

            await DisplayAlert("Registro exitoso", "Ahora puedes iniciar sesion", "Aceptar");
            await Shell.Current.GoToAsync("..");
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Error", "No hay conexion a internet", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    private async void btnIniciarSesion_clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(vistas.Usuario.Login.InicioSesion));
    }

    private bool CamposCompletos(string correo, string nombre, string user, string pass, string confirm)
    {
        if (string.IsNullOrWhiteSpace(correo) ||
            string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(user) ||
            string.IsNullOrWhiteSpace(pass) ||
            string.IsNullOrWhiteSpace(confirm))
        {
            DisplayAlert("Campos vacíos", "Por favor completa todos los campos.", "Aceptar");
            return false;
        }

        return true;
    }

    private bool ValidarContrasena(string password)
    {
        if (password.Length < 8)
            return false;

        bool mayus = password.Any(char.IsUpper);
        bool minus = password.Any(char.IsLower);
        bool numero = password.Any(char.IsDigit);

        return mayus && minus && numero;
    }

    private bool ValidarCorreo(string correo)
    {
        try
        {
            return new System.Net.Mail.MailAddress(correo).Address == correo;
        }
        catch
        {
            return false;
        }
    }

    private void UsuarioEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue))
            return;

        var espacios = e.NewTextValue.Replace(" ", string.Empty);

        if (espacios != e.NewTextValue)
            UsuarioEntry.Text = espacios;
    }

    private async Task<bool> ValidarFormularioAsync()
    {
        string correo = CorreoEntry.Text.Trim();
        string nombre = NombreCompletoEntry.Text.Trim();
        string usuario = UsuarioEntry.Text.Trim();
        string pass = ContrasenaEntry.Text;
        string passConfirm = ConfirmarContrasenaEntry.Text;

        if (!CamposCompletos(correo, nombre, usuario, pass, passConfirm))
            return false;

        if (!ValidarCorreo(correo))
        {
            await DisplayAlert("Correo inválido", "Ingresa un correo válido", "Aceptar");
            return false;
        }

        if (usuario.Contains(" "))
        {
            await DisplayAlert("Usuario inválido", "El nombre de usuario no puede contener espacios", "Aceptar");
            return false;
        }

        if (pass != passConfirm)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden", "Aceptar");
            return false;
        }

        if (!ValidarContrasena(pass))
        {
            await DisplayAlert(
                "Contraseña insegura",
                "Debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número",
                "Aceptar"
            );
            return false;
        }

        return true;
    }

    private void FotoPerfilDefecto()
    {
        FotoPerfil.Source = Config.Config.ProfilePic("avatar1.png");
    }
}