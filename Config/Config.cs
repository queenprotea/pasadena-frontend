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

    //User only
    public static string UserRegister => $"{URL_BASE}/profiles/register";
    public static string UserById(int id) => $"{URL_BASE}/profiles/{id}";
}
