namespace pasadena_vistas.Models.Login
{
    public class Usuario
    {
        public int id { get; set; }
        public string email { get; set; }
        public string full_name { get; set; }
        public string username { get; set; }
        public bool is_active { get; set; }
        public int role_id { get; set; }
    }
}
