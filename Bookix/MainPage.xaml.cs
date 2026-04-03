using Bookix.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading;
namespace Bookix;

public partial class MainPage : ContentPage
{
    public ObservableCollection<Book> MyBooks { get; set; } = [];

    public MainPage()
    {
        InitializeComponent();
        _ = LoadBooks();
        BooksCollection.ItemsSource = MyBooks;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await LoadBooks(); // Refresh list when coming back from Add Page    
    }
    private async void OnBookClicked(object? sender, EventArgs e)
    {
        if (sender is VisualElement visElement && visElement.BindingContext is Book book)
        {
            var sb = Toast.Make("Начало загрузки книги", ToastDuration.Short, 14);
            await MainThread.InvokeOnMainThreadAsync(async () => await sb.Show());
            await Navigation.PushAsync(new ReadingPage(book)); // Navigate to the Reading Page and pass the selected book info
        }
    }
    private async void OnSideMenuButtonTapped(object sender, EventArgs e)
    {
        await SideMenu.Open();
    }

    private async Task LoadBooks()
    {
        var savedBooks = Preferences.Default.Get("SavedBooks", string.Empty);
        if (!string.IsNullOrEmpty(savedBooks))
        {
            var books = JsonSerializer.Deserialize<List<Book>>(savedBooks);
            MyBooks.Clear();
            if (books != null)
            {
                // Compute progress for each book without blocking UI
                foreach (var book in books)
                {
                    MyBooks.Add(book);
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            int chunks = await BookChunkService.CountChunksAsync(book.FilePath);
                            int savedIndex = Preferences.Default.Get($"Bookmark_{book.FilePath}", 0);
                            int progress = 0;
                            if (chunks > 0)
                            {
                                // savedIndex can be equal to chunks (end), clamp
                                var clamped = Math.Clamp(savedIndex, 0, chunks);
                                progress = (int)Math.Round((double)clamped / chunks * 100);
                            }

                            MainThread.BeginInvokeOnMainThread(() => book.Progress = progress);
                        }
                        catch
                        {
                            await DisplayAlert("Ошибка", "Не получается посчитать процент прочтения", "Ok");
                        }
                    });
                }
            }
        }
    }

    private async void OnAddBookClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddBookPage()); // Opens the new window to add a book
    }
    private async void OnBookSelected(object sender, SelectionChangedEventArgs e) // не используется // удали уже. зачем оставлять?
    {
        if (e.CurrentSelection.FirstOrDefault() is Book selectedBook)
        {
            var sb = Snackbar.Make("Начало загрузки книги", () => DisplayAlert("Напоминание", "бу-бу-бу", "ОК", "чтааа", FlowDirection.RightToLeft), "pika", TimeSpan.FromSeconds(5), new SnackbarOptions
            {
                BackgroundColor = Colors.DarkSlateGray,
                TextColor = Colors.White,
                ActionButtonTextColor = Colors.LightBlue,
                CornerRadius = new CornerRadius(8),
                CharacterSpacing = 0.1
                
            });
            await MainThread.InvokeOnMainThreadAsync(async () => await sb.Show());
            await Navigation.PushAsync(new ReadingPage(selectedBook)); // Navigate to the Reading Page and pass the selected book data

            ((CollectionView)sender).SelectedItem = null; // Deselect the item so you can click it again later
        }
    }
    private async void OnBookSettingsButtonClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var currentBook = button.BindingContext as Book;
        if (currentBook != null)
        {
            var bookSettingsPage = new BookSettings(currentBook, async () => await LoadBooks());
            await bookSettingsPage.ShowAsync(Window);
        }

#if ANDROID
        var toast = Toast.Make("Открытие дополнительных опций");
        await toast.Show();
#endif
    }
}