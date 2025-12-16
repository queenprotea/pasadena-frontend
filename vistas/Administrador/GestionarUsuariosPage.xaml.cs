using pasadena_vistas.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Net.Http.Headers;
using pasadena_vistas.Config;
using System.Linq;

namespace pasadena_vistas.vistas.Administrador;

// ViewModel para manejar el estado de cada usuario en la lista de admin
public class UserAdminViewModel : INotifyPropertyChanged
{
    private bool _isBanned;

    public int UserId { get; set; }                 // ID real del usuario
    public string Username { get; set; }
    public string ProfilePicture { get; set; } = "user_profile_icon.png";

    public bool IsBanned
    {
        get => _isBanned;
        set
        {
            _isBanned = value;
            OnPropertyChanged(nameof(IsBanned));
            OnPropertyChanged(nameof(BanButtonText));
            OnPropertyChanged(nameof(BanButtonColor));
        }
    }

    public string BanButtonText => IsBanned ? "Desbanear" : "Banear";
    public Color BanButtonColor => IsBanned ? Colors.Green : Colors.Red;

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


// Lógica principal de la página
public partial class GestionarUsuariosPage : ContentPage
{
    public ObservableCollection<UserAdminViewModel> Users { get; set; }

    // NUEVO: lista base para búsqueda (no se muestra directamente)
    private readonly ObservableCollection<UserAdminViewModel> _allUsers = new();

    private readonly AuthService _authService = new();

    public GestionarUsuariosPage()
    {
        InitializeComponent();
        Users = new ObservableCollection<UserAdminViewModel>();
        UsersCollectionView.ItemsSource = Users;

        LoadUsers();
    }

    private async void LoadUsers()
    {
        Users.Clear();
        _allUsers.Clear();

        try
        {
            var token = await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Error", "No tienes sesión iniciada.", "OK");
                return;
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Llamamos al endpoint
            var response = await client.GetAsync(Config.Config.AdminUsersList);

            if (!response.IsSuccessStatusCode)
            {
                var errText = await response.Content.ReadAsStringAsync();
                await DisplayAlert(
                    "Error",
                    $"No se pudieron cargar los usuarios ({(int)response.StatusCode}): {errText}",
                    "OK"
                );
                return;
            }

            var jsonText = await response.Content.ReadAsStringAsync();

            var root = JsonNode.Parse(jsonText);

            if (root is JsonArray arr)
            {
                foreach (var node in arr)
                {
                    if (node is null) continue;

                    int id = node["id"]?.GetValue<int>() ?? 0;
                    string username = node["username"]?.GetValue<string>() ?? "";
                    bool isBanned = node["is_banned"]?.GetValue<bool>() ?? false;

                    var vm = new UserAdminViewModel
                    {
                        UserId = id,
                        Username = username,
                        IsBanned = isBanned,
                        ProfilePicture = "ic-admin-default.png"
                    };

                    _allUsers.Add(vm);
                    Users.Add(vm);
                }
            }
            else
            {
                if (root is not null)
                {
                    int id = root["id"]?.GetValue<int>() ?? 0;
                    string username = root["username"]?.GetValue<string>() ?? "";
                    bool isBanned = root["is_banned"]?.GetValue<bool>() ?? false;

                    var vm = new UserAdminViewModel
                    {
                        UserId = id,
                        Username = username,
                        IsBanned = isBanned,
                        ProfilePicture = "ic-admin-default.png"
                    };

                    _allUsers.Add(vm);
                    Users.Add(vm);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al cargar usuarios: {ex.Message}", "OK");
        }
    }

    // NUEVO: filtro en tiempo real por Username
    private void UsersSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        var text = e.NewTextValue?.Trim() ?? "";

        Users.Clear();

        if (string.IsNullOrWhiteSpace(text))
        {
            foreach (var u in _allUsers)
                Users.Add(u);

            return;
        }

        var filtered = _allUsers.Where(u =>
            !string.IsNullOrWhiteSpace(u.Username) &&
            u.Username.Contains(text, StringComparison.OrdinalIgnoreCase));

        foreach (var u in filtered)
            Users.Add(u);
    }

    // Lógica para el botón de Banear/Desbanear 
    private async void BanButton_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var user = button?.CommandParameter as UserAdminViewModel;

        if (user == null)
            return;

        try
        {
            if (user.IsBanned)
            {
                // Está baneado → desbanear en backend
                await _authService.UnbanUserAsync(user.UserId);
                user.IsBanned = false;

                await DisplayAlert(
                    "Baneo removido",
                    $"Se ha removido el baneo al usuario {user.Username}.",
                    "Aceptar"
                );
            }
            else
            {
                // Está activo → banear en backend
                await _authService.BanUserAsync(user.UserId);
                user.IsBanned = true;

                await DisplayAlert(
                    "Usuario baneado",
                    $"El usuario {user.Username} ha sido baneado.",
                    "Aceptar"
                );
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }
}
