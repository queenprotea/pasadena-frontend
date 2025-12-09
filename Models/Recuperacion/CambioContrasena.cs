using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pasadena_vistas.Models.Recuperacion
{
    public class CambioContrasena
    {
        public string email { get; set; }
        public string code { get; set; }
        public string new_password { get; set; }
        public string confirm_password { get; set; }
    }
}
