using pasadena_vistas.Config;
using pasadena_vistas.Models.Login;
using pasadena_vistas.Models.Registro;
using System.Net;
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

    public async Task<RegistroRespuesta> RegistrarUsuarioAsync(SolicitudRegistro solicitud)
    {
        try
        {
            var respuesta = await _clienteHttp.PostAsJsonAsync(Config.Config.AuthRegister, solicitud);

            if (respuesta.IsSuccessStatusCode)
                return new RegistroRespuesta { Exito = true };

            string error = await respuesta.Content.ReadAsStringAsync();

            if (respuesta.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                if (error.Contains("Email already exists"))
                    return new RegistroRespuesta { Exito = false, MensajeError =
                        "El correo electrónico ya está registrado."};

                if (error.Contains("Username already taken"))
                    return new RegistroRespuesta { Exito = false, MensajeError =
                        "Nombre de usuario ya está en uso."};
            }

            return new RegistroRespuesta { Exito = false, MensajeError = "Error al registrar usuario." };
        }
        catch (Exception ex)
        {
            return new RegistroRespuesta { Exito = false, MensajeError = $"Excepción: {ex.Message}" }; //delete, server error
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

    public async Task<Usuario?> ObtenerPerfilUsuarioAsync()
    {
        var token = await SecureStorage.GetAsync("auth_token");
        if (string.IsNullOrEmpty(token))
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, urlProfile);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _clienteHttp.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<Usuario>();
    }

    public async Task<bool> ValidarTokenAsync()
    {
        var token = await SecureStorage.GetAsync("auth_token");

        if (string.IsNullOrEmpty(token))
            return false;

        _clienteHttp.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var respuesta = await _clienteHttp.GetAsync(Config.Config.GetProfile);

        return respuesta.IsSuccessStatusCode;
    }

    public void LimpiarSesionAsync()
    {
        SecureStorage.Remove("auth_token");
        SecureStorage.Remove("profile_picture");

        _clienteHttp.DefaultRequestHeaders.Authorization = null;
        Preferences.Clear();
    }
}
