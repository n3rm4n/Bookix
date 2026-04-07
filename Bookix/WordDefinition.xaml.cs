using The49.Maui.BottomSheet;
namespace Bookix;

public partial class WordDefinition : BottomSheet
{
    public WordDefinition(string word)
    {
        InitializeComponent();


        // Загружаем значение из БД
        LoadDefinitionAsync(word);
    }

    private async void LoadDefinitionAsync(string word)
    {
        DefinitionLabel.Text = "Поиск в словаре...";

        // Получаем определение через наш сервис
        string definition = await Services.DictionaryService.GetDefinitionAsync(word);
        if (definition != "Определение не найдено.")
        {
            DefinitionLabel.Text = word + ", " + definition;
        }
        else
        {
            DefinitionLabel.Text = definition;
        }
    }
}