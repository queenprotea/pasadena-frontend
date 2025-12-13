using pasadena_vistas.Models.Playlist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace pasadena_vistas.Services
{
    internal class PlaylistService
    {

        private readonly HttpClient _clienteHttp;

        public PlaylistService()
        {
            _clienteHttp = new HttpClient();
        }

        // Registrar nueva playlist
        public async Task<PlaylistRespuesta> RegistrarPlaylistAsync(PlaylistRegistro solicitud)
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new Exception("Debes iniciar sesión para crear playlist");

            _clienteHttp.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var respuesta = await _clienteHttp.PostAsJsonAsync(Config.Config.PlaylistRegister, solicitud);

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();

                if (respuesta.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    if (error.Contains("Playlist name required"))
                        throw new Exception("Ingresa el nombre de la playlist");
                }

                throw new Exception($"Error al registrar la playlist: {respuesta.StatusCode} - {error}");
            }

            return await respuesta.Content.ReadFromJsonAsync<PlaylistRespuesta>();
        }

        // Subir cover de playlist
        public async Task SubirCoverPlaylistAsync(int playlistId, FileResult fileResult)
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new Exception("Debes iniciar sesión para subir un cover");

            _clienteHttp.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Abrir el stream desde el FileResult
            using var fileStream = await fileResult.OpenReadAsync();

            using var form = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);

            // Detectar MIME según extensión
            var ext = Path.GetExtension(fileResult.FileName).ToLower();
            var mime = ext == ".jpg" || ext == ".jpeg" ? "image/jpeg" : "image/png";
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mime);

            form.Add(streamContent, "file", fileResult.FileName);

            var url = Config.Config.PlaylistPostCover(playlistId);

            var respuesta = await _clienteHttp.PostAsync(url, form);

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();

                if (respuesta.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    if (error.Contains("Not allowed"))
                        throw new Exception("Sin acceso a la playlist");

                    else if (error.Contains("Only image files are allowed"))
                        throw new Exception("Solamente se admite imagen");

                    else
                        throw new Exception("fallo en subir la imagen: " + error.ToString());
                }

                throw new Exception("Error al subir cover.");
            }

            var playlistActualizado = await respuesta.Content.ReadFromJsonAsync<PlaylistRespuesta>();
            Console.WriteLine($"Cover actualizado");
        }

        // Obtener cover de playlist
        public async Task<ImageSource> ObtenerCoverPlaylistAsync(string coverFileName)
        {

            if (string.IsNullOrEmpty(coverFileName)) 
                return ImageSource.FromFile("default_cover.jpg");

            else
                return ImageSource.FromUri(new Uri(Config.Config.PlaylistCover(coverFileName))); // Construir la URL completa usando tu config

        }



        // Obtener playlists por owner_id
        public async Task<List<PlaylistRespuesta>> ObtenerPlaylistsPorOwnerAsync(int ownerId)
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new Exception("Debes iniciar sesión para ver tus playlists");

            _clienteHttp.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = Config.Config.PlaylistsByOwner(ownerId);

            var respuesta = await _clienteHttp.GetAsync(url);

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();

                throw new Exception($"Error al obtener playlists: {respuesta.StatusCode} - {error}");
            }

            var playlists = await respuesta.Content.ReadFromJsonAsync<List<PlaylistRespuesta>>();
            return playlists ?? new List<PlaylistRespuesta>();
        }


        // Obtener canciones de una playlist
        public async Task<List<PlaylistSongs>> ObtenerCancionesDePlaylistAsync(int playlistId)
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new Exception("Debes iniciar sesión para ver las canciones de la playlist");

            _clienteHttp.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = Config.Config.PlaylistSongs(playlistId);

            var respuesta = await _clienteHttp.GetAsync(url);

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                throw new Exception($"Error al obtener canciones de la playlist: {respuesta.StatusCode} - {error}");
            }

            var canciones = await respuesta.Content.ReadFromJsonAsync<List<PlaylistSongs>>();
            return canciones ?? new List<PlaylistSongs>();
        }

        // Obtener playlist por id 
        public async Task<PlaylistRespuesta> ObtenerPlaylistPorId(int playlistId)
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new Exception("Debes iniciar sesión para ver tus playlists");

            _clienteHttp.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = Config.Config.PlaylistById(playlistId);

            var respuesta = await _clienteHttp.GetAsync(url);

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();

                throw new Exception($"Error al obtener la playlist: {respuesta.StatusCode} - {error}");
            }

            var playlist = await respuesta.Content.ReadFromJsonAsync<PlaylistRespuesta>();
            return playlist ?? new PlaylistRespuesta();
        }

        public async Task<PlaylistRespuesta> ActualizarPlaylistAsync(PlaylistRegistro editado, int playlistId)
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new Exception("Debes iniciar sesión para crear playlist");

            _clienteHttp.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var respuesta = await _clienteHttp.PutAsJsonAsync(Config.Config.PlaylistUpdate(playlistId), editado);

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();

                if (respuesta.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    if (error.Contains("Playlist not found"))
                        throw new Exception("Playlist no encontrado");
                }

                throw new Exception($"Error al registrar la playlist: {respuesta.StatusCode} - {error}");
            }

            return await respuesta.Content.ReadFromJsonAsync<PlaylistRespuesta>();
        }

        // Agregar canción a playlist
        public async Task AgregarCancionAPlaylistAsync(int playlistId, string songId, int position)
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new Exception("Debes iniciar sesión para agregar canciones a la playlist");

            _clienteHttp.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = Config.Config.PlaylistAddSong(playlistId);

            var contenido = new StringContent(
                JsonSerializer.Serialize(new { song_id = songId, position = position }),
                Encoding.UTF8,
                "application/json"
            );
            var respuesta = await _clienteHttp.PostAsync(url, contenido);
            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();

                if (respuesta.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    if (error.Contains("Not allowed"))
                        throw new Exception("Sin acceso a la playlist");

                    if (error.Contains("Song not found"))
                        throw new Exception("Cancion no encontrada");
                }

                throw new Exception($"Error al agregar canción a la playlist: {respuesta.StatusCode} - {error}");
            }
        }

        // Remover canción de playlist
        public async Task RemoverCancionDePlaylistAsync(int playlistId, string songId)
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new Exception("Debes iniciar sesión para remover canciones de la playlist");

            _clienteHttp.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = Config.Config.PlaylistRemoveSong(playlistId, songId);

            
            var respuesta = await _clienteHttp.DeleteAsync(url);
            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();

                if (respuesta.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    if (error.Contains("Not allowed"))
                        throw new Exception("Sin acceso a la playlist");

                }

                throw new Exception($"Error al remover la canción a la playlist: {respuesta.StatusCode} - {error}");
            }
        }

        // Eliminar playlist
        public async Task EliminarPlaylistAsync(int playlistId)
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new Exception("Debes iniciar sesión para eliminar la playlist");

            _clienteHttp.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = Config.Config.PlaylistDelete(playlistId);


            var respuesta = await _clienteHttp.DeleteAsync(url);
            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();

                if (respuesta.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    if (error.Contains("Not allowed"))
                        throw new Exception("Sin acceso a la playlist");

                }

                throw new Exception($"Error al eliminar la playlist: {respuesta.StatusCode} - {error}");
            }
        }


        // Obtener playlists activas y publicas por owner_id
        public async Task<List<PlaylistRespuesta>> ObtenerPlaylistsActivasPublicasPorOwnerAsync(int ownerId)
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new Exception("Debes iniciar sesión para ver tus playlists");

            _clienteHttp.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = Config.Config.PlaylistActivePublicByOwner(ownerId);

            var respuesta = await _clienteHttp.GetAsync(url);

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();

                throw new Exception($"Error al obtener playlists: {respuesta.StatusCode} - {error}");
            }

            var playlists = await respuesta.Content.ReadFromJsonAsync<List<PlaylistRespuesta>>();
            return playlists ?? new List<PlaylistRespuesta>();
        }

        // Obtener playlists activas por owner_id
        public async Task<List<PlaylistRespuesta>> ObtenerPlaylistsActivasPorOwnerAsync(int ownerId)
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new Exception("Debes iniciar sesión para ver tus playlists");

            _clienteHttp.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = Config.Config.PlaylistActiveByOwner(ownerId);

            var respuesta = await _clienteHttp.GetAsync(url);

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();

                throw new Exception($"Error al obtener playlists: {respuesta.StatusCode} - {error}");
            }

            var playlists = await respuesta.Content.ReadFromJsonAsync<List<PlaylistRespuesta>>();
            return playlists ?? new List<PlaylistRespuesta>();
        }

        // Obtener playlists activas y publicas
        public async Task<List<PlaylistRespuesta>> ObtenerPlaylistsActivasPublicas()
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new Exception("Debes iniciar sesión para ver tus playlists");

            _clienteHttp.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = Config.Config.PlaylistActivePublic();

            var respuesta = await _clienteHttp.GetAsync(url);

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();

                throw new Exception($"Error al obtener playlists: {respuesta.StatusCode} - {error}");
            }

            var playlists = await respuesta.Content.ReadFromJsonAsync<List<PlaylistRespuesta>>();
            return playlists ?? new List<PlaylistRespuesta>();
        }

    }
}
