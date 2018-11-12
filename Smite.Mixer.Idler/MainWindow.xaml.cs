using Mixer.Base;
using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Smite.Mixer.Idler.Commands;
using Smite.Mixer.Idler.Helpers;

namespace Smite.Mixer.Idler
{
    public partial class MainWindow
    {
        private MixerConnection _connection;
        private ExpandedChannelModel _channel;
        private PrivatePopulatedUserModel _user;
        private ChatClient _chatClient;
        private readonly List<OAuthClientScopeEnum> _scopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.chat__chat,
            OAuthClientScopeEnum.chat__connect,
        };
        
        public MainWindow()
        {
            InitializeComponent();

            // set the taskbar icon to the grey one because we're not connected yet
            MainIcon.Icon = Properties.Resources.lumbridgeAvatarGrey;

            // attach a left click event to show the main window
            MainIcon.LeftClickCommand = new ShowWindowCommand();
            MainIcon.LeftClickCommandParameter = MainIcon;

            // attach a double click command to show the main window
            MainIcon.DoubleClickCommand = new ShowWindowCommand();
            MainIcon.DoubleClickCommandParameter = MainIcon;

            // get the menu item we want to edit (toggle launch with windows context menu item)
            var menuitem = MainIcon?.ContextMenu?.Items[0] as MenuItem;

            // set the current launch with windows option
            var launchWithWindows = WindowsHelper.GetStartup();
            if(launchWithWindows != null)
                UiHelper.UpdateUiAndTaskbarIcon(this, menuitem, (bool) launchWithWindows);
            
            // attach event handler for when the program is closed
            Closed += MainWindow_Closed;

            // attempt to login to the mixer client + smite chat
            AttemptLogin();
        }

        // minimize to system tray when applicaiton is closed
        protected override void OnClosing(CancelEventArgs e)
        {
            // hide the window
            Hide();
            // cancel the window close operation
            e.Cancel = true;
            // continue the overriden method with the cancel parameter
            base.OnClosing(e);
        }

        public async void AttemptLogin()
        {
            // hide login grid
            LoginGrid.Visibility = Visibility.Visible;
            // show main grid
            MainGrid.Visibility = Visibility.Collapsed;

            // attempt to login and authenticate with OAuth
            _connection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(ConfigurationManager.AppSettings["ClientID"], _scopes);

            // check that we logged in and got a connection
            if (_connection != null)
            {
                // get the current logged in user
                _user = await _connection.Users.GetCurrentUser();
                // get the channel we want to connect to
                _channel = await _connection.Channels.GetChannel("SmiteGame");
                // create a chat client so we can connect to the chat
                _chatClient = await ChatClient.CreateFromChannel(_connection, _channel);

                // attach an event handler incase we disconnect from chat
                _chatClient.OnDisconnectOccurred += ChatClient_OnDisconnectOccurred;

                // try and connect + authenticate with the chat client (will fail if banned from chat etc...)
                if (await _chatClient.Connect() && await _chatClient.Authenticate())
                {
                    // set logged in user on ui
                    ChannelUserTextBlock.Text = _user.username;
                    // set stream title on ui
                    StreamTitleTextBox.Text = _channel.name;
                    // set game title on ui
                    GameTitleTextBlock.Text = _channel.type.name;
                    // set connection status on ui
                    ConnectionStatus.Text = "Connected";
                    // set stream online status on ui
                    StreamStatus.Text = _channel.online.ToString();

                    // set taskbar hover text to show we're connected
                    MainIcon.ToolTipText = "Smite Mixer Idler (Connected)";
                    // set the icon to the coloured version
                    MainIcon.Icon = Properties.Resources.lumbridgeAvatar;
                    // show a notification to the user saying we connected and authenticated successfully
                    MainIcon.ShowBalloonTip("Smite Mixer Idler", "Successfully connected to chat.", Properties.Resources.lumbridgeAvatar, true);

                    // hide the login grid
                    LoginGrid.Visibility = Visibility.Collapsed;
                    // show the main grid
                    MainGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // attempt login on button click
            AttemptLogin();
        }

        private async void MainWindow_Closed(object sender, EventArgs e)
        {
            // when the program is closing then make sure we disconnect from chat gracefully
            if (_chatClient != null)
            {
                // disconnect from chat
                await _chatClient.Disconnect();
            }
        }
        
        private async void ChatClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            // set the UI to disconnected
            await ConnectionStatus.Dispatcher.BeginInvoke((Action)(() => ConnectionStatus.Text = "Disconnection occurred, awaiting reconnection..."));

            // set the taskbar icon to disconnected on hover
            MainIcon.ToolTipText = "Smite Mixer Idler (Disconnected)";
            // set the main icon to the grey one because we're not connected
            MainIcon.Icon = Properties.Resources.lumbridgeAvatarGrey;

            do
            {
                // wait here and try to reconnect
                await Task.Delay(2500);
            }
            // try to connect to chat and authenticate
            while (!await _chatClient.Connect() && !await _chatClient.Authenticate());

            // when we reconnect set the UI back to connected
            await ConnectionStatus.Dispatcher.BeginInvoke((Action)(() => ConnectionStatus.Text = "Connected"));

            // set the taskbar icon to connected on hover
            MainIcon.ToolTipText = "Smite Mixer Idler (Connected)";
            // set the taskbar icon back to the coloured one because we're connected
            MainIcon.Icon = Properties.Resources.lumbridgeAvatar;
        }
    }
}
