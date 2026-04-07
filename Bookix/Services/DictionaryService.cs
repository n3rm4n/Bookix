using SQLite;

namespace Bookix.Services;

// ВАЖНО: Имена таблицы и колонок (Table, Column) должны ТОЧНО совпадать 
// с теми, что созданы внутри вашего файла dal-dictionary.db!
[Table("dictionary")] // Замените на реальное имя таблицы в БД
public class DictionaryEntry
{
    [Column("key")] // Замените на имя колонки со словом
    public string Word { get; set; }

    [Column("value")] // Замените на имя колонки со значением
    public string Definition { get; set; }
}

public static class DictionaryService
{
    private static SQLiteAsyncConnection _db;
    private const string DbName = "dal_dictionary.db";

    public static async Task Init()
    {
        if (_db != null) return;

        // Полный путь к файлу в рабочей папке приложения на телефоне
        string targetPath = Path.Combine(FileSystem.AppDataDirectory, DbName);

        // ПРОВЕРКА: Если файла нет в рабочей папке, копируем его из ресурсов
        if (!File.Exists(targetPath))
        {
            try
            {
                // Открываем поток из ресурсов (папка Resources/Raw)
                using Stream inputStream = await FileSystem.OpenAppPackageFileAsync(DbName);

                // Создаем файл в доступной для записи папке
                using FileStream outputStream = File.Create(targetPath);

                await inputStream.CopyToAsync(outputStream);
            }
            catch (Exception ex)
            {
                // Если здесь упадет, значит файл не найден именно в Resources/Raw 
                // или не помечен как MauiAsset
                throw new Exception($"Не удалось скопировать базу данных: {ex.Message}");
            }
        }

        // Открываем соединение уже по рабочему пути
        _db = new SQLiteAsyncConnection(targetPath);
    }

    public static async Task<string> GetDefinitionAsync(string word)
    {
        await Init();

        // Очищаем слово от пробелов и знаков препинания
        string cleanWord = word.Trim(' ', ',', '.', '!', '?', '"', '\'').ToUpper();

        // Ищем в БД
        var entry = await _db.Table<DictionaryEntry>()
                             .Where(x => x.Word.ToLower() == cleanWord)
                             .FirstOrDefaultAsync();

        return entry?.Definition ?? "Определение не найдено.";
    }
}