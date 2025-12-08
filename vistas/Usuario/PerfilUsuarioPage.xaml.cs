using System.Threading.Tasks;

namespace pasadena_vistas.vistas.Usuario;

public partial class PerfilUsuarioPage : ContentPage
{
	public PerfilUsuarioPage()
	{
		InitializeComponent();
	}

    private async void EditarPerfil_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(EditarPerfilPage));
    }
    private async void Social_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SocialPage));
    }
    private async void AdminPanel_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(vistas.Administrador.GestionarUsuariosPage));
    }

    private async void LoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(vistas.Usuario.Login.InicioSesion));
    }
}