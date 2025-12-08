using System.Collections.ObjectModel;

namespace Pasadena.Vistas.Usuario;

public partial class UserView : ContentPage
{
    public UserView()
    {
        InitializeComponent();

        // 1. SIMULAMOS LOS DATOS DEL PERFIL (Mock Data)
        LblNombre.Text = "Andrés Oswaldo";
        LblUsername.Text = "@andres_dev";

        // Para la foto, puedes usar una URL de internet para probar rápido
        ImgPerfil.Source = ImageSource.FromUri(new Uri("https://i.imgur.com/DrkXqGg.png"));


        // 2. SIMULAMOS LAS LISTAS
        ListSiguiendo.ItemsSource = new ObservableCollection<string>
        {
            "ID_9921", "ID_1022", "ID_8832", "ID_4451", "ID_0021"
        };

        ListSeguidores.ItemsSource = new ObservableCollection<string>
        {
            "ID_7721", "ID_3322", "ID_1123", "ID_4444"
        };
    }
}