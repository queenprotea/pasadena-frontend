using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace pasadena_vistas.Config;

public static class Config
{
    public const string URL_BASE = "http://localhost:8080";

    //Auth only
    public static string AuthLogin => $"{URL_BASE}/auth/login";
    public static string AuthRegister => $"{URL_BASE}/auth/register";

    public static string GetProfile => $"{URL_BASE}/auth/me";

    public static string PasswordRecovery => $"{URL_BASE}/auth/password-recovery/initiate";
    public static string PasswordVerify => $"{URL_BASE}/auth/password-recovery/verify";
    public static string PasswordReset => $"{URL_BASE}/auth/password-recovery/reset";

    //User only
    public static string UserRegister => $"{URL_BASE}/profiles/register";
    public static string UserById(int id) => $"{URL_BASE}/profiles/{id}";
   public static string UserByUsername(string username) => $"{URL_BASE}/auth/users/username/{username}";
    public static string MetadataURL => $"{URL_BASE}/metadata.MetadataService";
    public static string StreamingURL => $"{URL_BASE}/streaming.StreamingService";
    public static string ProfilePic(string fotoUrl) => $"{URL_BASE}/profiles/static/avatars/{fotoUrl}";

    //Playlist only

    public static string PlaylistRegister => $"{URL_BASE}/playlist";
    public static string PlaylistPostCover(int playlistId) => $"{URL_BASE}/playlist/{playlistId}/cover";
    public static string PlaylistById(int playlistId) => $"{URL_BASE}/playlist/{playlistId}";
    public static string PlaylistsByOwner(int ownerId) => $"{URL_BASE}/playlist/{ownerId}/owner";

    public static string PlaylistCover(string coverName) => $"{URL_BASE}/static/{coverName}";
}
