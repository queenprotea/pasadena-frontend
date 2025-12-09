using Grpc.Core;
using Plugin.Maui.Audio;
using System.Diagnostics;
using pasadena_vistas.Models;
using Streaming;
using Metadata;

namespace pasadena_vistas.Services
{
    public class PlayerService
    {
        private IAudioPlayer? _player;
        private Queue<pasadena_vistas.Models.Song> _queue = new();

        public event Action<pasadena_vistas.Models.Song>? OnSongChanged;

        public string? CurrentUserId { get; set; }

        private DateTime _playStartTime;
        private pasadena_vistas.Models.Song? _currentSong;

        // ======================
        // MÉTODOS PÚBLICOS
        // ======================
        public async Task PlayAlbumAsync(IEnumerable<pasadena_vistas.Models.Song> songs)
        {
            if (songs == null || !songs.Any())
                return;

            _queue = new Queue<pasadena_vistas.Models.Song>(songs);
            await StartQueueAsync();
        }

        public async Task PlaySongAsync(pasadena_vistas.Models.Song song)
        {
            if (song == null)
                return;

            _queue.Clear();
            _queue.Enqueue(song);
            await StartQueueAsync();
        }

        // ======================
        // MÉTODOS PRIVADOS
        // ======================
        private async Task StartQueueAsync()
        {
            StopCurrentPlayer();

            await PlayNextAsync();
        }

        private void StopCurrentPlayer()
        {
            if (_player != null)
            {
                try
                {
                    _player.PlaybackEnded -= PlayerEnded;
                    _player.Stop();
                    _player.Dispose();
                }
                catch { }
                _player = null;
            }
        }

        private async Task PlayNextAsync()
        {
            if (_queue.Count == 0)
            {
                _currentSong = null;
                return;
            }

            var song = _queue.Dequeue();
            _currentSong = song;
            OnSongChanged?.Invoke(song);

            try
            {
                var stream = await StreamSongAsync(song.Id);
                _player = AudioManager.Current.CreatePlayer(stream);
                _player.PlaybackEnded -= PlayerEnded;
                _player.PlaybackEnded += PlayerEnded;

                _playStartTime = DateTime.UtcNow;

                // Registramos inicio con 0 segundos
                await RegisterPlayAsync(song.Id, 0);

                _player.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al reproducir canción {song.Id}: {ex}");
                await PlayNextAsync(); // Continuar con la siguiente
            }
        }

        private async void PlayerEnded(object? sender, EventArgs e)
        {
            if (_currentSong != null)
            {
                var seconds = (DateTime.UtcNow - _playStartTime).TotalSeconds;
                await RegisterPlayAsync(_currentSong.Id, seconds);
            }

            await PlayNextAsync();
        }

        private async Task<Stream> StreamSongAsync(string songId)
        {
            if (string.IsNullOrWhiteSpace(songId))
                throw new ArgumentException("songId no puede ser nulo");

            var client = Services.StreamingService.Client ?? throw new InvalidOperationException("StreamingService.Client no inicializado");

            using var call = client.StreamSong(new StreamRequest { SongId = songId });

            var ms = new MemoryStream();
            await foreach (var chunk in call.ResponseStream.ReadAllAsync())
            {
                if (chunk?.Chunk != null)
                    ms.Write(chunk.Chunk.ToByteArray());
            }

            ms.Position = 0;
            return ms;
        }

        // ======================
        // REGISTRO DE ESTADÍSTICAS
        // ======================
        private async Task RegisterPlayAsync(string songId, double seconds)
        {
            if (string.IsNullOrWhiteSpace(songId))
            {
                Debug.WriteLine("Abortando play: songId es nulo o vacío");
                return;
            }

            var client = Services.MetadataService.Client;
            if (client == null)
            {
                Debug.WriteLine("Abortando play: MetadataService.Client es null");
                return;
            }

            try
            {
                // Si CurrentUserId no está seteado, lo obtenemos
                if (string.IsNullOrWhiteSpace(CurrentUserId))
                {
                    var auth = new AuthService();
                    var usuario = await auth.ObtenerPerfilUsuarioAsync();

                    CurrentUserId = usuario.id.ToString();

                    if (string.IsNullOrWhiteSpace(CurrentUserId))
                    {
                        Debug.WriteLine("Abortando play: No se pudo obtener el CurrentUserId");
                        return;
                    }
                }

                var request = new UserPlayRequest
                {
                    UserId = CurrentUserId,
                    SongId = songId,
                    Seconds = seconds
                };

                var response = await client.RegisterUserPlayAsync(request);
                Debug.WriteLine($"Registro de play: {songId}, segundos={seconds}, success={response.Success}");
            }
            catch (RpcException rpcEx)
            {
                Debug.WriteLine($"Error gRPC al registrar play: {rpcEx.Status.Detail}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al registrar play: {ex}");
            }
        }

    }
}
