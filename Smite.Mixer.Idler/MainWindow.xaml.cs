using Mixer.Base;
using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using Mixer.Base.Model.OAuth;
using Smite.Mixer.Idler.Commands;

namespace Smite.Mixer.Idler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MixerConnection _connection;
        private ExpandedChannelModel _channel;
        private PrivatePopulatedUserModel _user;
        private ChatClient _chatClient;

        public MainWindow()
        {
            InitializeComponent();

            MainIcon.Icon = Properties.Resources.lumbridgeAvatarGrey;

            MainIcon.LeftClickCommand = new ShowWindowCommand();
            MainIcon.LeftClickCommandParameter = MainIcon;

            MainIcon.DoubleClickCommand = new ShowWindowCommand();
            MainIcon.DoubleClickCommandParameter = MainIcon;

            this.Closed += MainWindow_Closed;

            // atempt to login when the window is created todo: make this earlier, aka when program starts/is in task tray...
            AttemptLogin();
        }

        // minimize to system tray when applicaiton is closed
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            base.OnClosing(e);
        }

        public async void AttemptLogin()
        {
            LoginGrid.Visibility = Visibility.Visible;

            MainGrid.Visibility = Visibility.Collapsed;

            List<OAuthClientScopeEnum> scopes = new List<OAuthClientScopeEnum>()
            {
                OAuthClientScopeEnum.chat__chat,
                OAuthClientScopeEnum.chat__connect,
            };

            _connection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(ConfigurationManager.AppSettings["ClientID"], scopes);

            if (_connection != null)
            {
                _user = await _connection.Users.GetCurrentUser();
                _channel = await _connection.Channels.GetChannel("SmiteGame");

                _chatClient = await ChatClient.CreateFromChannel(_connection, _channel);

                _chatClient.OnDisconnectOccurred += ChatClient_OnDisconnectOccurred;
                _chatClient.OnPollStartOccurred += ChatClient_PollStartOccurred;
                _chatClient.OnPollEndOccurred += ChatClient_PollEndOccurred;

                if (await this._chatClient.Connect() && await _chatClient.Authenticate())
                {
                    ChannelUserTextBlock.Text = _user.username;
                    StreamTitleTextBox.Text = _channel.name;
                    GameTitleTextBlock.Text = _channel.type.name;
                    ConnectionStatus.Text = "Connected";
                    StreamStatus.Text = _channel.online.ToString();

                    MainIcon.ToolTipText = "Smite Mixer Idler (Connected)";
                    MainIcon.Icon = Properties.Resources.lumbridgeAvatar;
                    MainIcon.ShowBalloonTip("Smite Mixer Idler", "Successfully connected to chat.", Properties.Resources.lumbridgeAvatar, true);

                    LoginGrid.Visibility = Visibility.Collapsed;
                    MainGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            AttemptLogin();
        }

        private async void MainWindow_Closed(object sender, EventArgs e)
        {
            if (this._chatClient != null)
            {
                await this._chatClient.Disconnect();
            }
        }

        #region Chat Event Handler
        
        private async void ChatClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            await ConnectionStatus.Dispatcher.BeginInvoke((Action)(() => ConnectionStatus.Text = "Disconnection occurred, awaiting reconnection..."));

            MainIcon.ToolTipText = "Smite Mixer Idler (Disconnected)";
            MainIcon.Icon = Properties.Resources.lumbridgeAvatarGrey;

            do
            {
                await Task.Delay(2500);
            }
            while (!await this._chatClient.Connect() && !await this._chatClient.Authenticate());

            await ConnectionStatus.Dispatcher.BeginInvoke((Action)(() => ConnectionStatus.Text = "Connected"));

            MainIcon.ToolTipText = "Smite Mixer Idler (Connected)";
            MainIcon.Icon = Properties.Resources.lumbridgeAvatar;
        }

        private void ChatClient_PollStartOccurred(object sender, ChatPollEventModel e)
        {

        }

        private void ChatClient_PollEndOccurred(object sender, ChatPollEventModel e)
        {

        }

        #endregion Chat Event Handlers
    }
}
