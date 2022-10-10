using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Client.Services;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Guid _appIdentifier;
        private readonly IUserService _userService;
        private readonly ITextService _textService;

        public MainWindow()
        {
            _userService = new UserService("https://localhost:7053");
            _textService = new TextService("https://localhost:7053");
            _appIdentifier = Guid.NewGuid();
            InitializeComponent();
        }

        private async Task GetText_OnClick(object sender, RoutedEventArgs e)
        {
            var responseMessage = await _textService.GetText(TextName.Text);
            MessageBox.Show(responseMessage);
        }

        private async Task UpdateText_OnClick(object sender, RoutedEventArgs e)
        {
            var responseMessage = await _textService.UpdateText(TextName.Text, DecryptedText.Text);
            MessageBox.Show(responseMessage);
        }

        private async Task DeleteText_OnClick(object sender, RoutedEventArgs e)
        {
            var responseMessage = await _textService.DeleteText(TextName.Text);
            MessageBox.Show(responseMessage);
        }

        private async Task LogIn_OnClick(object sender, RoutedEventArgs e)
        {
            var response = await _userService.LogIn(UserName.Text, UserPassword.Text);
            if (_textService is TextService textService)
            {
                if (response.Token != string.Empty)
                {
                    textService.SetAuthorizationToken(response.Token);
                }
            }

            MessageBox.Show(response.Message);
        }

        private async Task CreateConnection_OnClick(object sender, RoutedEventArgs e)
        {
            var response = await _userService.CreateConnection(_appIdentifier);
            if (_textService is TextService textService)
            {
                if (response.SessionKeyResponse is not null)
                {
                    textService.SetUpAes(response.SessionKeyResponse);
                }
            }
            MessageBox.Show(response.Message);
        }
    }
}
