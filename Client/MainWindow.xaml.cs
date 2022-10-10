using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IUserService _userService;
        private readonly ITextService _textService;

        public MainWindow()
        {
            var serviceProvider = new ServiceCollection()
                .AddHttpClient("WpfClient", client => client.BaseAddress = new Uri("https://localhost:7053"))
                .Services.BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            _userService = new UserService(httpClientFactory);
            _textService = new TextService(_userService, httpClientFactory);
            InitializeComponent();
        }

        private async void GetText_OnClick(object sender, RoutedEventArgs e)
        {
            var responseMessage = await _textService.GetText(TextName.Text);
            MessageBox.Show(responseMessage);
        }

        private async void UpdateText_OnClick(object sender, RoutedEventArgs e)
        {
            var responseMessage = await _textService.UpdateText(TextName.Text, DecryptedText.Text);
            MessageBox.Show(responseMessage);
        }

        private async void DeleteText_OnClick(object sender, RoutedEventArgs e)
        {
            var responseMessage = await _textService.DeleteText(TextName.Text);
            MessageBox.Show(responseMessage);
        }

        private async void LogIn_OnClick(object sender, RoutedEventArgs e)
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

        private async void CreateConnection_OnClick(object sender, RoutedEventArgs e)
        {
            var response = await _userService.CreateConnection();
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
