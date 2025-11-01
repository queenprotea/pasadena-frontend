namespace pasadena_vistas.vistas.Usuario;

public partial class PantallaInicio : ContentPage
{
	public PantallaInicio()
	{
		InitializeComponent();
	}


	private async void ProfileButton_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(PerfilUsuarioPage));
	}
    private async void CrearPlaylist_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CrearPlaylistPage));
    }

    private async void Playlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedPlaylist = e.CurrentSelection.FirstOrDefault() as string;

        if (selectedPlaylist != null)
        {
            await Shell.Current.GoToAsync(nameof(EditarPlaylistPage));
            (sender as CollectionView).SelectedItem = null;
        }
    }
    private async void AdminUsuarios_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(vistas.Administrador.GestionarUsuariosPage));
    }
    private async void AdminCanciones_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(vistas.Administrador.GestionarCancionesPage));
    }
}