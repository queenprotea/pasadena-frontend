using pasadena_vistas.Config;
using pasadena_vistas.Models.Login;
using System.Net.Http.Json;
using System.Text.Json;
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


}
