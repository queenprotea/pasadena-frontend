using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace pasadena_vistas.vistas.Usuario;

public class UserViewModel : INotifyPropertyChanged
{
    private bool _isFollowing;

    public string Username { get; set; }
    public string ProfilePicture { get; set; }

    public bool IsFollowing
    {
        get => _isFollowing;
        set
        {
            _isFollowing = value;
            OnPropertyChanged(nameof(ButtonText));
            OnPropertyChanged(nameof(ButtonColor));
        }
    }

    public string ButtonText => IsFollowing ? "Siguiendo" : "Seguir";
    public Color ButtonColor => IsFollowing ? Colors.DarkGray : Colors.White;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public partial class SocialPage : ContentPage
{
    public ObservableCollection<UserViewModel> Users { get; set; }

    public SocialPage()
    {
        InitializeComponent();
        LoadUsers(); 
        UsersCollectionView.ItemsSource = Users;
    }

    private void LoadUsers()
    {
        Users = new ObservableCollection<UserViewModel>
        {
            new UserViewModel { Username = "Daniela_22", ProfilePicture = "user_profile_icon.png", IsFollowing = false },
            new UserViewModel { Username = "carlos_music", ProfilePicture = "user_profile_icon.png", IsFollowing = true },
            new UserViewModel { Username = "Sofia_Rock", ProfilePicture = "user_profile_icon.png", IsFollowing = false },
            new UserViewModel { Username = "Alex_DJ", ProfilePicture = "user_profile_icon.png", IsFollowing = false },
            new UserViewModel { Username = "Laura_Indie", ProfilePicture = "user_profile_icon.png", IsFollowing = true }
        };
    }

    private async void FollowButton_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var user = button?.CommandParameter as UserViewModel;

        if (user == null) return;

        user.IsFollowing = !user.IsFollowing;

        if (user.IsFollowing)
        {
            await DisplayAlert("¡Éxito!", $"Has comenzado a seguir a {user.Username}.", "Aceptar");
        }
        else
        {
            await DisplayAlert("Información", $"Has dejado de seguir a {user.Username}.", "Aceptar");
        }
    }
}