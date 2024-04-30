using CommunityToolkit.Maui.Converters;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
//using System.Text.Json.Serialization;
using DiscordRPC; // used for convenient, short-hand Discord platform integration; non-essential to function
using GTranslate;
using GTranslate.Translators; // used for translation/transliteration using different services; better than calling through a REST API as private keys are often provided in the package/results are scraped through free or inexpensive methods
using Microsoft.Maui.Platform;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
//using Microsoft.Maui.Graphics.Win2D;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Input;

//using Android

namespace RudymentaryMobile
{
    public partial class MainPage : ContentPage
    {
        DiscordRpcClient client = new DiscordRpcClient("DISCORDAPIAPPLICATIONIDHERE"); // actual api/application id omitted

        int count = 0;
        string[] supportedAudioCodecs = { ".mp3", ".flac" };
        string abcDEF = "abcDEF";
        List<Tuple<string, string>> songPathToNameTuples = new List<Tuple<string, string>>();
        List<AlbumData> allSavedAlbumData = new List<AlbumData>();
        List<SongData> songDataQueue = new List<SongData>();
        List<AlbumData> allSavedPlaylistData = new List<AlbumData>();
        List<Tuple<string, SongData>> allSongNameToData = new List<Tuple<string, SongData>>();
        Dictionary<string, int> allPlaylistToIndex = new Dictionary<string, int>();
        Dictionary<string, ImageSource> allImageReferences = new Dictionary<string, ImageSource>();
        ByteArrayToImageSourceConverter byteToImageConverter = new ByteArrayToImageSourceConverter();
        string currentAlbumPlayed = "";
        SongData addToPlaylistToBeAdded;
        SongData currentlyPlayingSongData;
        int songDataQueueIndex;
        bool UniversalMediaPlayer_LoopFlag = false;
        bool UniversalMediaPlayer_ShuffleFlag = false;
        bool UniversalMediaSlider_IsBeingDragged = false;
        Dictionary<string, bool> toggleablePreferences = new Dictionary<string, bool>();
        double relativeSliderViewWidth = DeviceDisplay.MainDisplayInfo.Width;
        ObservableCollection<SongData> albumDataItemsSourceObservable = new ObservableCollection<SongData>();
        List<SongData> albumDataItemsSourceTotal = new List<SongData>();
        int albumDataItemsTakeVal = 15;
        Frame addToPlaylistContentFrame;
        List<string> translatableLanguages = new List<string> { "Original", "Bulgarian", "Czech", "Danish", "German", "Greek", "English", "English (British)", "English (American)", "Spanish", "Estonian", "Finnish", "French", "Hungarian", "Indonesian", "Italian", "Japanese", "Korean", "Lithuanian", "Latvian", "Norwegian", "Dutch", "Polish", "Portuguese", "Portuguese (Brazilian)", "Romanian", "Russian", "Slovak", "Slovenian", "Swedish", "Turkish", "Ukrainian", "Chinese (simplified)" };
        public MainPage()
        {
            InitializeComponent();
            client.Initialize();
            //client.Dispose();
            // client.Initialize();
            CheckForUpdatedPreferences();
            if (File.Exists(Path.Combine(FileSystem.Current.AppDataDirectory, "albumtosongs.txt")))
            {
                List<AlbumDataByteArrayVer> allSavedAlbumDataByteArrayVer = JsonSerializer.Deserialize<List<AlbumDataByteArrayVer>>(File.ReadAllText(Path.Combine(FileSystem.Current.AppDataDirectory, "albumtosongs.txt")));
                //ByteArrayToImageSourceConverter byteToImageConverter = new ByteArrayToImageSourceConverter();
                List<AlbumData> toSaveAllSavedAlbumData = new List<AlbumData>();
                foreach (AlbumDataByteArrayVer album in allSavedAlbumDataByteArrayVer)
                {
                    toSaveAllSavedAlbumData.Add(new AlbumData { AlbumName = album.AlbumName, Songs = album.Songs, AlbumArt = byteToImageConverter.ConvertFrom(album.AlbumArt), AlbumArtists = album.AlbumArtists, IsPlaylist = false, MultiDisc = album.MultiDisc });
                }
                allSavedAlbumData = toSaveAllSavedAlbumData;
            }
            if (File.Exists(Path.Combine(FileSystem.Current.AppDataDirectory, "playlisttosongs.txt")))
            {
                List<AlbumDataByteArrayVer> allSavedPlaylistDataByteArrayVer = JsonSerializer.Deserialize<List<AlbumDataByteArrayVer>>(File.ReadAllText(Path.Combine(FileSystem.Current.AppDataDirectory, "playlisttosongs.txt")));
                List<AlbumData> toSaveAllSavedPlaylistData = new List<AlbumData>();
                int i = 0;
                foreach (AlbumDataByteArrayVer album in allSavedPlaylistDataByteArrayVer)
                {
                    toSaveAllSavedPlaylistData.Add(new AlbumData { AlbumName = album.AlbumName, Songs = album.Songs, AlbumArt = byteToImageConverter.ConvertFrom(album.AlbumArt), AlbumArtists = album.AlbumArtists, IsPlaylist = true, MultiDisc = album.MultiDisc });
                    allPlaylistToIndex.Add(album.AlbumName, i);
                    i++;
                }
                allSavedPlaylistData = toSaveAllSavedPlaylistData;
            }
            SettingsFolderCollection.ItemsSource = SettingsGetFolders();
            //SettingsGetAlbumsToSongs(new object(), new EventArgs());
            SettingsGetImageReferenceTest();
            BindableLayout.SetItemsSource(HomeAlbumsCollection, allSavedAlbumData);
            HomePlaylistsCollection.ItemsSource = allSavedPlaylistData;
            AlbumPageSongCollection.ItemsSource = albumDataItemsSourceObservable;
            Resources["SongDataTemplateWidthValue"] = DeviceDisplay.Current.MainDisplayInfo.Width - 180;
            relativeSliderViewWidth = this.Width;
            
            //UniversalMediaPlayerBar_RelativePosition.SetBinding(Grid.WidthRequestProperty, new Binding(relativeSliderViewWidth);
            //UniversalMediaPlayStopButtonCurrentlyPlayingVer.SetBinding(Microsoft.Maui.Controls.Button.TextProperty, new Binding(nameof(UniversalMediaPlayStopButton.Text)));
            /*ICommand load5 = new Command(async () =>
            {
                if (albumDataItemsSourceObservable.Count != albumDataItemsSourceTotal.Count)
                {
                    for (int i = albumDataItemsSourceObservable.Count; i < Math.Min(albumDataItemsSourceObservable.Count + 5, albumDataItemsSourceTotal.Count); i++)
                    {
                        albumDataItemsSourceObservable.Add(albumDataItemsSourceTotal[i]);
                    }
                    OnPropertyChanged(nameof(albumDataItemsSourceObservable));
                }
            }); */
            //AlbumPageSongCollection.RemainingItemsThresholdReachedCommand = load5;
            //LinearGradientBrush homeBackground = new LinearGradientBrush { GradientStops = { }}
            //UniversalMediaElementPlayer_NewSongSelected(new SongData { AlbumName="GT7"})
        }

        internal class FolderClass
        {
            public string FolderPath { get; set; }
        }

        internal class SongData
        {
            public string SongName { get; set; }
            public string ArtistName { get; set; }
            public string AlbumName { get; set; }
            public TimeSpan Duration { get; set; }
            public string DurationString { get; set; }
            public string SongPath { get; set; }
            public int TrackNumber { get; set; }
            public int RenderNumber { get; set; }
            public string ImageKey { get; set; }
            public Dictionary<string, string> Lyrics { get; set; }
            public TimeSpan[] LyricTimings { get; set; }
            public int DiscNo { get; set; }

        }
        internal class AlbumData
        {
            public string AlbumName { get; set; }
            public string AlbumArtists { get; set; }
            public ImageSource AlbumArt { get; set; }
            public List<SongData> Songs { get; set; }
            public bool MultiDisc { get; set; }
            public bool IsPlaylist { get; set; }
        }
        internal class AlbumDataByteArrayVer
        {
            public string AlbumName { get; set; }
            public string AlbumArtists { get; set; }
            public byte[] AlbumArt { get; set; }
            public List<SongData> Songs { get; set; }
            public bool MultiDisc { get; set; }
            public bool IsPlaylist { get; set; }
        }
        internal class AddToPlaylistCustomClass
        {
            public string PlaylistName { get; set; }
            public int PlaylistIndex { get; set; }
        }
        private void OnCounterClicked(object sender, EventArgs e)
        {

        }

        public void HomePageButton_Clicked(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            PageHome.IsVisible = true;
            PageSearch.IsVisible = false;
            PageAlbum.IsVisible = false;
            PageCreatePlaylist.IsVisible = false;
            PageSongLyrics.IsVisible = false;
            PageAddToPlaylist.IsVisible = false;
            PageSettings.IsVisible = false;
            UniversalMediaShowLyricsButton.Opacity = 0.5;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        public void SearchPageButton_Clicked(object sender, EventArgs e)
        {
            PageHome.IsVisible = false;
            PageSearch.IsVisible = true;
            PageAlbum.IsVisible = false;
            PageCreatePlaylist.IsVisible = false;
            PageSongLyrics.IsVisible = false;
            PageAddToPlaylist.IsVisible = false;
            PageSettings.IsVisible = false;
        }
        private async void AlbumPageButton_Clicked(object sender, EventArgs e)
        {
            //await DisplayAlert("Clicked", "You clicked an album", "I know");
            var button = (Border)sender;
            var item = (AlbumData)button.BindingContext;
            //Define AlbumPageAlbumData View and put binding context, can access it later through x.parent.parent.bindingcontext
            
            //AlbumPageSongCollection.ItemsSource = item.Songs;
            // Collection view in code

            if (item.IsPlaylist == true)
            {
                //BindableLayout.SetItemTemplate(AlbumPageSongCollection, (DataTemplate)this.Resources["PlaylistSongDataTemplate"]);
                AlbumPageSongCollection.ItemTemplate = (DataTemplate)this.Resources["PlaylistSongDataTemplate"];
            }
            else if (item.MultiDisc == true)
            {
                //BindableLayout.SetItemTemplate(AlbumPageSongCollection, (DataTemplate)this.Resources["SongDataTemplateMultiDisc"]);
                AlbumPageSongCollection.ItemTemplate = (DataTemplate)this.Resources["SongDataTemplateMultiDisc"];
            }
            else
            {
                //BindableLayout.SetItemTemplate(AlbumPageSongCollection, (DataTemplate)this.Resources["SongDataTemplate"]);
                AlbumPageSongCollection.ItemTemplate = (DataTemplate)this.Resources["SongDataTemplate"];
            }
            //
            AlbumPageAlbumDataView.BindingContext = item;
            albumDataItemsTakeVal = 15;
            //AlbumPageSongCollection.ItemsSource = item.Songs.Take(albumDataItemsTakeVal);
            //albumDataItemsSourceObservable.Add(item.Songs.Take(albumDataItemsTakeVal));
            if (item.Songs != albumDataItemsSourceTotal) { albumDataItemsSourceObservable.Clear(); }
            if (albumDataItemsSourceObservable.Count == 0) { foreach (SongData s in item.Songs.Take(albumDataItemsTakeVal)) { albumDataItemsSourceObservable.Add(s); } }
            albumDataItemsSourceTotal = item.Songs;
            PageAlbum.BindingContext = item;
            PageHome.IsVisible = false;
            PageSearch.IsVisible = false;
            PageAlbum.IsVisible = true;
            //await LazyLoadAlbumSongs.LoadViewAsync();
            PageCreatePlaylist.IsVisible = false;
            PageSettings.IsVisible = false;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            AlbumPageOpenedAnimationCompilation();
            //await FillAsynchronously();
            //FillAsynchronously();
        }

        private async void FillAsynchronously()
        {
            await Task.Delay(9000);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                for (int i = albumDataItemsTakeVal; i < albumDataItemsSourceTotal.Count; i++)
                {
                    albumDataItemsSourceObservable.Add(albumDataItemsSourceTotal[i]);
                }
            });
        }

