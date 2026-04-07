using System.Text.Json;
using The49.Maui.BottomSheet;
namespace Bookix;

public partial class BookSettings : BottomSheet
{
    private Book _currentBook;
    private Func<Task> _onDeletedCallback;
    private bool _bookSettingsClosingCalled;
    

    public BookSettings(Book book, Func<Task> onDeletedCallback)
    {
        InitializeComponent();
        _currentBook = book;
        _onDeletedCallback = onDeletedCallback;
    }

    private async void OnDeleteBookButtonClicked(object sender, EventArgs e)
    {
        if (_currentBook == null) return;

        bool confirm = await Application.Current.MainPage.DisplayAlert("Удаление", $"Точно удалить '{_currentBook.Title}'?", "Да", "Отмена");
        if (!confirm) return;

        // 2. Достаем список сохраненных книг
        var savedBooksJson = Preferences.Default.Get("SavedBooks", "[]"); //в mainpage чуть такой же код, лучше скопировать его будет
        var books = JsonSerializer.Deserialize<List<Book>>(savedBooksJson);

        // 3. Ищем эту книгу в списке (ищем по FilePath, так как он уникален для каждой книги)
        var bookToRemove = books.FirstOrDefault(b => b.FilePath == _currentBook.FilePath);

        if (bookToRemove != null)
        {
            // Удаляем из списка
            books.Remove(bookToRemove);

            // 4. Пересохраняем обновленный список обратно в Preferences
            Preferences.Default.Set("SavedBooks", JsonSerializer.Serialize(books));
        }

        // 5. Закрываем шторку (BottomSheet)
        
        await DismissAsync();
        if (_onDeletedCallback != null)
        {
            await _onDeletedCallback.Invoke();
        }

        // ВАЖНО: После закрытия шторки нужно обновить CollectionView на MainPage!
    }
}