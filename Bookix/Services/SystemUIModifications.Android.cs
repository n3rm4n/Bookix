#if ANDROID
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Handlers;
using The49.Maui.BottomSheet;
using Msg = CommunityToolkit.Maui.Alerts;
namespace Bookix.Services
{
    public partial class EnhancedLabelHandler : LabelHandler
    {
        protected override void ConnectHandler(AppCompatTextView platformView)
        {
            base.ConnectHandler(platformView);
            platformView.CustomSelectionActionModeCallback = new CustomSelectionCallback(platformView);
        }
    }

    public class CustomSelectionCallback : Java.Lang.Object, ActionMode.ICallback
    {
        private readonly AppCompatTextView _textView;
        public CustomSelectionCallback(AppCompatTextView textView)
        {
            _textView = textView;
        }
        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            menu.Add(0, 101, 0, "Словарь"); // (Id, GroupId, Order, Title)
            return true; // true - to create menu
        }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            if (item.ItemId == 101)
            {
                // 1. Получаем границы выделенного текста
                int selStart = _textView.SelectionStart;
                int selEnd = _textView.SelectionEnd;
                int min = Math.Max(0, Math.Min(selStart, selEnd));
                int max = Math.Max(0, Math.Max(selStart, selEnd));

                // 2. Вырезаем выделенное слово
                string selectedText = _textView.Text.Substring(min, max - min);

                // 3. Открываем шторку в главном потоке MAUI
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var sheet = new WordDefinition(selectedText);
                    await sheet.ShowAsync(App.Current.MainPage.Window);
                });

                mode.Finish(); // Закрываем контекстное меню Android
                return true;
            }
            return false;
        }

        public bool OnPrepareActionMode(ActionMode mode, IMenu menu) => false;
        public void OnDestroyActionMode(ActionMode mode) { }
    }
}
#endif