        private async void AlbumPageSongItem_Tapped(object sender, EventArgs e)
        {
            var button = (Frame)sender;
            var item = (SongData)button.BindingContext;

            songDataQueue = albumDataItemsSourceTotal; //(List<SongData>)AlbumPageSongCollection.ItemsSource; //(List<SongData>)BindableLayout.GetItemsSource(AlbumPageSongCollection);
            songDataQueueIndex = songDataQueue.IndexOf(item);
            var overallAlbum = (AlbumData)PageAlbum.BindingContext;
            //var currentArtPngPath = Path.Combine(FileSystem.Current.AppDataDirectory, "currentart.png");
            if (overallAlbum.AlbumArt != null)
            {
                //await File.WriteAllBytesAsync(currentArtPngPath, byteToImageConverter.ConvertBackTo(overallAlbum.AlbumArt));
            }
            else
            {
                //await File.WriteAllBytesAsync(currentArtPngPath, new byte[0]);
            }

            UniversalMediaPlayer_AlbumArtImage.Source = allImageReferences[item.ImageKey];
            //UniversalMediaPlayer_AlbumArtImageCurrentlyPlaying.Source = allImageReferences[item.ImageKey];
            UniversalMediaElementPlayer_NewSongSelected(item);
            currentAlbumPlayed = overallAlbum.AlbumName;
            //await DisplayAlert("Lyrics", item.Lyrics, "Alright");
            //await DisplayAlert("Current queue", JsonSerializer.Serialize(songDataQueue), "Alright");
        }
        private void CreatePlaylistPageButton_Clicked(object sender, EventArgs e)
        {
            PageHome.IsVisible = false;
            PageSearch.IsVisible = false;
            PageAlbum.IsVisible = false;
            PageCreatePlaylist.IsVisible = true;
            PageSettings.IsVisible = false;
        }
        public void SettingsPageButton_Clicked(object sender, EventArgs args)
        {
            PageHome.IsVisible = false;
            PageSearch.IsVisible = false;
            PageAlbum.IsVisible = false;
            PageCreatePlaylist.IsVisible = false;
            PageSettings.IsVisible = true;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        private void CreatePlaylistSaveButton_Clicked(object sender, EventArgs args)
        {
            string playlistName = CreatePlaylist_PlaylistName.Text;
            bool useDefaultPlaceholderPlaylistImage = CreatePlaylist_ImageCoverPlaceholderCheckBox.IsChecked;
            List<AlbumData> playlistData = allSavedPlaylistData;
            playlistData.Add(new AlbumData { AlbumArt = null, Songs = new List<SongData>(), AlbumName = playlistName, AlbumArtists = "User-made", IsPlaylist = true, MultiDisc = false });
            allSavedPlaylistData = playlistData;
            //HomePlaylistsCollection.ItemsSource = allSavedPlaylistData;
            CreatePlaylist_ImageCoverPlaceholderCheckBox.IsChecked = false;

            AddToPlaylist_SaveToFile();

            PageHome.IsVisible = true;
            PageAlbum.IsVisible = false;
            PageCreatePlaylist.IsVisible = false;
            PageSettings.IsVisible = false;
            //await DisplayAlert("Bruh", JsonSerializer.Serialize(allSavedPlaylistData), "Alright");
            HomePlaylistsCollection.ItemsSource = GetAllSavedPlaylistData().ToObservableCollection();


        }
        private async void SettingsPageAddFolderButton_Clicked(object sender, EventArgs args)
        {
            try
            {

                FolderPickerResult folderSelected = await FolderPicker.PickAsync(default);

                if (folderSelected.IsSuccessful == false) { return; }
                string foldersTxtPath = Path.Combine(FileSystem.Current.AppDataDirectory, "folders.txt");
                List<string> newFolders = new List<string>();
                if (File.Exists(foldersTxtPath))
                {
                    newFolders = File.ReadAllLines(foldersTxtPath).ToList();
                }
                newFolders.Add(folderSelected.Folder.Path);
                File.WriteAllLines(foldersTxtPath, newFolders.Distinct().ToArray());
                await DisplayAlert("Folders", string.Join(",", newFolders), "Thanks again");
                SettingsFolderCollection.ItemsSource = SettingsGetFolders();
                SettingsGetAlbumsToSongs(new object(), new EventArgs());

                await DisplayAlert("Success", "Folder successfully added", "Thanks");
            }
            catch (Exception ex)
            {

                await DisplayAlert("Error encountered!", ex.Message, "Alright");
            }
        }
        private async void SettingsPageAddThemeButton_Clicked(object sender, EventArgs args)
        {
            try
            {
                FilePickerFileType jsonType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>{
                        {DevicePlatform.iOS, new[] { "public.json" } },
                        {DevicePlatform.Android, new[] { "application/json" } },
                        {DevicePlatform.WinUI, new[] { ".json" } },
                        {DevicePlatform.Tizen, new[] {"*/*"} },
                        {DevicePlatform.macOS, new[] {"json"} }

                    }
                    );
                PickOptions jsonPickOptions = new()
                {
                    FileTypes = jsonType
                };
                FileResult jsonSelected = await FilePicker.PickAsync(jsonPickOptions);
                if (jsonSelected == null)
                {
                    return;
                }
                string jsonSelectedText = File.ReadAllText(jsonSelected.FullPath);
                File.WriteAllText(Path.Combine(FileSystem.Current.AppDataDirectory, "theme.json"), jsonSelectedText);
                Dictionary<string, string> keyValueJson = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonSelectedText);
                await DisplayAlert("keyValueJSON", JsonSerializer.Serialize(keyValueJson), "Alright");
                foreach (KeyValuePair<string, string> t in keyValueJson)
                {
                    Application.Current.Resources[t.Key] = Color.FromArgb(t.Value);
                }
                //Application.Current.Resources["SliderTrackValueFillPointerOver"] = Color.FromArgb("#00FF00");

            }
            catch (Exception e)
            {
                await DisplayAlert("Alert", e.Message, "Alright");
            }
        }
        private async void SettingsPageRemoveFolderButton_Clicked(object sender, EventArgs args)
        {

            var button = (Microsoft.Maui.Controls.Button)sender;
            var item = (FolderClass)button.BindingContext;
            string ignorePath = item.FolderPath;
            string foldersTxtPath = Path.Combine(FileSystem.Current.AppDataDirectory, "folders.txt");

            List<string> newFolders = new List<string>();
            foreach (string p in File.ReadAllLines(foldersTxtPath))
            {
                if (p == ignorePath) { continue; }
                newFolders.Add(p);
            }
            File.WriteAllLines(foldersTxtPath, newFolders.Distinct().ToArray());
            SettingsFolderCollection.ItemsSource = SettingsGetFolders();
            SettingsGetAlbumsToSongs(new object(), new EventArgs());
            SettingsUpdatePlaylistsToSongs();

            await DisplayAlert("Success", ignorePath + " was removed.", "Alright");

        }
        private List<FolderClass> SettingsGetFolders()
        {
            List<FolderClass> returnFolders = new List<FolderClass>();
            string foldersTxtPath = Path.Combine(FileSystem.Current.AppDataDirectory, "folders.txt");
            if (File.Exists(foldersTxtPath))
            {
                foreach (string p in File.ReadAllLines(foldersTxtPath))
                {
                    returnFolders.Add(new FolderClass { FolderPath = p });
                }
            }
            UniversalMediaElementPlayer.Stop();
            UniversalMediaElementPlayer.SeekTo(TimeSpan.Zero);
            UniversalMediaSlider.Value = 0;
            UniversalMediaElementPlayer.Source = "";
            //SettingsGetAlbumsToSongs(new object(), new EventArgs());
            return returnFolders;
        }
        private async void SettingsGetAlbumsToSongs_DirectoryHelper(string givenPath)
        {
            try
            {

                /* if (!Android.OS.Environment.IsExternalStorageManager)
                {
                    Intent intent = new Intent(
                    Android.Provider.Settings.ActionManageAppAllFilesAccessPermission,
                    Android.Net.Uri.Parse("package:" + Application.Context.PackageName));

                    intent.AddFlags(ActivityFlags.NewTask);

                    Application.Context.StartActivity(intent);
                } */
                PermissionStatus canread = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                PermissionStatus canwrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                if (canread == PermissionStatus.Denied)
                {
                    canread = await Permissions.RequestAsync<Permissions.StorageRead>();
                }

                //await DisplayAlert("Can it Write", canwrite.ToString(), "It can write?");
                //await DisplayAlert("Can It Read", canread.ToString(), "It can read?");
                string[] filesInDirectory = Directory.EnumerateFiles(givenPath).ToArray();

                // await DisplayAlert("Type", (filesInDirectory == null).ToString(), "Woah okay");
                //await DisplayAlert("files in directory", filesInDirectory[0], "Brooo");
                foreach (string f in filesInDirectory)
                {
                    foreach (string ending in supportedAudioCodecs)
                    {
                        if (f.EndsWith(ending))
                        {
                            //await DisplayAlert("song found", f, "Okay");
                            songPathToNameTuples.Add(Tuple.Create(f, f.Substring(givenPath.Length + 1)));
                        }
                    }

                }
                string[] directoriesInDirectory = Directory.GetDirectories(givenPath);
                foreach (string d in directoriesInDirectory)
                {
                    SettingsGetAlbumsToSongs_DirectoryHelper(d);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Exception encounter", ex.Message, "Ohh I See");
            }


        }
        private async void SettingsGetAlbumsToSongs(object sender, EventArgs args)
        {
            allSavedAlbumData.Clear();
            //await DisplayAlert("App project directory", Path.Combine(System.AppContext.BaseDirectory, "..", "AndroidProject"), "Thanks");
            //SettingsIndexingFoldersActivityIndicator.IsRunning = true;
            Dictionary<string, List<Tuple<string, string>>> albumsToSongs = new Dictionary<string, List<Tuple<string, string>>>();
            string foldersTxtPath = Path.Combine(FileSystem.Current.AppDataDirectory, "folders.txt");
            if (!File.Exists(foldersTxtPath))
            {
                await DisplayAlert("Not existing", foldersTxtPath, "Alright");
                return;
            }
            foreach (string p in File.ReadAllLines(foldersTxtPath))
            {
                // await DisplayAlert("folder read", p, "Next");
                SettingsGetAlbumsToSongs_DirectoryHelper(p);
            }
            songPathToNameTuples = songPathToNameTuples.Distinct().ToList();
            foreach (Tuple<string, string> t in songPathToNameTuples)
            {
                var tagFile = TagLib.File.Create(t.Item1);
                string tagFileAlbum = tagFile.Tag.Album;

                if (tagFileAlbum == null)
                {
                    tagFileAlbum = "Download";
                }
                if (!(albumsToSongs.ContainsKey(tagFileAlbum)))
                {
                    albumsToSongs.Add(tagFileAlbum, new List<Tuple<string, string>>());
                }
                albumsToSongs[tagFileAlbum].Add(t);
            }
            //string serializedJson = JsonSerializer.Serialize(albumsToSongs);
            string albumtosongsTxtPath = Path.Combine(FileSystem.Current.AppDataDirectory, "albumtosongs.txt");
            List<AlbumData> allAlbums = new List<AlbumData>();
            //MediaElement invisibleMediaElement = new MediaElement();
            foreach (string k in albumsToSongs.Keys)
            {
                List<SongData> onlySongsList = new List<SongData>();
                HashSet<int> discNumbers = new HashSet<int>();
                Dictionary<string, int> artistAppearances = new Dictionary<string, int>();
                bool albumArtFound = false;
                ImageSource tagFileAlbumArt = null;
                foreach (Tuple<string, string> t in albumsToSongs[k])
                {
                    var tagFile = TagLib.File.Create(t.Item1);
                    if (!albumArtFound && tagFile.Tag.Album != null)
                    {
                        if (tagFile.Tag.Pictures.Length >= 1)
                        {
                            var bitMapped = (byte[])(tagFile.Tag.Pictures[0].Data.Data);
                            albumArtFound = true;
                            //ByteArrayToImageSourceConverter byteToImageConverter = new ByteArrayToImageSourceConverter();
                            tagFileAlbumArt = byteToImageConverter.ConvertFrom(bitMapped);
                        }
                    }

                    string tagFileSongName = tagFile.Tag.Title;
                    int tagFileTrackNumber = 0;
                    tagFileTrackNumber = (int)tagFile.Tag.Track;

                    if (tagFileSongName == null) { tagFileSongName = t.Item2; }
                    string tagFileArtistName = tagFile.Tag.FirstAlbumArtist;
                    if (tagFileArtistName == null) { tagFileArtistName = tagFile.Tag.FirstArtist; }
                    if (tagFileArtistName == null) { tagFileArtistName = "Unknown"; }
                    string[] tagFileArtists = tagFileArtistName.Split(";"); //tagFile.Tag.AlbumArtists;
                    tagFileArtistName = string.Join(", ", tagFileArtists);
                    foreach (string artist in tagFileArtists)
                    {
                        if (!(artistAppearances.ContainsKey(artist)))
                        {
                            artistAppearances.Add(artist, 0);
                        }
                        artistAppearances[artist] += 1;
                    }

                    //string tagFileLength = tagFile.Properties.;
                    //int tagFileLengthSeconds = 0;
                    //string[] tagFileLengthSplit = tagFileLength.Split(":");
                    //tagFileLengthSeconds += Int32.Parse(tagFileLengthSplit[0]) * 3600;
                    //tagFileLengthSeconds += Int32.Parse(tagFileLengthSplit[1]) * 60;
                    //tagFileLengthSeconds += Int32.Parse(tagFileLengthSplit[2]);
                    //invisibleMediaElement.Source = t.Item1;
                    TimeSpan tagFileDuration = tagFile.Properties.Duration;
                    int tagFileDiscNumber = (int)tagFile.Tag.Disc;
                    if (tagFileDiscNumber == null)
                    {
                        tagFileDiscNumber = 1;
                    }
                    discNumbers.Add(tagFileDiscNumber);
                    string tPathName = t.Item1;
                    string tagFileLyrics = tagFile.Tag.Lyrics;
                    if (tagFileLyrics == null) { tagFileLyrics = ""; }
                    TimeSpan[] lyricTimings = null;
                    //TimeSpan[] lyricTimings = new TimeSpan[0];
                    if (Path.Exists(tPathName.Replace(".mp3", ".lrc")))
                    {

                        List<string> foundLyrics = new List<string>();
                        List<TimeSpan> temporaryTimings = new List<TimeSpan>();
                        foreach (string l in File.ReadLines(tPathName.Replace(".mp3", ".lrc")))
                        {
                            if (l.Length < 2) { continue; }
                            if (char.IsDigit(l[1]))
                            {
                                foundLyrics.Add(l.Substring(10));
                                string timespanToBeParsed = l.Substring(1, 8); // 00:00.00
                                double timespanInSeconds = (int.Parse(timespanToBeParsed.Substring(0, 2)) * 60) + (int.Parse(timespanToBeParsed.Substring(3, 2))) + (int.Parse(timespanToBeParsed.Substring(6, 2)) / 100);
                                temporaryTimings.Add(TimeSpan.FromSeconds(timespanInSeconds));
                            }
                        }
                        //await DisplayAlert("Found .lrc", string.Join("\n", foundLyrics), "No way!");
                        tagFileLyrics = string.Join("\r\n", foundLyrics);
                        lyricTimings = temporaryTimings.ToArray();
                    }
                    SongData toAppendSongData = new SongData { SongName = tagFileSongName, ArtistName = tagFileArtistName, AlbumName = k, Duration = tagFileDuration, DurationString = string.Format("{0}:{1}", formatTimeSpanData(tagFileDuration.Minutes.ToString()), formatTimeSpanData(tagFileDuration.Seconds.ToString())), SongPath = tPathName, TrackNumber = tagFileTrackNumber, RenderNumber = -1, ImageKey = k, Lyrics = new Dictionary<string, string> { { "Original", tagFileLyrics } }, LyricTimings = lyricTimings, DiscNo = tagFileDiscNumber };
                    allSongNameToData.Add(Tuple.Create(tagFileSongName, toAppendSongData));
                    onlySongsList.Add(toAppendSongData);
                }

                var newArr = artistAppearances.ToArray().ToList();
                newArr.Sort((x, y) => x.Value.CompareTo(y.Value));
                newArr.Reverse();
                List<string> tagFileAlbumArtistsList = new List<string>();
                int albumArtistIterator = 0;
                foreach (KeyValuePair<string, int> kVP in newArr)
                {
                    if (albumArtistIterator == 3)
                    {
                        break;
                    }
                    tagFileAlbumArtistsList.Add(kVP.Key);
                    albumArtistIterator++;
                }
                if (newArr.Count > 3)
                {
                    tagFileAlbumArtistsList.Add("Other Artists");
                }
                string tagFileAlbumArtists = String.Join(", ", tagFileAlbumArtistsList);
                onlySongsList.OrderBy(x => x.DiscNo).ThenBy(x => x.TrackNumber);
                int renderNumberI = 1;
                foreach (SongData s in onlySongsList)
                {
                    s.RenderNumber = renderNumberI;
                    renderNumberI++;
                }
                allImageReferences[k] = ImageSource.FromFile("download_svgrepo_comwhite.png");
                if (tagFileAlbumArt != null)
                {
                    allImageReferences[k] = tagFileAlbumArt;
                }
                bool albumIsMultiDisc = discNumbers.Count > 1;
                if (k == "Download") { albumIsMultiDisc = false; }
                allAlbums.Add(new AlbumData { AlbumName = k, Songs = onlySongsList, AlbumArt = tagFileAlbumArt, AlbumArtists = tagFileAlbumArtists, IsPlaylist = false, MultiDisc = albumIsMultiDisc });
            }
            songPathToNameTuples.Clear();
            allAlbums.Sort((x, y) => x.AlbumName.CompareTo(y.AlbumName));
            List<AlbumDataByteArrayVer> allAlbumsByteArrayVer = new List<AlbumDataByteArrayVer>();
            //ByteArrayToImageSourceConverter byteArrayToImageConverter = new ByteArrayToImageSourceConverter();

            foreach (AlbumData album in allAlbums)
            {

                allAlbumsByteArrayVer.Add(new AlbumDataByteArrayVer { AlbumName = album.AlbumName, Songs = album.Songs, AlbumArt = (byte[])byteToImageConverter.ConvertBackTo(album.AlbumArt), AlbumArtists = album.AlbumArtists, IsPlaylist = false, MultiDisc = album.MultiDisc });
            }
            string serializedJson = JsonSerializer.Serialize(allAlbumsByteArrayVer);
            //File.WriteAllText(Path.Combine(FileSystem.Current.AppDataDirectory, "test1.txt"), JsonSerializer.Serialize(allImageReferences));
            if (Application.Current.Resources.ContainsKey("imageReferences"))
            {
                Application.Current.Resources.Remove("imageReferences");
            }
            Application.Current.Resources.Add("imageReferences", allImageReferences);
            File.WriteAllText(albumtosongsTxtPath, serializedJson);
            allSavedAlbumData = allAlbums;
            BindableLayout.SetItemsSource(HomeAlbumsCollection, allAlbums);
            GC.Collect();
            //await DisplayAlert("Epic Dictionary Gameplay", serializedJson, "OK Great");
            //await DisplayAlert("Album Data", allSavedAlbumData.ToString(), "Thanks, finally");
        }
        private async void SettingsGetImageReferenceTest()
        {
            //allSavedAlbumData.Clear();
            //await DisplayAlert("Does it exist", Directory.Exists("/storage/emulated/0/TestMusicFolder").ToString(), "Thanks");
            Dictionary<string, List<Tuple<string, string>>> albumsToSongs = new Dictionary<string, List<Tuple<string, string>>>();
            string foldersTxtPath = Path.Combine(FileSystem.Current.AppDataDirectory, "folders.txt");
            if (!File.Exists(foldersTxtPath))
            {
                //await DisplayAlert("Not existing", foldersTxtPath, "Alright");
                return;
            }
            foreach (string p in File.ReadAllLines(foldersTxtPath))
            {
                SettingsGetAlbumsToSongs_DirectoryHelper(p);
            }
            songPathToNameTuples = songPathToNameTuples.Distinct().ToList();
            foreach (Tuple<string, string> t in songPathToNameTuples)
            {
                var tagFile = TagLib.File.Create(t.Item1);
                string tagFileAlbum = tagFile.Tag.Album;

                if (tagFileAlbum == null)
                {
                    tagFileAlbum = "Download";
                }
                if (!(albumsToSongs.ContainsKey(tagFileAlbum)))
                {
                    albumsToSongs.Add(tagFileAlbum, new List<Tuple<string, string>>());
                }
                albumsToSongs[tagFileAlbum].Add(t);
            }
            //string serializedJson = JsonSerializer.Serialize(albumsToSongs);
            string albumtosongsTxtPath = Path.Combine(FileSystem.Current.AppDataDirectory, "albumtosongs.txt");
            List<AlbumData> allAlbums = new List<AlbumData>();
            //MediaElement invisibleMediaElement = new MediaElement();
            foreach (string k in albumsToSongs.Keys)
            {
                List<SongData> onlySongsList = new List<SongData>();
                HashSet<int> discNumbers = new HashSet<int>();
                Dictionary<string, int> artistAppearances = new Dictionary<string, int>();
                bool albumArtFound = false;
                ImageSource tagFileAlbumArt = null;
                foreach (Tuple<string, string> t in albumsToSongs[k])
                {
                    var tagFile = TagLib.File.Create(t.Item1);
                    if (!albumArtFound && tagFile.Tag.Album != null)
                    {
                        if (tagFile.Tag.Pictures.Length >= 1)
                        {
                            var bitMapped = (byte[])(tagFile.Tag.Pictures[0].Data.Data);
                            albumArtFound = true;
                            //ByteArrayToImageSourceConverter byteToImageConverter = new ByteArrayToImageSourceConverter();
                            tagFileAlbumArt = byteToImageConverter.ConvertFrom(bitMapped);
                        }
                    }

                    string tagFileSongName = tagFile.Tag.Title;
                    int tagFileTrackNumber = 0;
                    tagFileTrackNumber = (int)tagFile.Tag.Track;

                    if (tagFileSongName == null) { tagFileSongName = t.Item2; }
                    string tagFileArtistName = tagFile.Tag.FirstAlbumArtist;
                    if (tagFileArtistName == null) { tagFileArtistName = tagFile.Tag.FirstArtist; }
                    if (tagFileArtistName == null) { tagFileArtistName = "Unknown"; }
                    string[] tagFileArtists = tagFileArtistName.Split(";"); //tagFile.Tag.AlbumArtists;
                    tagFileArtistName = string.Join(", ", tagFileArtists);
                    foreach (string artist in tagFileArtists)
                    {
                        if (!(artistAppearances.ContainsKey(artist)))
                        {
                            artistAppearances.Add(artist, 0);
                        }
                        artistAppearances[artist] += 1;
                    }

                    //string tagFileLength = tagFile.Properties.;
                    //int tagFileLengthSeconds = 0;
                    //string[] tagFileLengthSplit = tagFileLength.Split(":");
                    //tagFileLengthSeconds += Int32.Parse(tagFileLengthSplit[0]) * 3600;
                    //tagFileLengthSeconds += Int32.Parse(tagFileLengthSplit[1]) * 60;
                    //tagFileLengthSeconds += Int32.Parse(tagFileLengthSplit[2]);
                    //invisibleMediaElement.Source = t.Item1;
                    TimeSpan tagFileDuration = tagFile.Properties.Duration;
                    int tagFileDiscNumber = (int)tagFile.Tag.Disc;
                    if (tagFileDiscNumber == null)
                    {
                        tagFileDiscNumber = 1;
                    }
                    discNumbers.Add(tagFileDiscNumber);
                    string tPathName = t.Item1;
                    string tagFileLyrics = tagFile.Tag.Lyrics;
                    if (tagFileLyrics == null) { tagFileLyrics = ""; }
                    TimeSpan[] lyricTimings = null;
                    SongData toAppendSongData = new SongData { SongName = tagFileSongName, ArtistName = tagFileArtistName, AlbumName = k, Duration = tagFileDuration, DurationString = string.Format("{0}:{1}", formatTimeSpanData(tagFileDuration.Minutes.ToString()), formatTimeSpanData(tagFileDuration.Seconds.ToString())), SongPath = tPathName, TrackNumber = tagFileTrackNumber, RenderNumber = -1, ImageKey = k, Lyrics = new Dictionary<string, string> { { "Original", tagFileLyrics } }, LyricTimings = lyricTimings, DiscNo = tagFileDiscNumber };
                    allSongNameToData.Add(Tuple.Create(tagFileSongName, toAppendSongData));
                    onlySongsList.Add(toAppendSongData);
                }

                var newArr = artistAppearances.ToArray().ToList();
                newArr.Sort((x, y) => x.Value.CompareTo(y.Value));
                newArr.Reverse();
                List<string> tagFileAlbumArtistsList = new List<string>();
                int albumArtistIterator = 0;
                foreach (KeyValuePair<string, int> kVP in newArr)
                {
                    if (albumArtistIterator == 3)
                    {
                        break;
                    }
                    tagFileAlbumArtistsList.Add(kVP.Key);
                    albumArtistIterator++;
                }
                if (newArr.Count > 3)
                {
                    tagFileAlbumArtistsList.Add("Other Artists");
                }
                string tagFileAlbumArtists = String.Join(", ", tagFileAlbumArtistsList);
                onlySongsList.OrderBy(x => x.DiscNo).ThenBy(x => x.TrackNumber);
                int renderNumberI = 1;
                foreach (SongData s in onlySongsList)
                {
                    s.RenderNumber = renderNumberI;
                    renderNumberI++;
                }
                allImageReferences[k] = ImageSource.FromFile("download_svgrepo_comwhite.png");
                if (tagFileAlbumArt != null)
                {
                    allImageReferences[k] = tagFileAlbumArt;
                }
                allAlbums.Add(new AlbumData { AlbumName = k, Songs = onlySongsList, AlbumArt = tagFileAlbumArt, AlbumArtists = tagFileAlbumArtists, IsPlaylist = false, MultiDisc = discNumbers.Count > 1 });
            }
            songPathToNameTuples.Clear();
            allAlbums.Sort((x, y) => x.AlbumName.CompareTo(y.AlbumName));
            List<AlbumDataByteArrayVer> allAlbumsByteArrayVer = new List<AlbumDataByteArrayVer>();
            //ByteArrayToImageSourceConverter byteArrayToImageConverter = new ByteArrayToImageSourceConverter();

            foreach (AlbumData album in allAlbums)
            {

                allAlbumsByteArrayVer.Add(new AlbumDataByteArrayVer { AlbumName = album.AlbumName, Songs = album.Songs, AlbumArt = (byte[])byteToImageConverter.ConvertBackTo(album.AlbumArt), AlbumArtists = album.AlbumArtists, IsPlaylist = false, MultiDisc = album.MultiDisc });
            }
            string serializedJson = JsonSerializer.Serialize(allAlbumsByteArrayVer);
            //File.WriteAllText(Path.Combine(FileSystem.Current.AppDataDirectory, "test1.txt"), JsonSerializer.Serialize(allImageReferences));
            if (Application.Current.Resources.ContainsKey("imageReferences"))
            {
                Application.Current.Resources.Remove("imageReferences");
            }
            Application.Current.Resources.Add("imageReferences", allImageReferences);
            //File.WriteAllText(albumtosongsTxtPath, serializedJson);
            //allSavedAlbumData = allAlbums;
            //BindableLayout.SetItemsSource(HomeAlbumsCollection, allAlbums);
            //SettingsIndexingFoldersActivityIndicator.IsRunning = false;
            GC.Collect();
        }
        private void SettingsUpdatePlaylistsToSongs()
        {
            string[] folders = File.ReadAllLines(Path.Combine(FileSystem.Current.AppDataDirectory, "folders.txt"));
            foreach (AlbumData p in allSavedPlaylistData)
            {
                List<SongData> updatedSongs = new List<SongData>();
                int songRenderNoIterator = 0;
                foreach (SongData s in p.Songs)
                {
                    SongData newS = s;
                    bool isStillInFolders = false;
                    foreach (string f in folders)
                    {
                        if (s.SongPath.StartsWith(f))
                        {
                            isStillInFolders = true;
                        }
                    }
                    if (isStillInFolders == true)
                    {
                        songRenderNoIterator++;
                        newS.RenderNumber = songRenderNoIterator;
                        updatedSongs.Add(newS);
                    }
                }
                p.Songs = updatedSongs;
            }
            List<AlbumDataByteArrayVer> newAllSavedPlaylistData = new List<AlbumDataByteArrayVer>();
            foreach (AlbumData p in allSavedPlaylistData)
            {
                newAllSavedPlaylistData.Add(new AlbumDataByteArrayVer { AlbumArt = byteToImageConverter.ConvertBackTo(p.AlbumArt), AlbumArtists = p.AlbumArtists, AlbumName = p.AlbumName, IsPlaylist = p.IsPlaylist, Songs = p.Songs, MultiDisc = p.MultiDisc });
            }
            string playlistToSongTxtPath = Path.Combine(FileSystem.Current.AppDataDirectory, "playlisttosongs.txt");
            File.WriteAllText(playlistToSongTxtPath, JsonSerializer.Serialize(newAllSavedPlaylistData));
            HomePlaylistsCollection.ItemsSource = allSavedPlaylistData.ToObservableCollection();

        }
        private void UniversalMediaPlayStopButton_Clicked(object sender, EventArgs e)
        {

            if (UniversalMediaElementPlayer.CurrentState == CommunityToolkit.Maui.Core.Primitives.MediaElementState.Playing)
            {
                UniversalMediaPlayStopButton.Text = "|>";
                
                UniversalMediaElementPlayer.Pause();
            }
            else
            {
                UniversalMediaPlayStopButton.Text = "||";
                //UniversalMediaPlayStopButtonCurrentlyPlayingVer.Text = "||";
                UniversalMediaElementPlayer.Play();
            }

        }
        private async void UniversalMediaElementPlayer_NewSongSelected(SongData givenSongData)
        {
            try
            {
                string givenPath = givenSongData.SongPath;
                string albumArtists = "Unknown";
                if (givenSongData.ArtistName != null)
                {
                    albumArtists = givenSongData.ArtistName;
                }
                client.ClearPresence();

                UniversalMediaPlayer_AlbumArtImage.Source = allImageReferences[givenSongData.ImageKey];
                //UniversalMediaPlayer_AlbumArtImageCurrentlyPlaying.FadeTo(0, 250);
                if (UniversalMediaPlayer_AlbumArtImageCurrentlyPlaying.Source != allImageReferences[givenSongData.ImageKey])
                {
                    //Animation fadeInFromNothing = new Animation(v => CurrentlyPlayingPage_ContentViewGrid.Opacity = v, 0, 1, Easing.CubicInOut);
                    //fadeInFromNothing.Commit(this, "currentlyPlayingAlbumArtAnimationChanged", 1000);
                }
                UniversalMediaPlayer_AlbumArtImageCurrentlyPlaying.Source = allImageReferences[givenSongData.ImageKey];
                // animate in the switch so it's less abrupt/apparent since it's displayed much larger
                //Animation slideInFromRight = new Animation(v => UniversalMediaPlayer_AlbumArtImageCurrentlyPlaying.TranslationX = v, 75, 0, Easing.CubicInOut);
                //Animation fadeInFromSwitch = new Animation(v => UniversalMediaPlayer_AlbumArtImageCurrentlyPlaying.Opacity = v, 0, 1, Easing.CubicInOut);
                //slideInFromRight.Commit(this, "currentlyPlayingImageSlideFromRight");
                //fadeInFromSwitch.Commit(this, "currentlyPlayingImageFadeFromSwitch");
                //
                UniversalMediaPlayer_CurrentlyPlayingName.Text = givenSongData.SongName;
                UniversalMediaPlayer_CurrentlyPlayingArtists.Text = givenSongData.ArtistName;
                ToolTipProperties.SetText(UniversalMediaPlayer_CurrentlyPlayingName, givenSongData.SongName);

                UniversalMediaElementPlayer.Stop();
                UniversalMediaElementPlayer.Source = givenPath;
                UniversalMediaElementPlayer.Play();
                if (UniversalMediaPlayerBar.IsVisible == false)
                {
                    // animated fade-in of player bar when first song is selected
                    UniversalMediaPlayerBar.IsVisible = true;
                    UniversalMediaPlayerBar.Opacity = 1; // temporary


                    Animation barSizeGrowth = new Animation(v => UniversalMediaPlayerBar.HeightRequest = v, 0, 100); ;
                    Animation barSizeGrowthRowDefinition = new Animation(v => UniversalMediaPlayerBarRowDefinition.Height = v, 0, 100);
                    barSizeGrowth.Commit(this, "PlayerBarGrowth", 16, 1000, Easing.CubicOut);
                    barSizeGrowthRowDefinition.Commit(this, "PlayerBarGrowthRowDefinition", 16, 1000, Easing.CubicOut);
                    /*UniversalMediaPlayer_AlbumArtImageCurrentlyPlaying.WidthRequest = UniversalMediaPlayer_AlbumArtImageCurrentlyPlaying.Width;
                    UniversalMediaPlayer_AlbumArtImageCurrentlyPlaying.HeightRequest = UniversalMediaPlayer_AlbumArtImageCurrentlyPlaying.Height;
                    UniversalMediaPlayer_AlbumArtImageCurrentlyPlaying.HorizontalOptions = LayoutOptions.Start;
                    UniversalMediaPlayer_AlbumArtImageCurrentlyPlayingBorder.VerticalOptions = LayoutOptions.Start;
                    
                    UniversalMediaPlayer_AlbumArtImageCurrentlyPlayingBorder.WidthRequest = UniversalMediaPlayer_AlbumArtImageCurrentlyPlayingBorder.Width;
                    UniversalMediaPlayer_AlbumArtImageCurrentlyPlayingBorder.HeightRequest = UniversalMediaPlayer_AlbumArtImageCurrentlyPlayingBorder.Height;
                    UniversalMediaPlayer_AlbumArtImageCurrentlyPlayingBorder.HorizontalOptions = LayoutOptions.Start;
                    UniversalMediaPlayer_AlbumArtImageCurrentlyPlayingBorder.VerticalOptions = LayoutOptions.Start; */
                    
                    
                    //UniversalMediaPlayerBarRowDefinition.Height = 100; 
                    //UniversalMediaPlayerBar.HeightRequest = 100; 
                    //UniversalMediaPlayerBar.IsVisible = true; 
                    //UniversalMediaPlayerBar.FadeTo(1, 500, Easing.SinIn); 
                    // opacity animation i think would make it too confusing with the slide in
                }
                currentlyPlayingSongData = givenSongData;

                if (LyricsListBottomContainer.Children.Count > 0) { LyricsListBottomContainer.Clear(); }
                if (givenSongData.Lyrics["Original"] != "")
                {
                    //SongLyricsTextLabel.Text = givenSongData.Lyrics["Original"];
                    LyricsLoadLabel(givenSongData.Lyrics["Original"]);
                    Picker translateToLabel = new Picker { ItemsSource = new List<string> { "Translate to..", "Transliterate to.." }, SelectedIndex = 0, TextColor = Color.Parse("White") };
                    Picker translateToLanguagePicker = new Picker { ItemsSource = translatableLanguages, SelectedIndex = 0, TextColor = Color.Parse("White") };
                    Microsoft.Maui.Controls.Button translateButton = new Microsoft.Maui.Controls.Button { Text = "🗪", BackgroundColor = Color.Parse("Transparent"), TextColor = Color.Parse("White"), FontFamily="NotoSansSymbols2" };
                    translateToLanguagePicker.SelectedIndexChanged += new EventHandler(LyricsTranslatePickerSelection_Changed);
                    translateToLabel.SelectedIndexChanged += new EventHandler(LyricsTranslatePickerSelection_Changed);
                    translateButton.Clicked += new EventHandler(LyricsTranslateButton_Clicked);
                    LyricsListBottomContainer.Add(translateToLabel);
                    LyricsListBottomContainer.Add(translateToLanguagePicker);
                    LyricsListBottomContainer.Add(translateButton);

                    UniversalMediaShowLyricsButton.IsVisible = true;
                }
                else
                {
                    LyricsListBottomContainer.Clear();
                    //SongLyricsTextLabel.Text = "Lyrics couldn't be found for this song.";
                    LyricsLoadLabel("Lyrics couldn't be found for this song.");
                    /*SongLyricsTextLabel.VerticalTextAlignment = TextAlignment.Center;
                    SongLyricsTextLabel.VerticalOptions = LayoutOptions.Center;
                    */
                    UniversalMediaShowLyricsButton.IsVisible = false;
                }
                UniversalMediaPlayStopButton.Text = "||";

            }
            catch (Exception ex)
            {
                await DisplayAlert("Whoa-oh!", ex.Message, "Alright");
            }


        }
        private void UniversalMediaSlider_DragCompleted(object sender, EventArgs e)
        {
            UniversalMediaSlider_IsBeingDragged = false;
            UniversalMediaElementPlayer.SeekTo(TimeSpan.FromSeconds((UniversalMediaSlider.Value * UniversalMediaElementPlayer.Duration.TotalSeconds)));
            UniversalMediaSlider_IsBeingDragged = false;

        }
        private void UniversalMediaSlider_DragStarted(object sender, EventArgs e)
        {
            UniversalMediaSlider_IsBeingDragged = true;
        }
        private async void UniversalMediaElementPlayer_PositionChanged(object sender, CommunityToolkit.Maui.Core.Primitives.MediaPositionChangedEventArgs e)
        {

            // This has to be outside since we want to update the boxes during a drag too dependent on slider position rather than song/media position
            MainThread.BeginInvokeOnMainThread(new Action(async () =>
            {
                double umepDuration = UniversalMediaElementPlayer.Duration.TotalSeconds;
                TimeSpan umsPositionTimeSpan = TimeSpan.FromSeconds(UniversalMediaSlider.Value * umepDuration);
                string umsPosMinutes = formatTimeSpanData(umsPositionTimeSpan.Minutes.ToString());
                string umsPosSeconds = formatTimeSpanData(umsPositionTimeSpan.Seconds.ToString());
                string umsDurMinutes = formatTimeSpanData(UniversalMediaElementPlayer.Duration.Minutes.ToString());
                string umsDurSeconds = formatTimeSpanData(UniversalMediaElementPlayer.Duration.Seconds.ToString());
                UniversalMediaSlider_SongPosition.Text = $"{umsPosMinutes}:{umsPosSeconds}";
                UniversalMediaSlider_SongDuration.Text = $"{umsDurMinutes}:{umsDurSeconds}";
                if (currentlyPlayingSongData.LyricTimings != null && LyricsListLabelContainer.Children.Count > 1)
                {
                    // optimize later by only doing this when drag is performed, else just continue along with song and update next line from there
                    int iterator = 0;
                    //await DisplayAlert("LyricTimings Are Not Null", "Yeah", "Arigato");
                    foreach (Label line in LyricsListLabelContainer.Children)
                    {
                        if (iterator < currentlyPlayingSongData.LyricTimings.Length)
                        {
                            if (umsPositionTimeSpan > currentlyPlayingSongData.LyricTimings[iterator])
                            {
                                //line.TextColor = Color.Parse("White");
                                line.FadeTo(1.0);
                            }
                            else
                            {
                                //line.TextColor = Color.Parse("Gray"); 
                                line.Opacity = 0.5;
                            }


                            iterator++;
                        }
                    }
                }
                if (UniversalMediaSlider_IsBeingDragged == false)
                {
                    UniversalMediaSlider.Value = UniversalMediaElementPlayer.Position.TotalSeconds / UniversalMediaElementPlayer.Duration.TotalSeconds;
                    string umepPosMinutes = formatTimeSpanData(UniversalMediaElementPlayer.Position.Minutes.ToString());
                    string umepPosSeconds = formatTimeSpanData(UniversalMediaElementPlayer.Position.Seconds.ToString());
                    string umepDurMinutes = umsDurMinutes;
                    string umepDurSeconds = umsDurSeconds;

                    string timerString = string.Format("{0}:{1} | {2}:{3}", umepPosMinutes, umepPosSeconds, umepDurMinutes, umepDurSeconds);
                    string largeImagePath = null;
                    string potentialLargeImagePath = Path.Combine(FileSystem.Current.AppDataDirectory, "currentart.png");
                    if (File.Exists(potentialLargeImagePath))
                    {
                        largeImagePath = potentialLargeImagePath;
                    }
                    //Image image = new Image();
                    //image.Source =ImageSource.FromUri(new Uri(largeImagePath));
                    //SliderPosition.Text = UniversalMediaSlider.Value.ToString() + "\n" + UniversalMediaElementPlayer.Position.TotalSeconds.ToString() + "\n" + UniversalMediaElementPlayer.Duration.TotalSeconds.ToString();
                    //client.
                    Resources["UniversalMediaPlayerBar_RelativeSliderPosition"] = UniversalMediaSlider.Value * this.Width;
                    client.SetPresence(new RichPresence()
                    {
                        State = timerString,
                        Details = $"{UniversalMediaPlayer_CurrentlyPlayingName.Text}",
                        Assets = new Assets()
                        {
                            LargeImageKey = "https://i.ibb.co/7xfhYwh/rudymentarylogoborder.png", //https://i.ibb.co/3h58fm0/rudymentarylogo.png                                                                      
                            LargeImageText = $"{currentAlbumPlayed}",
                            SmallImageKey = "https://i.ibb.co/k43SbwS/music-note-3-svgrepo-com-1.png", //https://i.ibb.co/nsc6wWv/music-note-3-svgrepo-com.png
                            SmallImageText = $"Featuring {UniversalMediaPlayer_CurrentlyPlayingArtists.Text}"
                        }
                    });
                }
            }));


        }

        private void UniversalMediaElementPlayer_MediaEnded(object sender, EventArgs e)
        {

            MainThread.BeginInvokeOnMainThread(new Action(() =>
            {
                if (UniversalMediaPlayer_ShuffleFlag)
                {
                    Random randomGen = new Random();
                    int previousInd = songDataQueueIndex;
                    songDataQueueIndex = randomGen.Next(songDataQueue.Count);
                    if ((songDataQueueIndex == previousInd) && songDataQueue.Count > 1)
                    {
                        while (songDataQueueIndex == previousInd)
                        {
                            songDataQueueIndex = randomGen.Next(songDataQueue.Count);
                        }
                    }
                    UniversalMediaElementPlayer_NewSongSelected(songDataQueue[songDataQueueIndex]);
                }
                else
                {
                    songDataQueueIndex++;
                    //await DisplayAlert("Queue Index", songDataQueueIndex.ToString(), "Alright");
                    if (songDataQueueIndex >= songDataQueue.Count && UniversalMediaPlayer_LoopFlag)
                    {
                        songDataQueueIndex = 0;
                    }
                    if (songDataQueueIndex < songDataQueue.Count)
                    {
                        UniversalMediaElementPlayer_NewSongSelected(songDataQueue[songDataQueueIndex]);

                    }
                    else
                    {
                        UniversalMediaElementPlayer.Pause();
                        //UniversalMediaElementPlayer.Source = "";
                        //UniversalMediaElementPlayer.SeekTo(TimeSpan.Zero);
                    }
                }
            }));
        }
        private void UniversalMediaElementPlayer_ShuffleFlagClicked(object sender, EventArgs e)
        {
            UniversalMediaPlayer_ShuffleFlag = !UniversalMediaPlayer_ShuffleFlag;
            var sendingObj = (Microsoft.Maui.Controls.Button)sender;
            if (UniversalMediaPlayer_ShuffleFlag)
            {
                sendingObj.Opacity = 1;
            }
            else
            {
                sendingObj.Opacity = 0.5;
            }
        }
        private void UniversalMediaElementPlayer_LoopFlagClicked(object sender, EventArgs e)
        {
            UniversalMediaPlayer_LoopFlag = !UniversalMediaPlayer_LoopFlag;
            var sendingObj = (Microsoft.Maui.Controls.Button)sender;
            if (UniversalMediaPlayer_LoopFlag)
            {
                sendingObj.Opacity = 1;
            }
            else
            {
                sendingObj.Opacity = 0.5;
            }
        }
        private void UniversalMediaElementPlayer_ShowLyricsClicked(object sender, EventArgs e)
        {
            if (UniversalMediaShowLyricsButton.Opacity == 1)
            {
                HomePageButton_Clicked(new object(), new EventArgs());
            }
            else
            {
                PageSongLyrics.IsVisible = true;
                PageAlbum.IsVisible = false;
                PageHome.IsVisible = false;
                PageCreatePlaylist.IsVisible = false;
                PageAddToPlaylist.IsVisible = false;
                PageSettings.IsVisible = false;
                UniversalMediaShowLyricsButton.Opacity = 1;
                //TappedEventArgs exTappedEvent = (TappedEventArgs)new object();
                //CloseCurrentlyPlayingScreen(new object(), exTappedEvent);

            }



        }

        private async void AddToPlaylistContext_Clicked(object sender, EventArgs e)
        {
            var menuFlyout = (MenuFlyoutItem)sender;

            var songDataVar = (SongData)menuFlyout.Parent.Parent.BindingContext;
            addToPlaylistToBeAdded = songDataVar;
            //await DisplayAlert("Okay", addToPlaylistToBeAdded.SongName, "Alright");
            PageAddToPlaylist.IsVisible = true;
            List<AddToPlaylistCustomClass> sourceList = new List<AddToPlaylistCustomClass>();
            int i = 0;
            foreach (AlbumData playlist in allSavedPlaylistData)
            {
                sourceList.Add(new AddToPlaylistCustomClass { PlaylistName = playlist.AlbumName, PlaylistIndex = i });
                i++;
            }
            AddToPlaylistSelectCollection.ItemsSource = sourceList;

            //await DisplayAlert("Element", songDataVar.SongPath, "Alright");
        }
        private async void RemoveFromPlaylistContext_Clicked(object sender, EventArgs e)
        {
            var menuFlyout = (MenuFlyoutItem)sender;
            var item = (SongData)menuFlyout.Parent.Parent.BindingContext;
            var albumData = (AlbumData)AlbumPageAlbumDataView.BindingContext;
            if (!albumData.IsPlaylist)
            {
                await DisplayAlert("Not a playlist", "Cannot remove songs from albums", "Alright");
                return;
            }
            int playlistIndex = allPlaylistToIndex[albumData.AlbumName];
            List<SongData> playlistSongs = albumData.Songs;
            List<SongData> newSongs = new List<SongData>();
            int iteratorI = 1;
            foreach (SongData s in playlistSongs)
            {
                if (s.RenderNumber == item.RenderNumber)
                {
                    continue;
                }
                SongData newS = s;
                newS.RenderNumber = iteratorI;
                iteratorI++;
                newSongs.Add(s);

            }
            albumData.Songs = newSongs;
            allSavedPlaylistData[playlistIndex] = albumData;
            AddToPlaylist_SaveToFile();
            AlbumPageSongCollection.ItemsSource = newSongs.ToObservableCollection();
            //BindableLayout.SetItemsSource(AlbumPageSongCollection, newSongs.ToObservableCollection());
            await DisplayAlert("Woah", albumData.AlbumName, "Alright");

        }
        private async void AddToPlaylist_SelectedPlaylist(object sender, TappedEventArgs e)
        {
            try
            {
                var button = (Frame)sender;
                AddToPlaylistCustomClass playlistInformation = (AddToPlaylistCustomClass)button.BindingContext;
                //await DisplayAlert("All saved playlist", allSavedPlaylistData.Count.ToString(), "Okay");
                //await DisplayAlert("ID", playlistInformation.PlaylistIndex.ToString(), "Alright");
                addToPlaylistToBeAdded.RenderNumber = allSavedPlaylistData[playlistInformation.PlaylistIndex].Songs.Count + 1;
                allSavedPlaylistData[playlistInformation.PlaylistIndex].Songs.Add(addToPlaylistToBeAdded);

                PageAddToPlaylist.IsVisible = false;
                AddToPlaylist_SaveToFile();
            }
            catch (Exception ex)
            {

            }

            //await DisplayAlert("ID", playlistInformation.PlaylistIndex.ToString(), "Alright");
        }
        private void AddToPlaylist_SaveToFile()
        {
            string playlistToSongsTxtPath = Path.Combine(FileSystem.Current.AppDataDirectory, "playlisttosongs.txt");
            List<AlbumDataByteArrayVer> toSaveAllSavedPlaylistData = new List<AlbumDataByteArrayVer>();
            foreach (AlbumData album in allSavedPlaylistData)
            {
                toSaveAllSavedPlaylistData.Add(new AlbumDataByteArrayVer { AlbumName = album.AlbumName, Songs = album.Songs, AlbumArt = byteToImageConverter.ConvertBackTo(album.AlbumArt), AlbumArtists = album.AlbumArtists, IsPlaylist = true, MultiDisc = album.MultiDisc });
            }
            File.WriteAllText(playlistToSongsTxtPath, JsonSerializer.Serialize(toSaveAllSavedPlaylistData));

        }

        private void HomePlaylistCollectionContext_DeletePlaylist(object sender, EventArgs e)
        {
            var button = (MenuFlyoutItem)sender;
            var item = (AlbumData)button.Parent.BindingContext;
            List<AlbumData> toSaveAllSavedPlaylistData = new List<AlbumData>();
            foreach (AlbumData album in allSavedPlaylistData)
            {
                if (album.AlbumName == item.AlbumName)
                {
                    continue;
                }
                toSaveAllSavedPlaylistData.Add(new AlbumData { AlbumName = album.AlbumName, Songs = album.Songs, AlbumArt = album.AlbumArt, AlbumArtists = album.AlbumArtists, IsPlaylist = true, MultiDisc = album.MultiDisc });
            }
            allSavedPlaylistData = toSaveAllSavedPlaylistData;
            AddToPlaylist_SaveToFile();
            MainThread.BeginInvokeOnMainThread(new Action(() =>
            {
                HomePlaylistsCollection.ItemsSource = allSavedPlaylistData;
            }));



        }

        private void Image_Loaded(object sender, EventArgs e)
        {
            var image = (Image)sender;
            //var item = sender.
            if (image.Source == null)
            {
                image.Source = ImageSource.FromFile("download_svgrepo_com.png");
            }
        }
        private string formatTimeSpanData(string givenString)
        {
            if (givenString.Length < 2)
            {
                givenString = "0" + givenString;
            }
            return givenString;
        }
        private List<AlbumData> GetAllSavedPlaylistData()
        {
            return allSavedPlaylistData;
        }

        private void UniversalMediaNextTrackButton_Clicked(object sender, EventArgs e)
        {
            SongSkippedAnimationCompilation();
            UniversalMediaElementPlayer_MediaEnded(new object(), new EventArgs());
        }

        private void UniversalMediaPreviousTrackButton_Clicked(object sender, EventArgs e)
        {
            if ((UniversalMediaElementPlayer.Position.TotalSeconds < 3 || ((UniversalMediaElementPlayer.Position.TotalSeconds / UniversalMediaElementPlayer.Duration.TotalSeconds) < 0.02)))
            {
                if (songDataQueueIndex > 0)
                {
                    songDataQueueIndex--;
                    SongBackAnimationCompilation();
                    UniversalMediaElementPlayer_NewSongSelected(songDataQueue[songDataQueueIndex]);
                }

            }
            else
            {
                UniversalMediaElementPlayer.SeekTo(TimeSpan.Zero);
                UniversalMediaElementPlayer.Play();
            }
        }

        private void UniversalMediaElementPlayer_VolumeSlider_DragCompleted(object sender, EventArgs e)
        {
            UniversalMediaElementPlayer.Volume = UniversalMediaElementPlayer_VolumeSlider.Value;
        }

        private void SearchSongSearchEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            GC.Collect(); GC.WaitForPendingFinalizers();
            SearchBar sendingobj = (SearchBar)sender;
            string startswith = sendingobj.Text;
            if (startswith.Length > 0) { SearchSongSearchResults.IsVisible = true; }
            else { SearchSongSearchResults.IsVisible = false; GC.Collect(); GC.WaitForPendingFinalizers(); }
            List<Tuple<string, SongData>> filteredResults = allSongNameToData.Where(tupleItem => tupleItem.Item1.ToLower().StartsWith(startswith.ToLower())).ToList();
            var extractedSongDataFromTuples = from t in filteredResults select t.Item2;
            SearchSongSearchResults.ItemsSource = extractedSongDataFromTuples.ToObservableCollection();
            /* this is a horrible implementation
            who wrote this? */
        }
        private void LyricsTranslatePickerSelection_Changed(object sender, EventArgs e)
        {
            //Picker senderobj = (Picker)sender;
            Picker senderobj = (Picker)LyricsListBottomContainer.Children.ElementAt(1);
            Picker translateOrTransliterate = (Picker)LyricsListBottomContainer.Children.ElementAt(0);
            List<string> conversionOptions = new List<string> { "", " (Transliterated)" };
            Microsoft.Maui.Controls.Button translateButton = (Microsoft.Maui.Controls.Button)LyricsListBottomContainer.Children.ElementAt(2);
            if (translatableLanguages[senderobj.SelectedIndex] == "Original")
            {
                //SongLyricsTextLabel.Text = currentlyPlayingSongData.Lyrics["Original"];
                LyricsLoadLabel(currentlyPlayingSongData.Lyrics["Original"]);
                translateButton.TextColor = Color.Parse("White");
            }
            else if (currentlyPlayingSongData.Lyrics.ContainsKey(translatableLanguages[senderobj.SelectedIndex] + conversionOptions[translateOrTransliterate.SelectedIndex]))
            {
                translateButton.TextColor = Color.Parse("White");
                //SongLyricsTextLabel.Text = currentlyPlayingSongData.Lyrics[translatableLanguages[senderobj.SelectedIndex] + conversionOptions[translateOrTransliterate.SelectedIndex]];
                LyricsLoadLabel(currentlyPlayingSongData.Lyrics[translatableLanguages[senderobj.SelectedIndex] + conversionOptions[translateOrTransliterate.SelectedIndex]]);
            }
            else
            {
                translateButton.TextColor = Color.Parse("Gray");
            }
        }
        private async void LyricsTranslateButton_Clicked(object sender, EventArgs e)
        {
            SongData currentSongCopy = currentlyPlayingSongData;
            //var translator = new GoogleTranslator();
            Picker languagePicked = (Picker)LyricsListBottomContainer.Children.ElementAt(1);
            Picker translateOrTransliterate = (Picker)LyricsListBottomContainer.Children.ElementAt(0);
            List<string> conversionOptions = new List<string> { "", " (Transliterated)" };
            if (translatableLanguages[languagePicked.SelectedIndex] == "Original")
            {
                return;
            }

            string convertedLyrics;
            if (translateOrTransliterate.SelectedIndex == 0)
            {
                var translator = new GoogleTranslator();
                var results = await translator.TranslateAsync(currentSongCopy.Lyrics["Original"], Language.GetLanguage(translatableLanguages[languagePicked.SelectedIndex]));

                convertedLyrics = results.Translation;
            }
            else
            {
                var translator = new GoogleTranslator2();
                var transliteratedResults = await translator.TransliterateAsync(currentSongCopy.Lyrics["Original"], Language.GetLanguage(translatableLanguages[languagePicked.SelectedIndex]));
                /*string[] resultLines = transliteratedResults.Transliteration.Split("\r");
                for (int i = 0; i< resultLines.Length; i++)
                {
                    if (resultLines[i].Length > 0) {
                        resultLines[i] = resultLines[i][0].ToString().ToUpper() + resultLines[i].Substring(1);  
                    }
                }
                await DisplayAlert("Woah", string.Join("\r", resultLines), "Okay I see I see"); */

                convertedLyrics = transliteratedResults.Transliteration;//string.Join("\n", resultLines);
            }

            // rough way of doing things for now, will try to create references to the places they're contained
            // don't want to mess with user-tags and lyrics since thats uh like kind of intrusive i'd guess
            //await DisplayAlert("Translated", "Nice", "Okay okay I get it");

            string dictionaryKey = translatableLanguages[languagePicked.SelectedIndex] + conversionOptions[translateOrTransliterate.SelectedIndex];
            foreach (AlbumData a in allSavedAlbumData)
            {
                foreach (SongData s in a.Songs)
                {
                    if (s.SongPath == currentSongCopy.SongPath)
                    {
                        if (s.Lyrics.ContainsKey(dictionaryKey)) { s.Lyrics.Remove(dictionaryKey); }
                        s.Lyrics.Add(dictionaryKey, convertedLyrics);

                    }
                }
            }
            foreach (AlbumData a in allSavedPlaylistData)
            {
                foreach (SongData s in a.Songs)
                {
                    if (s.SongPath == currentSongCopy.SongPath)
                    {
                        if (s.Lyrics.ContainsKey(dictionaryKey)) { s.Lyrics.Remove(dictionaryKey); }
                        s.Lyrics.Add(dictionaryKey, convertedLyrics);
                    }
                }
            }

            string albumsToSongsTxtPath = Path.Combine(FileSystem.Current.AppDataDirectory, "albumtosongs.txt");
            List<AlbumDataByteArrayVer> toSaveAllAlbumData = new List<AlbumDataByteArrayVer>();
            foreach (AlbumData album in allSavedAlbumData)
            {
                toSaveAllAlbumData.Add(new AlbumDataByteArrayVer { AlbumName = album.AlbumName, Songs = album.Songs, AlbumArt = byteToImageConverter.ConvertBackTo(album.AlbumArt), AlbumArtists = album.AlbumArtists, IsPlaylist = true, MultiDisc = album.MultiDisc });
            }
            File.WriteAllText(albumsToSongsTxtPath, JsonSerializer.Serialize(toSaveAllAlbumData));
            AddToPlaylist_SaveToFile();
            //SongLyricsTextLabel.Text = convertedLyrics;
            //await DisplayAlert("Translated", convertedLyrics + convertedLyrics.Split(Environment.NewLine).Length.ToString(), "Thanks");
            LyricsLoadLabel(convertedLyrics);
        }
        private void HomeSearchResultItem_Tapped(object sender, TappedEventArgs e)
        {
            var button = (Border)sender;
            var item = (SongData)button.BindingContext;
            songDataQueue = new List<SongData> { item };
            songDataQueueIndex = 0;
            UniversalMediaPlayer_AlbumArtImage.Source = allImageReferences[item.ImageKey];
            //UniversalMediaPlayer_AlbumArtImageCurrentlyPlaying.Source = allImageReferences[item.ImageKey];
            currentAlbumPlayed = item.AlbumName;
            UniversalMediaElementPlayer_NewSongSelected(item);
        }

        private void CheckForUpdatedPreferences()
        {
            string persistentPreferencesPath = Path.Combine(FileSystem.Current.AppDataDirectory, "preferences.txt");
            toggleablePreferences.Add("QuickLoadAlbums", false);
            if (File.Exists(persistentPreferencesPath))
            {
                toggleablePreferences = JsonSerializer.Deserialize<Dictionary<string, bool>>(File.ReadAllText(persistentPreferencesPath));
                foreach (HorizontalStackLayout x in SettingsPreferencesVerticalStack.Children)
                {
                    Switch toggleableElement = (Switch)x.Children.ElementAt(1);
                    toggleableElement.IsToggled = toggleablePreferences[toggleableElement.AutomationId];
                }
            }
            else
            {
                File.WriteAllText(persistentPreferencesPath, JsonSerializer.Serialize(toggleablePreferences));
            }
        }
        private void UpdatePreferenceSelection(object sender, ToggledEventArgs e)
        {
            string persistentPreferencesPath = Path.Combine(FileSystem.Current.AppDataDirectory, "preferences.txt");
            foreach (HorizontalStackLayout x in SettingsPreferencesVerticalStack.Children)
            {
                Switch toggleableElement = (Switch)x.Children.ElementAt(1);
                toggleablePreferences[toggleableElement.AutomationId] = toggleableElement.IsToggled;
            }
            File.WriteAllText(persistentPreferencesPath, JsonSerializer.Serialize(toggleablePreferences));

        }
        private async void LyricsLoadLabel(string lyricLabel)
        {
            LyricsListLabelContainer.Clear();
            string[] lyricsByLine = Regex.Split(lyricLabel, "\r?\n");
            //string[] lyricsByLine = lyricLabel.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None);
            if (lyricsByLine.Length == 1)
            {
                // separate if statement to preserve original formatting on normal lyrics while accounting for formatting given through transliteration   
                lyricLabel.ReplaceLineEndings();
                string[] lines = lyricLabel.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None);

                lyricsByLine = lines;
                //await DisplayAlert("How Many Lines?", lines.Length.ToString(), "Cool");
            }
            //await DisplayAlert("number of lyrics", lyricsByLine.Length.ToString(), "Thanks");
            Color defaultTextColor = Color.Parse("White");
            double textOpacity = 1;
            if (currentlyPlayingSongData.LyricTimings != null)
            {
                //defaultTextColor = Color.Parse("Gray"); 
                textOpacity = 0.5;
            }
            //LyricsListLabelContainer.Add(breakLine);
            FontSizeConverter convertToLarge = new FontSizeConverter();
            int automateID = 0;
            TapGestureRecognizer lyricTapped = new TapGestureRecognizer();
            lyricTapped.Tapped += (s, e) =>
            {
                // s = object sender, e = EventArgs
                Label senderobj = (Label)s;
                int automatedIdx = int.Parse(senderobj.AutomationId);
                MainThread.BeginInvokeOnMainThread(new Action(() =>
                {
                    //UniversalMediaElementPlayer.SeekTo(currentlyPlayingSongData.LyricTimings[automatedIdx]);
                    UniversalMediaSlider.Value = currentlyPlayingSongData.LyricTimings[automatedIdx].TotalSeconds / currentlyPlayingSongData.Duration.TotalSeconds;
                    UniversalMediaElementPlayer.SeekTo(currentlyPlayingSongData.LyricTimings[automatedIdx]);

                }));
            };
            foreach (string l in lyricsByLine)
            {
                Label lineToBeAdded = new Label { Text = l, FontSize = (double)convertToLarge.ConvertFromString("Large"), FontAttributes = FontAttributes.Bold, Opacity = textOpacity, TextColor = defaultTextColor, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, AutomationId = automateID.ToString() };
                if (currentlyPlayingSongData.LyricTimings != null) { lineToBeAdded.GestureRecognizers.Add(lyricTapped); }
                LyricsListLabelContainer.Add(lineToBeAdded);
                automateID++;
            }
            //LyricsListLabelContainer.Add(breakLine);

        }
        private void SongSkippedAnimationCompilation()
        {
            //Animation imageFadeFromLeft = new Animation(v => UniversalMediaPlayer_AlbumArtImage.Opacity = v, 0, 1, Easing.SinIn);
            Animation songNameFade = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingName.Opacity = v, 0, 1, Easing.CubicIn);
            Animation songFromRight = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingName.TranslationX = v, 25, 0, Easing.CubicIn);
            Animation artistNameFade = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingArtists.Opacity = v, 0, 1, Easing.CubicIn);
            Animation artistFromRight = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingArtists.TranslationX = v, 25, 0, Easing.CubicIn);
            // currently playing screen bound labels
            Animation songNameFadeCurrentlyPlaying = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingNameScreenVer.Opacity = v, 0, 1, Easing.CubicIn);
            Animation songFromRightCurrentlyPlaying = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingNameScreenVer.TranslationX = v, 25, 0, Easing.CubicIn);
            Animation artistNameFadeCurrentlyPlaying = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingArtistsScreenVer.Opacity = v, 0, 1, Easing.CubicIn);
            Animation artistFromRightCurrentlyPlaying = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingArtistsScreenVer.TranslationX = v, 25, 0, Easing.CubicIn);
            //imageFadeFromLeft.Commit(this, "albumImageSkipFade", 16, 1000);
            songNameFade.Commit(this, "songNameSkipFade", 16, 500);
            artistNameFade.Commit(this, "artistNameSkipFade", 16, 500);
            songFromRight.Commit(this, "songNameFromRight", 16, 500);
            artistFromRight.Commit(this, "artistNameFromRight", 16, 500);
            // currently playing animation commits
            songNameFadeCurrentlyPlaying.Commit(this, "songNameSkipFadeCurrentlyPlaying", 16, 500);
            artistNameFadeCurrentlyPlaying.Commit(this, "artistNameSkipFadeCurrentlyPlaying", 16, 500);
            songFromRightCurrentlyPlaying.Commit(this, "songNameFromRightCurrentlyPlaying", 16, 500);
            artistFromRightCurrentlyPlaying.Commit(this, "artistNameFromRightCurrentlyPlaying", 16, 500);

        }
        private void SongBackAnimationCompilation()
        {
            Animation songNameFade = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingName.Opacity = v, 0, 1, Easing.CubicIn);
            Animation songFromLeft = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingName.TranslationX = v, -25, 0, Easing.CubicIn);
            Animation artistNameFade = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingArtists.Opacity = v, 0, 1, Easing.CubicIn);
            Animation artistFromLeft = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingArtists.TranslationX = v, -25, 0, Easing.CubicIn);
            // currently playing screen bound labels
            Animation songNameFadeCurrentlyPlaying = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingNameScreenVer.Opacity = v, 0, 1, Easing.CubicIn);
            Animation songFromLeftCurrentlyPlaying = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingNameScreenVer.TranslationX = v, -25, 0, Easing.CubicIn);
            Animation artistNameFadeCurrentlyPlaying = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingArtistsScreenVer.Opacity = v, 0, 1, Easing.CubicIn);
            Animation artistFromLeftCurrentlyPlaying = new Animation(v => UniversalMediaPlayer_CurrentlyPlayingArtistsScreenVer.TranslationX = v, -25, 0, Easing.CubicIn);
            //
            songNameFade.Commit(this, "songNameSkipFade", 16, 500);
            artistNameFade.Commit(this, "artistNameSkipFade", 16, 500);
            songFromLeft.Commit(this, "songNameFromLeft", 16, 500);
            artistFromLeft.Commit(this, "artistNameFromLeft", 16, 500);
            // currently  playing animation commits
            songNameFadeCurrentlyPlaying.Commit(this, "songNameSkipFadeCurrentlyPlaying", 16, 500);
            artistNameFadeCurrentlyPlaying.Commit(this, "artistNameSkipFadeCurrentlyPlaying", 16, 500);
            songFromLeftCurrentlyPlaying.Commit(this, "songNameFromLeftCurrentlyPlaying", 16, 500);
            artistFromLeftCurrentlyPlaying.Commit(this, "artistNameFromLeftCurrentlyPlaying", 16, 500);
        }
        private void AlbumPageOpenedAnimationCompilation()
        {
            Animation enterTopDown = new Animation(v => AlbumPageExitButton.TranslationY = v, -25, 0, Easing.CubicOut);
            Animation fadeTopDown = new Animation(v => AlbumPageExitButton.Opacity = v, 0, 1, Easing.CubicOut);
            enterTopDown.Commit(this, "albumPageTopEnter", 16, 250);
            fadeTopDown.Commit(this, "albumPageTopFade", 16, 250);

        }

        private void OpenCurrentlyPlayingScreen(object sender, SwipedEventArgs e)
        {
            PageCurrentlyPlaying.IsVisible = true;
            //int a = PageCurrentlyPlaying.TranslationX;
            double windowHeight = DeviceDisplay.MainDisplayInfo.Height;

            Animation slideFromBottom = new Animation(v => PageCurrentlyPlaying.TranslationY = v, this.Height, 0, Easing.CubicInOut);
            slideFromBottom.Commit(this, "currentlyPlayingBottom", 8, 500);
        }
        

        private void CloseCurrentlyPlayingScreen(object sender, TappedEventArgs e)
        {
            //PageCurrentlyPlaying.IsVisible = false;
            double windowHeight = DeviceDisplay.MainDisplayInfo.Height;

            Animation slideFromScreen = new Animation(v => PageCurrentlyPlaying.TranslationY = v, 0, this.Height, Easing.CubicInOut);
            slideFromScreen.Commit(this, "currentlyPlayingExit", 16, 300, finished: (val, canceled) =>
            {
                if (!canceled) { PageCurrentlyPlaying.IsVisible = false; }
            });
            

        }

        private void UniversalMediaPlayerBarSwipeLeft(object sender, SwipedEventArgs e)
        {
            UniversalMediaNextTrackButton_Clicked(new object(), new EventArgs());
        }
        private void UniversalMediaPlayerBarSwipeRight(object sender, SwipedEventArgs e)
        {
            UniversalMediaPreviousTrackButton_Clicked(new object(), new EventArgs());
        }

        private async void PageAlbum_Scrolled(object sender, ScrolledEventArgs e)
        {
            if (albumDataItemsSourceObservable.Count == albumDataItemsSourceTotal.Count) { return; }
            ScrollView scrollView = (ScrollView)sender;
            double scrollingSpace = scrollView.ContentSize.Height - scrollView.Height;

            if (scrollingSpace - e.ScrollY < 100 )
            {

                if (albumDataItemsSourceObservable.Count != albumDataItemsSourceTotal.Count)
                {

                    //albumDataItemsSourceObservable = albumDataItemsSourceTotal.ToObservableCollection();
                    
                    for (int i = albumDataItemsSourceObservable.Count; i < Math.Min(albumDataItemsTakeVal+10, albumDataItemsSourceTotal.Count); i++)
                    {
                        albumDataItemsSourceObservable.Add(albumDataItemsSourceTotal[i]);
                    }
                    //OnPropertyChanged(nameof(albumDataItemsSourceObservable));
                }
                albumDataItemsTakeVal += 10;
            } else
            {
                // do nothing
            }
            
        }

        private async void DragGestureRecognizer_DragStarting(object sender, DragStartingEventArgs e)
        {
            try
            {
                DragGestureRecognizer sendingObj = (DragGestureRecognizer)sender;
                addToPlaylistContentFrame = (Frame)sendingObj.Parent;
            } catch (Exception ex)
            {
                await DisplayAlert("Error found drag", ex.Message, "Okay");
            }
            
            //await DisplayAlert("Type", sendingObj.Parent.GetType().ToString(), "Okay");

        }

        private async void DropGestureRecognizer_Drop(object sender, DropEventArgs e)
        {
            try
            {
            //var droppedSong = e.Data;
                addToPlaylistToBeAdded = (SongData)addToPlaylistContentFrame.BindingContext;
                await DisplayAlert("Dropped item", addToPlaylistToBeAdded.SongName, "Muchas gracias");
                AlbumData currentAlbumData = (AlbumData)AlbumPageAlbumDataView.BindingContext;
                if (currentAlbumData.IsPlaylist == false)
                {
                    
                    List<AddToPlaylistCustomClass> sourceList = new List<AddToPlaylistCustomClass>();
                    int i = 0;
                    foreach (AlbumData playlist in allSavedPlaylistData)
                    {
                        sourceList.Add(new AddToPlaylistCustomClass { PlaylistName = playlist.AlbumName, PlaylistIndex = i });
                        i++;
                    }
                    AddToPlaylistSelectCollection.ItemsSource = sourceList;
                    PageAddToPlaylist.IsVisible = true;
                } else
                {

                }
            } catch (Exception ex)
            {
                await DisplayAlert("Error encountered", ex.Message, "Okay");
            }
            
        }

        
    }
}
