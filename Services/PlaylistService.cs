using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using pasadena_vistas.Models.Playlist;

namespace pasadena_vistas.Services
{
    internal class PlaylistService
    {
        private readonly HttpClient _clienteHttp;

        public async Task RegistrarPlaylist(PlaylistRegistro playlist)
        {
            var respuesta = await _clienteHttp.PostAsJsonAsync(Config.Config.PlaylistsCreate, playlist);

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();

                if (respuesta.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    if (error.Contains("Playlist name required"))
                        throw new Exception("Ingresa el nombre de la playlist");
                }

                throw new Exception("Error al registrar el usuario.");
            }
        }
    }
}
