﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using NLog;
using Playnite;
using Playnite.Database;
using Playnite.Models;
using PlayniteUI.Windows;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for GameEditWindow.xaml
    /// </summary>
    public partial class GameEditWindow : Window, INotifyPropertyChanged
    {        
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private WindowPositionHandler positionManager;

        private IGame game;
        public IGame Game
        {
            get
            {
                return game;
            }

            set
            {
                game = value;
                DataChanged(game);
            }
        }

        private IEnumerable<IGame> games;
        public IEnumerable<IGame> Games
        {
            get
            {
                return games;
            }

            set
            {
                games = value;
                DataChanged(games);
            }
        }

        private bool checkBoxesVisible = false;
        public bool CheckBoxesVisible
        {
            get
            {
                return checkBoxesVisible;
            }

            set
            {
                checkBoxesVisible = value;
                OnPropertyChanged("CheckBoxesVisible");
            }
        }

        private GameTask tempPlayTask;
        public GameTask TempPlayTask
        {
            get
            {
                return tempPlayTask;
            }

            set
            {
                tempPlayTask = value;
                OnPropertyChanged("ShowAddPlayAction");
                OnPropertyChanged("ShowRemovePlayAction");
                OnPropertyChanged("TempPlayTask");
                OnPropertyChanged("ShowPlayActionEdit");
            }
        }

        private ObservableCollection<GameTask> tempOtherTasks;
        public ObservableCollection<GameTask> TempOtherTasks
        {
            get
            {
                return tempOtherTasks;
            }

            set
            {
                tempOtherTasks = value;
                OnPropertyChanged("TempOtherTasks");
            }
        }           

        public bool ShowPlayActionEdit
        {
            get
            {

                return TempPlayTask != null;
            }
        }

        public bool ShowAddPlayAction
        {
            get
            {
                if (DataContext == null)
                {
                    return false;
                }

                if (Game.Provider != Provider.Custom)
                {
                    return false;
                }

                return TempPlayTask == null;
            }
        }

        public bool ShowRemovePlayAction
        {
            get
            {
                if (DataContext == null)
                {
                    return false;
                }

                if (Game.Provider != Provider.Custom)
                {
                    return false;
                }

                return TempPlayTask != null && !TempPlayTask.IsBuiltIn;
            }
        }

        #region Dirty flags
        public bool IsNameBindingDirty
        {
            get
            {
                return IsControlBindingDirty(TextName, TextBox.TextProperty);
            }
        }

        public bool IsGenreBindingDirty
        {
            get
            {
                return IsControlBindingDirty(TextGenres, TextBox.TextProperty);
            }
        }

        public bool IsReleaseDateBindingDirty
        {
            get
            {
                return IsControlBindingDirty(TextReleaseDate, TextBox.TextProperty);
            }
        }

        public bool IsDeveloperBindingDirty
        {
            get
            {
                return IsControlBindingDirty(TextDeveloper, TextBox.TextProperty);
            }
        }

        public bool IsPublisherBindingDirty
        {
            get
            {
                return IsControlBindingDirty(TextPublisher, TextBox.TextProperty);
            }
        }

        public bool IsCategoriesBindingDirty
        {
            get
            {
                return IsControlBindingDirty(TextCategories, TextBox.TextProperty);
            }
        }

        public bool IsStoreBindingDirty
        {
            get
            {
                return IsControlBindingDirty(TextStore, TextBox.TextProperty);
            }
        }

        public bool IsWikiBindingDirty
        {
            get
            {
                return IsControlBindingDirty(TextWiki, TextBox.TextProperty);
            }
        }

        public bool IsForumsBindingDirty
        {
            get
            {
                return IsControlBindingDirty(TextForums, TextBox.TextProperty);
            }
        }

        public bool IsDescriptionBindingDirty
        {
            get
            {
                return IsControlBindingDirty(TextDescription, TextBox.TextProperty);
            }
        }

        public bool IsIconBindingDirty
        {
            get
            {
                return IsControlBindingDirty(ImageIcon, Image.SourceProperty);
            }
        }

        public bool IsImageBindingDirty
        {
            get
            {
                return IsControlBindingDirty(ImageImage, Image.SourceProperty);
            }
        }

        #endregion Dirty flags

        private CheckBox checkIcon;
        private CheckBox checkImage;

        public event PropertyChangedEventHandler PropertyChanged;

        public GameEditWindow()
        {
            InitializeComponent();
            positionManager = new WindowPositionHandler(this, "EditGame");
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool IsControlBindingDirty(DependencyObject control, DependencyProperty property)
        {
            if (control == null)
            {
                return false;
            }

            return BindingOperations.GetBindingExpression(control, property).IsDirty;
        }

        private string SelectImage(string filter)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = filter
            };

            if (dialog.ShowDialog(this) == true)
            {
                return dialog.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        private void DataChanged(object data)
        {
            if (data is IEnumerable<IGame>)
            {
                CheckBoxesVisible = true;
                var previewGame = GamesEditor.GetMultiGameEditObject(Games);
                DataContext = previewGame;
                Game = previewGame;
                TabActions.Visibility = Visibility.Hidden;
                ButtonDownload.Visibility = Visibility.Hidden;
            }
            else
            {
                DataContext = Game;

                if (Game.PlayTask != null)
                {
                    TempPlayTask = Playnite.CloneObject.CloneJson<GameTask>(Game.PlayTask);
                }

                if (Game.OtherTasks != null)
                {
                    TempOtherTasks = Playnite.CloneObject.CloneJson<ObservableCollection<GameTask>>(Game.OtherTasks);
                    controlOtherTasks.ItemsSource = TempOtherTasks;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowAddPlayAction"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowRemovePlayAction"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowPlayActionEdit"));
            }
        }

        private BitmapImage CreateBitmapSource(string imagePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.UriSource = new Uri(imagePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        private void PreviewGameData(IGame game)
        {
            var listConverter = new ListToStringConverter();
            var dateConverter = new NullableDateToStringConverter();

            TextName.Text = string.IsNullOrEmpty(game.Name) ? TextName.Text : game.Name;
            TextDeveloper.Text = (string)listConverter.Convert(game.Developers, typeof(string), null, null);
            TextPublisher.Text = (string)listConverter.Convert(game.Publishers, typeof(string), null, null);
            TextGenres.Text = (string)listConverter.Convert(game.Genres, typeof(string), null, null);
            TextReleaseDate.Text = (string)dateConverter.Convert(game.ReleaseDate, typeof(DateTime?), null, null);

            if (!string.IsNullOrEmpty(game.Image))
            {
                var extension = Path.GetExtension(game.Image);
                var tempPath = Path.Combine(Paths.TempPath, "tempimage" + extension);
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                Web.DownloadFile(game.Image, tempPath);
                ImageImage.Source = CreateBitmapSource(tempPath);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ImageImage.Tag.ToString()));
            }
        }

        private string SaveFileIconToTemp(string exePath)
        {
            var ico = IconExtension.ExtractIconFromExe(exePath, true);
            if (ico == null)
            {
                return string.Empty;
            }

            var tempPath = Path.Combine(Paths.TempPath, "tempico.png");

            if (ico != null)
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                ico.ToBitmap().Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
            }

            return tempPath;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (Games == null)
            {
                if (string.IsNullOrWhiteSpace(TextName.Text))
                {
                    MessageBox.Show("Name cannot be empty.", "Invalid game data", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(TextReleaseDate.Text) && !DateTime.TryParseExact(TextReleaseDate.Text, Playnite.Constants.DateUiFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                MessageBox.Show("Release date in is not valid format.", "Invalid game data", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (IsNameBindingDirty && checkName.IsChecked == true)
            {
                BindingOperations.GetBindingExpression(TextName, TextBox.TextProperty).UpdateSource();
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Name = Game.Name;
                    }
                }
            }
            
            if (IsGenreBindingDirty && checkGenres.IsChecked == true)
            {
                BindingOperations.GetBindingExpression(TextGenres, TextBox.TextProperty).UpdateSource();
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Genres = Game.Genres;
                    }
                }
            }
            
            if (IsReleaseDateBindingDirty && checkReleaseDate.IsChecked == true)
            {
                BindingOperations.GetBindingExpression(TextReleaseDate, TextBox.TextProperty).UpdateSource();
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.ReleaseDate = Game.ReleaseDate;
                    }
                }
            }
            
            if (IsDeveloperBindingDirty && checkDeveloper.IsChecked == true)
            {
                BindingOperations.GetBindingExpression(TextDeveloper, TextBox.TextProperty).UpdateSource();
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Developers = Game.Developers;
                    }
                }
            }

            if (IsPublisherBindingDirty && checkPublisher.IsChecked == true)
            {
                BindingOperations.GetBindingExpression(TextPublisher, TextBox.TextProperty).UpdateSource();
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Publishers = Game.Publishers;
                    }
                }
            }
            
            if (IsCategoriesBindingDirty && checkCategories.IsChecked == true)
            {
                BindingOperations.GetBindingExpression(TextCategories, TextBox.TextProperty).UpdateSource();
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Categories = Game.Categories;
                    }
                }
            }

            if (IsStoreBindingDirty && checkStore.IsChecked == true)
            {
                BindingOperations.GetBindingExpression(TextStore, TextBox.TextProperty).UpdateSource();
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.StoreUrl = Game.StoreUrl;
                    }
                }
            }

            if (IsWikiBindingDirty && checkWiki.IsChecked == true)
            {
                BindingOperations.GetBindingExpression(TextWiki, TextBox.TextProperty).UpdateSource();
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.WikiUrl = Game.WikiUrl;
                    }
                }
            }

            if (IsForumsBindingDirty && checkForums.IsChecked == true)
            {
                BindingOperations.GetBindingExpression(TextForums, TextBox.TextProperty).UpdateSource();
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.CommunityHubUrl = Game.CommunityHubUrl;
                    }
                }
            }

            if (IsDescriptionBindingDirty && checkDescription.IsChecked == true)
            {
                BindingOperations.GetBindingExpression(TextDescription, TextBox.TextProperty).UpdateSource();
                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        game.Description = Game.Description;
                    }
                }
            }

            if (IsIconBindingDirty && checkIcon.IsChecked == true)
            {
                var iconPath = HttpUtility.UrlDecode(((BitmapImage)ImageIcon.Source).UriSource.AbsolutePath);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(iconPath);
                var iconId = "images/custom/" + fileName;
                GameDatabase.Instance.AddImage(iconId, fileName, File.ReadAllBytes(iconPath));

                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        if (!string.IsNullOrEmpty(game.Icon))
                        {
                            GameDatabase.Instance.DeleteImageSafe(game.Icon, game);
                        }

                        game.Icon = iconId;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Game.Icon))
                    {
                        GameDatabase.Instance.DeleteImageSafe(Game.Icon, Game);
                    }

                    Game.Icon = iconId;
                }

                if (Path.GetDirectoryName(iconPath) == Paths.TempPath)
                {
                    File.Delete(iconPath);
                }
            }

            if (IsImageBindingDirty && checkImage.IsChecked == true)
            {
                var imagePath = HttpUtility.UrlDecode(((BitmapImage)ImageImage.Source).UriSource.AbsolutePath);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagePath);
                var imageId = "images/custom/" + fileName;
                GameDatabase.Instance.AddImage(imageId, fileName, File.ReadAllBytes(imagePath));

                if (Games != null)
                {
                    foreach (var game in Games)
                    {
                        if (!string.IsNullOrEmpty(game.Image))
                        {
                            GameDatabase.Instance.DeleteImageSafe(game.Image, game);
                        }

                        game.Image = imageId;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Game.Image))
                    {
                        GameDatabase.Instance.DeleteImageSafe(Game.Image, Game);
                    }

                    Game.Image = imageId;
                }

                if (Path.GetDirectoryName(imagePath) == Paths.TempPath)
                {
                    File.Delete(imagePath);
                }
            }

            if (Games == null)
            {
                if (!Game.PlayTask.IsEqualJson(TempPlayTask))
                {
                    Game.PlayTask = TempPlayTask;
                }

                if (!Game.OtherTasks.IsEqualJson(TempOtherTasks))
                {
                    Game.OtherTasks = TempOtherTasks;
                }
            }

            if (Games != null)
            {
                foreach (var game in Games)
                {
                    GameDatabase.Instance.UpdateGame(game);
                }
            }
            else
            {
                GameDatabase.Instance.UpdateGame(Game);
            }

            DialogResult = true;
            Close();
        }

        private void ButtonSelectIcon_Click(object sender, RoutedEventArgs e)
        {
            var path = SelectImage("Image Files (*.bmp, *.jpg, *.png, *.gif, *.ico)|*.bmp;*.jpg*;*.png;*.gif;*.ico|Executable (.exe)|*.exe");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.EndsWith("exe", StringComparison.CurrentCultureIgnoreCase))
                {
                    path = SaveFileIconToTemp(path);

                    if (string.IsNullOrEmpty(path))
                    {
                        return;
                    }
                }               

                ImageIcon.Source = CreateBitmapSource(path);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ImageIcon.Tag.ToString()));
            }
        }

        private void ButtonExecIcon_Click(object sender, RoutedEventArgs e)
        {
            if (TempPlayTask == null || TempPlayTask.Type == GameTaskType.URL)
            {
                return;
            }

            var icon = SaveFileIconToTemp(TempPlayTask.Path);
            if (string.IsNullOrEmpty(icon))
            {
                return;
            }

            ImageIcon.Source = CreateBitmapSource(icon);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ImageIcon.Tag.ToString()));
        }

        private void ButtonDefaulIcon_Click(object sender, RoutedEventArgs e)
        {
            var image = new BitmapImage(new Uri(Game.DefaultIcon, UriKind.Relative));
            ImageIcon.Source = image;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ImageIcon.Tag.ToString()));
        }

        private void ButtonDefaulImage_Click(object sender, RoutedEventArgs e)
        {
            if (Game.Provider == Provider.Custom)
            {
                Game.Icon = Game.DefaultIcon;
            }
        }

        private void ButtonSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var path = SelectImage("Image Files (*.bmp, *.jpg, *.png, *.gif)|*.bmp;*.jpg*;*.png;*.gif");
            if (!string.IsNullOrEmpty(path))
            {
                var bitmap = new BitmapImage(new Uri(path));
                ImageImage.Source = (bitmap);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ImageImage.Tag.ToString()));
            }
        }

        private void ButtonAddPlayAction_Click(object sender, RoutedEventArgs e)
        {
            TempPlayTask = new GameTask()
            {
                Name = "Play",
                IsBuiltIn = false
            };
        }

        private void ButtonRemovePlayAction_Click(object sender, RoutedEventArgs e)
        {
            TempPlayTask = null;
        }

        private void ButtonAddAction_Click(object sender, RoutedEventArgs e)
        {
            if (TempOtherTasks == null)
            {
                TempOtherTasks = new ObservableCollection<GameTask>();
                controlOtherTasks.ItemsSource = TempOtherTasks;
            }

            TempOtherTasks.Add(new GameTask()
            {
                Name = "New Action",
                IsBuiltIn = false
            });
        }

        private void ButtonDeleteAction_Click(object sender, RoutedEventArgs e)
        {
            var task = ((Button)sender).DataContext as GameTask;
            TempOtherTasks.Remove(task);
        }

        private void ButtonDownload_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextName.Text))
            {
                MessageBox.Show("Game name cannot be empty.", "", MessageBoxButton.OK);
                return;
            }

            var window = new MetadataLookupWindow()
            {
                DataContext = Game,
                ShowInTaskbar = false,
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (window.LookupData(TextName.Text) == true)
            {
                PreviewGameData(window.MetadataData);
                CheckBoxesVisible = true;
            }            
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var obj = (FrameworkElement)sender;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(obj.Tag.ToString()));
        }

        private void Image_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            var obj = (FrameworkElement)sender;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(obj.Tag.ToString()));
        }

        private void CheckIcon_Loaded(object sender, RoutedEventArgs e)
        {
            checkIcon = sender as CheckBox;
        }

        private void CheckImage_Loaded(object sender, RoutedEventArgs e)
        {
            checkImage = sender as CheckBox;
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                positionManager.SavePosition(Settings.Instance);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            positionManager.RestoreSizeAndLocation(Settings.Instance);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Console.WriteLine("changed" + e.NewSize.Height);
                positionManager.SaveSize(Settings.Instance);
            }
        }

        private void ButtonPickCat_Click(object sender, RoutedEventArgs e)
        {
            var converter = new ListToStringConverter();
            var dummyGame = new Game()
            {
                Categories = (List<string>)converter.ConvertBack(TextCategories.Text, typeof(List<string>), null, CultureInfo.InvariantCulture)
            };

            var window = new CategoryConfigWindow()
            {
                AutoUpdateGame = false,
                Game = dummyGame,
                Owner = this
            };

            window.ShowDialog();

            if (window.DialogResult == true)
            {
                TextCategories.Text = (string)converter.Convert(dummyGame.Categories, typeof(string), null, CultureInfo.InvariantCulture);
            }
        }
    }
}
