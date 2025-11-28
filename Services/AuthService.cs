using pasadena_vistas.Config;
using pasadena_vistas.Models.Login;
using System.Net.Http.Json;
namespace pasadena_vistas.Services;

public class AuthService
{
    private readonly HttpClient _clienteHttp;
    private string urlBase = Config.Config.AuthLogin;

    public AuthService()
    {
        _clienteHttp = new HttpClient();
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
}
