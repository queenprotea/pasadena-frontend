using pasadena_vistas.Services;

namespace pasadena_vistas.vistas.Usuario;

public partial class EditarPerfilPage : ContentPage
{
    private string? _avatarActual;
    private string? _avatarSeleccionado;
    private readonly UserService _userService = new UserService();

    public EditarPerfilPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _avatarActual = await SecureStorage.GetAsync("profile_picture");

        if (!string.IsNullOrWhiteSpace(_avatarActual))
        {
            FotoPerfil.Source = AvatarHelper.Get(_avatarActual);
        }
        else
        {
            _avatarActual = "avatar1.png";
            FotoPerfil.Source = AvatarHelper.Get(_avatarActual);
        }

        GuardarCambios.IsEnabled = false;
    }


    private void CambiarFoto_Clicked(object sender, EventArgs e)
    {
        AvatarSelector.IsVisible = !AvatarSelector.IsVisible;
    }

    private async void AvatarSeleccionado(object sender, EventArgs e)
    {
        if (sender is not ImageButton btn || btn.CommandParameter is not string avatar)
            return;

        if (string.Equals(avatar, _avatarActual, StringComparison.OrdinalIgnoreCase))
        {
            await DisplayAlert(
                "Sin cambios",
                "Ya estás usando esta foto de perfil.",
                "OK"
            );

            GuardarCambios.IsEnabled = false;
            return;
        }

        _avatarSeleccionado = avatar;
        FotoPerfil.Source = AvatarHelper.Get(avatar);
        GuardarCambios.IsEnabled = true;
    }


    private async void GuardarCambios_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_avatarSeleccionado))
            return;

        if (string.Equals(_avatarSeleccionado, _avatarActual, StringComparison.OrdinalIgnoreCase))
        {
            await DisplayAlert(
                "Sin cambios",
                "La foto seleccionada es la misma que la actual.",
                "OK"
            );
            return;
        }

        try
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new Exception("Sesión no válida");

            int userId = JwtHelper.GetUserId(token);


            await _userService.ActualizarFotoPerfilAsync(userId, _avatarSeleccionado);
            await SecureStorage.SetAsync("profile_picture", _avatarSeleccionado);

            await DisplayAlert("Éxito", "Foto de perfil actualizada", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void Cancelar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
