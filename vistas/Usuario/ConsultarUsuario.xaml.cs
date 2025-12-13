using System.ComponentModel;
using System.Runtime.CompilerServices;
using pasadena_vistas.Models.Login;
using pasadena_vistas.Services;

namespace pasadena_vistas.vistas.Usuario;

public partial class ConsultarUsuario : ContentPage, INotifyPropertyChanged
{
    private readonly UserService _userService = new();
    private readonly AuthService _authService = new();

    private pasadena_vistas.Models.Login.Usuario _usuarioConsultado;
    public pasadena_vistas.Models.Login.Usuario UsuarioConsultado
    {
        get => _usuarioConsultado;
        set
        {
            _usuarioConsultado = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FotoPerfil));
        }
    }

    private pasadena_vistas.Models.Login.Usuario _usuarioActual;
    public pasadena_vistas.Models.Login.Usuario UsuarioActual
    {
        get => _usuarioActual;
        set
        {
            _usuarioActual = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MostrarSeguir));
            OnPropertyChanged(nameof(MostrarDejarSeguir));
        }
    }

    private bool _isFollowing;
    public bool IsFollowing
    {
        get => _isFollowing;
        set
        {
            _isFollowing = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MostrarSeguir));
            OnPropertyChanged(nameof(MostrarDejarSeguir));
        }
    }

    // ===== VISIBILIDAD =====
    public bool MostrarSeguir =>
        UsuarioActual != null && !IsFollowing;

    public bool MostrarDejarSeguir =>
        UsuarioActual != null && IsFollowing;

    // ===== AVATAR LOCAL =====
    public string FotoPerfil =>
        string.IsNullOrWhiteSpace(UsuarioConsultado?.profile_picture)
            ? "avatar1.png"
            : UsuarioConsultado.profile_picture;

    public ConsultarUsuario(string username)
    {
        InitializeComponent();
        BindingContext = this;

        Loaded += async (_, __) =>
        {
            await CargarUsuarioAsync(username);
        };
    }

    private async Task CargarUsuarioAsync(string username)
    {
        // Usuario consultado
        UsuarioConsultado = await _authService.GetUserByUsernameAsync(username);

        // Usuario logueado (puede ser null)
        UsuarioActual = await _authService.ObtenerPerfilUsuarioAsync();

        // ?? USAMOS is_following
        if (UsuarioActual != null)
        {
            IsFollowing = await _userService.IsFollowingAsync(
                UsuarioActual.id,
                UsuarioConsultado.id
            );
        }
    }

    private async void Seguir_Clicked(object sender, EventArgs e)
    {
        await _userService.SeguirUsuarioAsync(UsuarioConsultado.id);
        IsFollowing = true;
    }

    private async void DejarSeguir_Clicked(object sender, EventArgs e)
    {
        await _userService.DejarDeSeguirUsuarioAsync(UsuarioConsultado.id);
        IsFollowing = false;
    }

    public new event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
