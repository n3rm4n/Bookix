using System.Text.Json;
namespace Bookix;

public partial class AddBookPage : ContentPage
{
    private string selectedFilePath = string.Empty;
    private string selectedCoverPath = "cover.png";

    public AddBookPage()
    {
        InitializeComponent();
    }

    private async void OnPickFileClicked(object sender, EventArgs e)
    {
        try
        {
            // Restrict file types
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] {
                    "application/epub+zip",
                    "application/x-fictionbook+xml",
                    "text/xml",
                    "application/xml",
                    "application/zip",
                    "application/x-zip-compressed"
                } },
                { DevicePlatform.WinUI, new[] { ".epub", ".fb2", ".zip" } },
            });

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите файл книги",
                FileTypes = customFileType
            });

            if (result != null)
            {
                selectedFilePath = result.FullPath;
                FilePathLabel.Text = result.FileName;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Could not pick file: " + ex.Message, "OK");
        }
    }

    private async void OnSaveBookClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text) || string.IsNullOrWhiteSpace(selectedFilePath))
        {
            await DisplayAlert("Error", "Please enter a title and select a file.", "OK");
            return;
        }

        var newBook = new Book
        {
            Title = TitleEntry.Text,
            Author = AuthorEntry.Text ?? "Unknown Author",
            FilePath = selectedFilePath,
            CoverPath = selectedCoverPath
        };

        // Load existing, add new, and save
        var savedBooksJson = Preferences.Default.Get("SavedBooks", "[]");
        var books = JsonSerializer.Deserialize<List<Book>>(savedBooksJson);
        books.Add(newBook);

        Preferences.Default.Set("SavedBooks", JsonSerializer.Serialize(books));

        // Go back to main menu
        await Navigation.PopAsync();
    }

    private async void OnChooseBookCover(object sender, EventArgs e)
    {
        try
        {
            // 1. Pick the photo
            var photo = await MediaPicker.Default.PickPhotoAsync();

            if (photo != null)
            {
                // 2. Create a permanent path in the app's local folder
                string localFolder = FileSystem.AppDataDirectory;
                string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                string localFilePath = Path.Combine(localFolder, newFileName);

                // 3. Copy the file there
                using (var sourceStream = await photo.OpenReadAsync())
                using (var localStream = File.OpenWrite(localFilePath))
                {
                    await sourceStream.CopyToAsync(localStream);
                }

                // 4. Update UI and variable
                selectedCoverPath = localFilePath;
                CoverPreview.Source = ImageSource.FromFile(localFilePath);
                CoverPreview.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", "Не получилось выбрать изображение: " + ex.Message, "OK");
        }
    }
}