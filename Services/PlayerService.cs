using Grpc.Core;
using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pasadena_vistas.Models;
using Streaming;

namespace pasadena_vistas.Services
{
    public class PlayerService
    {
        private IAudioPlayer? _player;
        private Queue<Song> _queue = new();

        public event Action<Song>? OnSongChanged;

        public async Task PlayAlbumAsync(IEnumerable<Song> songs)
        {
            _queue = new Queue<Song>(songs);
            await StartQueue();
        }

        public async Task PlaySongAsync(Song song)
        {
            _queue.Clear();
            _queue.Enqueue(song);
            await StartQueue();
        }

        private async Task StartQueue()
        {
            // Detener cualquier reproducción anterior
            if (_player != null)
            {
                try
                {
                    _player.PlaybackEnded -= PlayerEnded;
                    _player.Stop();
                    _player.Dispose();
                }
                catch { }
            }

            await PlayNextAsync();
        }

        private async Task PlayNextAsync()
        {
            if (_queue.Count == 0)
                return;

            var song = _queue.Dequeue();
            OnSongChanged?.Invoke(song);

            var stream = await StreamSong(song.Id);

            _player = AudioManager.Current.CreatePlayer(stream);

            // Registrar evento SIN duplicarlo
            _player.PlaybackEnded -= PlayerEnded;
            _player.PlaybackEnded += PlayerEnded;

            _player.Play();
        }

        private async void PlayerEnded(object? sender, EventArgs e)
        {
            await PlayNextAsync();
        }

        private async Task<Stream> StreamSong(string songId)
        {
            var client = Services.StreamingService.Client;
            using var call = client.StreamSong(new StreamRequest { SongId = songId });

            var ms = new MemoryStream();

            await foreach (var chunk in call.ResponseStream.ReadAllAsync())
                ms.Write(chunk.Chunk.ToByteArray());

            ms.Position = 0;
            return ms;
        }
    }


}
