using pasadena_vistas.Config;
using pasadena_vistas.Models.Login;
using pasadena_vistas.Models.Recuperacion;
using pasadena_vistas.Models.Registro;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
namespace pasadena_vistas.Services;

public class AuthService
{
    private readonly HttpClient _clienteHttp;
    private string urlBase = Config.Config.AuthLogin;
    private string urlProfile = Config.Config.GetProfile;

    public AuthService()
    {
        _clienteHttp = new HttpClient();
    }

    public async Task RegistrarUsuarioAsync(SolicitudRegistro solicitud)
    {
        var respuesta = await _clienteHttp.PostAsJsonAsync(Config.Config.AuthRegister, solicitud);

        if (!respuesta.IsSuccessStatusCode)
        {
            var error = await respuesta.Content.ReadAsStringAsync();

            if (respuesta.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                if (error.Contains("Email already"))
                    throw new Exception("El correo electrónico ya está registrado.");

                if (error.Contains("Username"))
                    throw new Exception("El nombre de usuario ya está en uso.");
            }

            throw new Exception("Error al registrar el usuario.");
        }
    }

    public async Task<TokenRespuesta> LoginAsync(string identifier, string password)
    {
        var loginInfo = new SolicitudLogin
        {
            identifier = identifier,
            password = password
        };

        var respuesta = await _clienteHttp.PostAsJsonAsync($"{urlBase}", loginInfo);

        if (!respuesta.IsSuccessStatusCode)
            throw new Exception("Credenciales incorrectas");

        return await respuesta.Content.ReadFromJsonAsync<TokenRespuesta>();
    }
    public async Task<Usuario> GetUserByUsernameAsync(string username)
    {
        var url = Config.Config.UserByUsername(username);

        using var client = new HttpClient();

        var response = await client.GetAsync(url);

        var json = await response.Content.ReadAsStringAsync();

        // Si NO es exitoso → el JSON contiene el error
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("API ERROR => " + json);
            return null;
        }

        return JsonSerializer.Deserialize<Usuario>(json);
    }


    public async Task<Usuario?> ObtenerPerfilUsuarioAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                return null;

            using var request = new HttpRequestMessage(HttpMethod.Get, urlProfile);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _clienteHttp.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<Usuario>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> ValidarTokenAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrEmpty(token))
                return false;

            _clienteHttp.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var respuesta = await _clienteHttp.GetAsync(Config.Config.GetProfile);

            return respuesta.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public void LimpiarSesion()
    {
        SecureStorage.Remove("auth_token");
        SecureStorage.Remove("profile_picture");

        _clienteHttp.DefaultRequestHeaders.Authorization = null;
        Preferences.Clear();
    }

    public async Task IniciarRecuperacionAsync(SolicitudCambioContrasena solicitud)
    {
        var respuesta = await _clienteHttp.PostAsJsonAsync(
            $"{Config.Config.PasswordRecovery}", solicitud);

        if (!respuesta.IsSuccessStatusCode)
        {
            var error = await respuesta.Content.ReadAsStringAsync();
            throw new Exception(error);
        }
    }

    public async Task VerificarCodigoAsync(VerificarCambioContrasena solicitud)
    {
        var respuesta = await _clienteHttp.PostAsJsonAsync(
            $"{Config.Config.PasswordVerify}", solicitud);

        if (!respuesta.IsSuccessStatusCode)
        {
            var error = await respuesta.Content.ReadAsStringAsync();
            throw new Exception(error);
        }
    }

    public async Task CambiarContrasenaAsync(CambioContrasena solicitud)
    {
        var respuesta = await _clienteHttp.PostAsJsonAsync(
            $"{Config.Config.PasswordReset}", solicitud);

        if (!respuesta.IsSuccessStatusCode)
        {
            var error = await respuesta.Content.ReadAsStringAsync();
            throw new Exception(error);
        }
    }

}
