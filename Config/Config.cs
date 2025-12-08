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
    public static string ProfilePic(string fotoUrl) => $"{URL_BASE}/profiles/static/avatars/{fotoUrl}";

    //playlist only
    public static string PlaylistsCreate => $"{URL_BASE}/playlist";
    public static string PlaylistUpdate(int playlistId) => $"{URL_BASE}/playlist/{playlistId}";
    public static string PlaylistDelete(int playlistId) => $"{URL_BASE}/playlist/{playlistId}";
    public static string GetCover(string coverName) => $"{URL_BASE}/static/{coverName}";


}
