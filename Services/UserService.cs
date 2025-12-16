using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace pasadena_vistas.Services;

public class UserService
{
    private readonly HttpClient _clienteHttp;

    public UserService()
    {
        _clienteHttp = HttpClientFactory.Client;
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

    public async Task SeguirUsuarioAsync(int followedId)
    {
        await SetAuthHeaderAsync();

        var response = await _clienteHttp.PostAsync(
            Config.Config.FollowUser(followedId),
            null
        );

        if (!response.IsSuccessStatusCode)
            throw new Exception("Error al seguir al usuario.");
    }

    public async Task DejarDeSeguirUsuarioAsync(int followedId)
    {
        await SetAuthHeaderAsync();

        var response = await _clienteHttp.DeleteAsync(
            Config.Config.UnfollowUser(followedId)
        );

        if (!response.IsSuccessStatusCode)
            throw new Exception("Error al dejar de seguir al usuario.");
    }

    public async Task<List<T>> ObtenerSeguidoresAsync<T>(int userId)
    {
        var response = await _clienteHttp.GetAsync(
            Config.Config.Followers(userId)
        );

        if (!response.IsSuccessStatusCode)
            throw new Exception("Error al obtener seguidores.");

        return await response.Content.ReadFromJsonAsync<List<T>>();
    }

    public async Task<bool> IsFollowingAsync(int followerId, int followedId)
    {
        await SetAuthHeaderAsync();

        var response = await _clienteHttp.GetAsync(
            Config.Config.IsFollowing(followerId, followedId)
        );

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content
            .ReadFromJsonAsync<IsFollowingResponse>();

        return result?.is_following ?? false;
    }



    private async Task SetAuthHeaderAsync()
    {
        var token = await SecureStorage.GetAsync("auth_token");

        if (string.IsNullOrWhiteSpace(token))
            throw new Exception("Token de autenticación no encontrado.");

        _clienteHttp.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public class IsFollowingResponse
    {
        public bool is_following { get; set; }
    }
}
