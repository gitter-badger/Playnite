﻿using System;
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
using System.Windows.Shapes;
using Microsoft.Win32;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private bool providerIntegrationChanged = false;
        public bool ProviderIntegrationChanged
        {
            get
            {
                return providerIntegrationChanged;
            }
        }

        private bool databaseLocationChanged = false;
        public bool DatabaseLocationChanged
        {
            get
            {
                return databaseLocationChanged;
            }
        }

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (RadioSteamImportLib.IsChecked == true && string.IsNullOrEmpty(TextSteamAccountName.Text))
            {
                MessageBox.Show("Steam account name cannot be empty.", "Wrong settings data", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(TextDatabase.Text))
            {
                MessageBox.Show("Database path cannot be empty.", "Wrong settings data", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var databasePath = TextDatabase.GetBindingExpression(TextBox.TextProperty);
            if (databasePath.IsDirty)
            {
                databaseLocationChanged = true;
                databasePath.UpdateSource();
            }

            var steamEnabled = CheckSteamEnabled.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (steamEnabled.IsDirty)
            {
                providerIntegrationChanged = true;
                steamEnabled.UpdateSource();
            }

            var steamDownloadLib = RadioSteamImportLib.GetBindingExpression(RadioButton.IsCheckedProperty);
            if (steamDownloadLib.IsDirty)
            {
                providerIntegrationChanged = true;
                steamDownloadLib.UpdateSource();
            }

            var steamAccountName = TextSteamAccountName.GetBindingExpression(TextBox.TextProperty);
            if (steamAccountName.IsDirty)
            {
                providerIntegrationChanged = true;
                steamAccountName.UpdateSource();
            }

            var gogEnabled = CheckGogEnabled.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (gogEnabled.IsDirty)
            {
                providerIntegrationChanged = true;
                gogEnabled.UpdateSource();
            }

            var gogRunGalaxy = CheckGogRunGalaxy.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (gogRunGalaxy.IsDirty)
            {
                gogRunGalaxy.UpdateSource();
            }

            var gogIcons = CheckGogIcons.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (gogIcons.IsDirty)
            {
                gogIcons.UpdateSource();
            }

            var gogDownloadLib = RadioGogImportLib.GetBindingExpression(RadioButton.IsCheckedProperty);
            if (gogDownloadLib.IsDirty)
            {
                providerIntegrationChanged = true;
                gogDownloadLib.UpdateSource();
            }

            var originEnabled = CheckOriginEnabled.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (originEnabled.IsDirty)
            {
                providerIntegrationChanged = true;
                originEnabled.UpdateSource();
            }

            var originDownloadLib = RadioOriginImportLib.GetBindingExpression(RadioButton.IsCheckedProperty);
            if (originDownloadLib.IsDirty)
            {
                providerIntegrationChanged = true;
                originDownloadLib.UpdateSource();
            }

            Playnite.Settings.Instance.SaveSettings();
            Close();
        }

        private void ButtonGogAuth_Click(object sender, RoutedEventArgs e)
        {
            var api = new Playnite.Providers.GOG.WebApiClient();
            if (api.Login(this))
            {
                providerIntegrationChanged = true;
            }
        }

        private void ButtonOriginAuth_Click(object sender, RoutedEventArgs e)
        {
            var api = new Playnite.Providers.Origin.WebApiClient();
            if (api.Login(this))
            {
                providerIntegrationChanged = true;
            }
        }

        private void ButtonBrowserDbFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "Database file (*.db)|*.db"
            };

            if (dialog.ShowDialog(this) == true)
            {
                TextDatabase.Text = dialog.FileName;
            }
        }
    }
}
