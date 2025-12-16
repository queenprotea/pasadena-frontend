using Grpc.Core;
using Metadata;
using pasadena_vistas.Models;
using Plugin.Maui.Audio;
using Streaming;
using System.Diagnostics;
using System.Linq;

namespace pasadena_vistas.Services
{
    public class PlayerService
    {
        private IAudioPlayer? _player;

        // Cola de canciones por reproducir
        private Queue<pasadena_vistas.Models.Song> _queue = new();

        // Historial para botón "Anterior"
        private Stack<pasadena_vistas.Models.Song> _history = new();

        public event Action<pasadena_vistas.Models.Song>? OnSongChanged;

        public string? CurrentUserId { get; set; }

        private DateTime _playStartTime;
        private pasadena_vistas.Models.Song? _currentSong;
        public bool IsPlaying => _player?.IsPlaying ?? false;
        public event Action<bool>? OnPlayStateChanged; // true = reproduciendo, false = pausado


        // =======================================================
        // MÉTODOS PARA REPRODUCIR
        // =======================================================

        public async Task PlayAlbumAsync(IEnumerable<pasadena_vistas.Models.Song> songs)
        {
            if (songs == null || !songs.Any())
                return;

            _queue = new Queue<pasadena_vistas.Models.Song>(songs);
            _history.Clear();

            await StartQueueAsync();
        }

        public async Task PlaySongAsync(pasadena_vistas.Models.Song song)
        {
            if (song == null)
                return;

            _queue.Clear();
            _history.Clear();

            _queue.Enqueue(song);

            await StartQueueAsync();
        }

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

        // =======================================================
        // PLAY - NEXT - PREVIOUS
        // =======================================================

        public async Task PlayNextAsync()
        {
            StopCurrentPlayer();   // <- DETENER PLAYER ACTUAL

            if (_currentSong != null)
                _history.Push(_currentSong);

            await InternalPlayNextAsync();
        }


        private async Task InternalPlayNextAsync()
        {
            if (_queue.Count == 0)
            {
                _currentSong = null;
                return;
            }

            var song = _queue.Dequeue();
            _currentSong = song;
            if (_currentSong != null)
            {
                OnPlayStateChanged?.Invoke(true); // empieza a reproducirse
            }
            else
            {
                OnPlayStateChanged?.Invoke(false); // cola vacía, poner Play
            }
            OnSongChanged?.Invoke(song);
            
            try
            {
                var stream = await StreamSongAsync(song.Id);

                _player = AudioManager.Current.CreatePlayer(stream);
                _player.PlaybackEnded -= PlayerEnded;
                _player.PlaybackEnded += PlayerEnded;

                _playStartTime = DateTime.UtcNow;

                await RegisterPlayAsync(song.Id, 0);

                _player.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al reproducir canción {song.Id}: {ex}");
                await InternalPlayNextAsync();
            }
        }

        public async Task PlayPreviousAsync()
        {
            if (_history.Count == 0)
                return;

            StopCurrentPlayer();   // <- EVITA DOBLE AUDIO

            if (_currentSong != null)
                _queue = new Queue<pasadena_vistas.Models.Song>(new[] { _currentSong }.Concat(_queue));

            var prev = _history.Pop();

            _queue = new Queue<pasadena_vistas.Models.Song>(new[] { prev }.Concat(_queue));

            await StartQueueAsync();
        }


        public void TogglePlayPause()
        {
            if (_player == null) return;

            if (_player.IsPlaying)
            {
                _player.Pause();
                OnPlayStateChanged?.Invoke(false);
            }
            else
            {
                _player.Play();
                OnPlayStateChanged?.Invoke(true);
            }
        }
        // =======================================================
        // EVENTO CUANDO TERMINA LA CANCIÓN
        // =======================================================
        private async void PlayerEnded(object? sender, EventArgs e)
        {
            if (_currentSong != null)
            {
                var seconds = (DateTime.UtcNow - _playStartTime).TotalSeconds;
                await RegisterPlayAsync(_currentSong.Id, seconds);
            }

            await InternalPlayNextAsync();
        }

        // =======================================================
        // STREAM DESDE BACKEND
        // =======================================================
        private async Task<Stream> StreamSongAsync(string songId)
        {
            var client = StreamingService.Client;

            using var call = client.StreamSong(new StreamRequest { SongId = songId });

            MemoryStream ms = new();
            await foreach (var chunk in call.ResponseStream.ReadAllAsync())
            {
                if (chunk?.Chunk != null)
                    ms.Write(chunk.Chunk.ToByteArray());
            }

            ms.Position = 0;
            return ms;
        }

        // =======================================================
        // REGISTRO DE ESTADÍSTICAS
        // =======================================================
        private async Task RegisterPlayAsync(string songId, double seconds)
        {
            var client = MetadataService.Client;
            if (client == null) return;

            try
            {
                if (string.IsNullOrWhiteSpace(CurrentUserId))
                {
                    var auth = new AuthService();
                    var usuario = await auth.ObtenerPerfilUsuarioAsync();
                    CurrentUserId = usuario.id.ToString();
                }

                var request = new UserPlayRequest
                {
                    UserId = CurrentUserId,
                    SongId = songId,
                    Seconds = seconds
                };

                var response = await client.RegisterUserPlayAsync(request);
            }
            catch (Exception ex)
            {
               
            }
        }
    }
}