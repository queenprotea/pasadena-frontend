using Metadata;
using pasadena_vistas.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TagLib;
using File = System.IO.File;



namespace pasadena_vistas.vistas.Administrador;

public partial class AgregarCancionAdminPage : ContentPage
{
   
    private Models.Song newSong = new Models.Song();
    private readonly MetadataService.MetadataServiceClient _client;
    public AgregarCancionAdminPage()
    {
        InitializeComponent();
    }

    

     private async void Guardar_Clicked(object sender, EventArgs e)
     {
         try
         {
             // Validar campos vacíos
             if (string.IsNullOrWhiteSpace(NombreEntry.Text) ||
                 string.IsNullOrWhiteSpace(ArtistaEntry.Text) ||
                 string.IsNullOrWhiteSpace(AlbumEntry.Text) ||
                 string.IsNullOrWhiteSpace(GeneroEntry.Text) ||
                 string.IsNullOrWhiteSpace(DuracionEntry.Text))
             {
                 await DisplayAlert("Error", "Por favor, completa todos los campos.", "OK");
                 return;
             }

             // Validar números
             if (!double.TryParse(DuracionEntry.Text.Replace(":", "."), out double duracion))
             {
                 await DisplayAlert("Error", "La duración debe contener solo números o formato mm:ss.", "OK");
                 return;
             }

             int pista = 0;
             if (!string.IsNullOrWhiteSpace(PistaEntry.Text) && !int.TryParse(PistaEntry.Text, out pista))
             {
                 await DisplayAlert("Error", "El campo 'Pista' debe contener solo números.", "OK");
                 return;
             }

             // Asignar datos desde los Entry
             newSong.title = NombreEntry.Text.Trim();
             newSong.artist = ArtistaEntry.Text.Trim();
             newSong.album = AlbumEntry.Text.Trim();
             newSong.genre = GeneroEntry.Text.Trim();
             newSong.songNumber = pista;
             newSong.duration = duracion; // en segundos

             // Verificar si hay carátula
             if (newSong.album_cover == null)
             {
                 await DisplayAlert("Aviso", "No se ha detectado carátula. Puedes continuar, pero el campo quedará vacío.", "OK");
             }

             // ✅ Crear cliente gRPC y enviar la canción
             using var channel = Grpc.Net.Client.GrpcChannel.ForAddress("http://localhost:50051"); // Cambia al host/puerto de tu backend
             var client = new MetadataService.MetadataServiceClient(channel);

     var request = new AddSongRequest
      {
          FileData = Google.Protobuf.ByteString.CopyFrom(newSong.file_data ?? Array.Empty<byte>()),
          Title = newSong.title,
          Artist = newSong.artist,
          Album = newSong.album,
          Year = "", // Si no tienes campo de año aún
          Genre = newSong.genre,
          Duration = newSong.duration,
          AlbumCover = Google.Protobuf.ByteString.CopyFrom(newSong.album_cover ?? Array.Empty<byte>())
      };

      var response = await client.AddSongAsync(request);

      await DisplayAlert("Éxito", $"Canción '{response.Song.Title}' agregada correctamente con ID {response.Song.SongId}.", "OK");
  }
  catch (Exception ex)
  {
      await DisplayAlert("Error", $"No se pudo guardar la canción: {ex.Message}", "OK");
  }

}



    private async void Cancelar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(".."); // Volver a la página anterior
    }

    private async void AlbumCover_clicked(object sender, EventArgs e)
    {
        try
        {
            var resultado = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Selecciona una imagen (JPG)",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "image/jpeg" } },
                { DevicePlatform.iOS, new[] { "public.jpeg" } },
                { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg" } },
            })
            });

            if (resultado != null)
            {
                using var stream = await resultado.OpenReadAsync();
                using var memoryStream = new MemoryStream();

                await stream.CopyToAsync(memoryStream);
                newSong.album_cover = memoryStream.ToArray();

                AlbumCoverImg.Source = ImageSource.FromStream(() => new MemoryStream(newSong.album_cover));

                await DisplayAlert("Imagen seleccionada", resultado.FileName, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo seleccionar la imagen: {ex.Message}", "OK");
        }
    }
    private async void SeleccionarArchivo_Clicked(object sender, EventArgs e)
    {
        try
        {
            var resultado = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Selecciona un archivo MP3",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "audio/mpeg" } },
                { DevicePlatform.iOS, new[] { "public.audio" } },
                { DevicePlatform.WinUI, new[] { ".mp3" } },
            })
            });

            if (resultado != null)
            {
                ArchivoSeleccionadoLabel.Text = resultado.FileName;
                var rutaArchivo = resultado.FullPath;

                // Guardar todo el archivo MP3 en memoria
                newSong.file_data = await File.ReadAllBytesAsync(rutaArchivo);


                // Leer metadata con TagLib
                var archivo = TagLib.File.Create(rutaArchivo);

                // Extraer datos principales
                var titulo = archivo.Tag.Title;
                var artista = string.Join(", ", archivo.Tag.Performers);
                var album = archivo.Tag.Album;
                var genero = string.Join(", ", archivo.Tag.Genres);
                var duracion = archivo.Properties.Duration; // TimeSpan
                var duracionTexto = duracion.ToString(@"mm\:ss");
                var pista = archivo.Tag.Track;

                // Mostrar valores en los Entry (solo para edición)
                NombreEntry.Text = titulo ?? "";
                ArtistaEntry.Text = artista ?? "";
                AlbumEntry.Text = album ?? "";
                GeneroEntry.Text = genero ?? "";
                DuracionEntry.Text = duracionTexto;
                PistaEntry.Text = pista > 0 ? pista.ToString() : "";

                // Carátula (si existe)
                if (archivo.Tag.Pictures.Length > 0)
                {
                    newSong.album_cover = archivo.Tag.Pictures[0].Data.Data;
                    using var stream = new MemoryStream(newSong.album_cover);
                    AlbumCoverImg.Source = ImageSource.FromStream(() => new MemoryStream(newSong.album_cover));
                }

                await DisplayAlert("Archivo Cargado", "Los metadatos fueron extraídos correctamente. Puedes editar los campos antes de guardar.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo seleccionar el archivo: {ex.Message}", "OK");
        }
    }


}