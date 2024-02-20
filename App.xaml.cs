using System.Text.Json;

namespace Rudymentary
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            
            MainPage = new AppShell();
            LoadThemeJson();
        }
        private void LoadThemeJson()
        {
            try
            {
                string themeJsonPath = Path.Combine(FileSystem.Current.AppDataDirectory, "theme.json");
                if (File.Exists(themeJsonPath))
                {
                    foreach (KeyValuePair<string,string> t in JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(themeJsonPath)).AsEnumerable())
                    {
                        Application.Current.Resources[t.Key] = Color.FromArgb(t.Value);
                    }
                }
            } catch (Exception e)
            {

            }
        }

    }
}