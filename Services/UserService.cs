using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace pasadena_vistas.Services;

public class UserService
{
    private readonly HttpClient _clienteHttp;

    public UserService()
    {
        _clienteHttp = new HttpClient();
    }

    public async Task ActualizarFotoPerfilAsync(int userId, string avatar)
    {
        var token = await SecureStorage.GetAsync("auth_token");

        _clienteHttp.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var carga = new
        {
            profile_picture = avatar
        };

        var respuesta= await _clienteHttp.PutAsJsonAsync(
            $"{Config.Config.URL_BASE}/profiles/{userId}/picture",
            carga
        );

        if (!respuesta.IsSuccessStatusCode)
            throw new Exception("Error al actualizar la foto de perfil.");
    }
}
