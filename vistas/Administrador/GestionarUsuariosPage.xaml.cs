using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace pasadena_vistas.vistas.Administrador;

// ViewModel para manejar el estado de cada usuario en la lista de admin
public class UserAdminViewModel : INotifyPropertyChanged
{
    private bool _isBanned;
    public string Username { get; set; }
    public string ProfilePicture { get; set; } = "user_profile_icon.png"; // Imagen por defecto

    public bool IsBanned
    {
        get => _isBanned;
        set
        {
            _isBanned = value;
            OnPropertyChanged(nameof(BanButtonText));
            OnPropertyChanged(nameof(BanButtonColor));
        }
    }

    // Propiedades dinámicas para el botón
    public string BanButtonText => IsBanned ? "Desbanear" : "Banear";
    public Color BanButtonColor => IsBanned ? Colors.Green : Colors.Red;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Lógica principal de la página
public partial class GestionarUsuariosPage : ContentPage
{
    public ObservableCollection<UserAdminViewModel> Users { get; set; }

    public GestionarUsuariosPage()
    {
        InitializeComponent();
        LoadUsers(); // Cargamos usuarios de ejemplo
        UsersCollectionView.ItemsSource = Users;
    }

    private void LoadUsers()
    {
        // En un caso real, estos datos vendrían de tu backend
        Users = new ObservableCollection<UserAdminViewModel>
        {
            new UserAdminViewModel { Username = "Daniela_22", IsBanned = false },
            new UserAdminViewModel { Username = "carlos_music", IsBanned = true },
            new UserAdminViewModel { Username = "Sofia_Rock", IsBanned = false }
        };
    }

    // Lógica para el botón de Banear/Desbanear (CU-06)
    private async void BanButton_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var user = button?.CommandParameter as UserAdminViewModel;

        if (user == null) return;

        // Invertimos el estado de baneo
        user.IsBanned = !user.IsBanned;

        // Mostramos el mensaje correspondiente
        if (user.IsBanned)
        {
            await DisplayAlert("Usuario Baneado", $"El usuario {user.Username} ha sido baneado.", "Aceptar");
        }
        else
        {
            await DisplayAlert("Baneo Removido", $"Se ha removido el baneo al usuario {user.Username}.", "Aceptar");
        }
    }
}