namespace pasadena_vistas
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(vistas.Usuario.PerfilUsuarioPage), typeof(vistas.Usuario.PerfilUsuarioPage));
            Routing.RegisterRoute(nameof(vistas.Usuario.EditarPerfilPage), typeof(vistas.Usuario.EditarPerfilPage));
            Routing.RegisterRoute(nameof(vistas.Usuario.SocialPage), typeof(vistas.Usuario.SocialPage));
            Routing.RegisterRoute(nameof(vistas.Usuario.CrearPlaylistPage), typeof(vistas.Usuario.CrearPlaylistPage));
            Routing.RegisterRoute(nameof(vistas.Usuario.EditarPlaylistPage), typeof(vistas.Usuario.EditarPlaylistPage));
            Routing.RegisterRoute(nameof(vistas.Usuario.AgregarCancionPage), typeof(vistas.Usuario.AgregarCancionPage));
            Routing.RegisterRoute(nameof(vistas.Usuario.Login.InicioSesion), typeof(vistas.Usuario.Login.InicioSesion));
            Routing.RegisterRoute(nameof(vistas.Administrador.GestionarUsuariosPage), typeof(vistas.Administrador.GestionarUsuariosPage));
            Routing.RegisterRoute(nameof(vistas.Administrador.GestionarCancionesPage), typeof(vistas.Administrador.GestionarCancionesPage));
            Routing.RegisterRoute(nameof(vistas.Administrador.AgregarCancionAdminPage), typeof(vistas.Administrador.AgregarCancionAdminPage));
            Routing.RegisterRoute(nameof(vistas.Administrador.AdminDashboardPage), typeof(vistas.Administrador.AdminDashboardPage));
        }
    }
}
