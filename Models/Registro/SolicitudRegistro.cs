using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pasadena_vistas.Models.Registro;

public class SolicitudRegistro
{
    public string email { get; set; }
    public string full_name { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    public int role_id { get; set; } = 2;
}
