﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using NLog;
using Playnite;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public string Version
        {
            get
            {
                return Update.GetCurrentVersion().ToString(2);
            }
        }            

        public AboutWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DiagButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "ZIP Archive (*.zip)|*.zip"
            };

            if (dialog.ShowDialog(this) == true)
            {
                try
                {
                    Diagnostic.CreateDiagPackage(dialog.FileName);
                    MessageBox.Show("Diagnostics package created successfully.");
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Faild to created diagnostics package.");
                    MessageBox.Show("Failed to create diagnostics package.");
                }
            }
        }
    }
}
