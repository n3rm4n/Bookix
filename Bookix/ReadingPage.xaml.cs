using VersOne.Epub;
using HtmlAgilityPack;
using System.Xml.Linq;
using System.IO.Compression;
using CommunityToolkit.Maui.Alerts;
using Bookix.Services;
using Microsoft.Maui.Controls;

namespace Bookix;

public partial class ReadingPage : ContentPage
{
    private string _filePath;
    private bool _isInitialLoad = true;
    public ReadingPage(Book selectedBook)
    {
        InitializeComponent();
        Title = selectedBook.Title;
        _filePath = selectedBook.FilePath;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadingSpinner.IsVisible = true;
        LoadingSpinner.IsRunning = true;

        var bookChunks = await LoadBookChunks(_filePath);

        await Toast.Make($"Загружено { bookChunks.Count} блоков.", CommunityToolkit.Maui.Core.ToastDuration.Short, 14).Show();

        LoadingSpinner.IsRunning = false;
        LoadingSpinner.IsVisible = false;

        await RestoreReadingPosition(bookChunks);
    }

    private async Task RestoreReadingPosition(List<string> chunks)
    {
        BookListView.ItemsSource = chunks;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            int savedIndex = Preferences.Default.Get($"Bookmark_{_filePath}", 0);

            if (savedIndex > 0 && savedIndex < chunks.Count)
            {
                // CollectionView.ScrollTo поддерживает передачу элемента
                var targetItem = chunks[savedIndex];
                BookListView.ScrollTo(targetItem, position: ScrollToPosition.Start);
            }
            _isInitialLoad = false;
        });
    }

    // Для CollectionView используем событие Scrolled и FirstVisibleItemIndex
    private void OnBookChunksScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        if (_isInitialLoad) return;

        if (e.FirstVisibleItemIndex >= 0)
        {
            Preferences.Default.Set($"Bookmark_{_filePath}", e.FirstVisibleItemIndex);
        }
    }

    private async Task<List<string>> LoadBookChunks(string filePath)
    {
        try
        {
            // Handle .fb2
            if (filePath.EndsWith(".fb2", StringComparison.OrdinalIgnoreCase))
            {
                using var stream = File.OpenRead(filePath);
                return ReadFb2FromStream(stream);
            }
            // Handle .fb2.zip
            if (filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                using var archive = ZipFile.OpenRead(filePath);
                var entry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".fb2", StringComparison.OrdinalIgnoreCase));
                if (entry != null)
                {
                    using var stream = entry.Open();
                    return ReadFb2FromStream(stream);
                }
            }

            // Handle .epub
            if (filePath.EndsWith(".epub", StringComparison.OrdinalIgnoreCase))
            {
                return await ReadEpubChunksAsync(filePath);
            }


            return new List<string> { "Format not recognized." };
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", "Ошибка чтения типа файла.\nПопробуйте поддерживаемый формат", "Ok");
            return new List<string> { $"Error: {ex.Message}" };
        }
    }

    private async Task<List<string>> ReadEpubChunksAsync(string filePath)
    {
        EpubBook book = await EpubReader.ReadBookAsync(filePath);
        List<string> allParagraphs = new List<string>();
        //using (var reader = new StreamReader(stream, System.Text.Encoding.GetEncoding("windows-1251")))   понадобится для фикса epub
        //{                                                                                                 это для кодировок по типу windows-1251
        //    var content = await reader.ReadToEndAsync();                                                  есть еще koi8-r и прочие
        //}
        foreach (EpubLocalTextContentFile textFile in book.ReadingOrder)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(textFile.Content);

            // Target common text containers
            var nodes = htmlDoc.DocumentNode.SelectNodes("//p | //div | //span | //li");

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    // Only take nodes that actually contain text and aren't tiny artifacts
                    string text = node.InnerText.Trim();
                    if (!string.IsNullOrWhiteSpace(text) && text.Length > 3)
                    {
                        // Clean up HTML entities like &nbsp; or &quot;
                        string cleanedText = System.Net.WebUtility.HtmlDecode(text);
                        allParagraphs.Add(cleanedText);
                    }
                }
            }
            else
            {
                // Emergency fallback: If no tags found, split the whole body by line breaks
                var bodyText = htmlDoc.DocumentNode.InnerText;
                var lines = bodyText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line)) allParagraphs.Add(line.Trim());
                }
            }
        }
        return allParagraphs;
    }

    private List<string> ReadFb2FromStream(Stream stream)
    {
        XDocument doc = XDocument.Load(stream);
        XNamespace fb2Ns = "http://www.gribuser.ru/xml/fictionbook/2.0";

        // We look for BOTH paragraphs (p) and verse lines (v)
        var textNodes = doc.Descendants()
                           .Where(n => n.Name == fb2Ns + "p" || n.Name == fb2Ns + "v")
                           .Select(n => n.Value.Trim())
                           .Where(t => !string.IsNullOrWhiteSpace(t))
                           .ToList();

        return textNodes;
    }
}