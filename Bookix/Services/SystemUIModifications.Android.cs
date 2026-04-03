#if ANDROID
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Handlers;
using Msg = CommunityToolkit.Maui.Alerts;
namespace Bookix.Services
{
    public partial class EnhancedLabelHandler : LabelHandler
    {
        protected override void ConnectHandler(AppCompatTextView platformView)
        {
            base.ConnectHandler(platformView);
            // Applying custom callback for the  selection menu
            platformView.CustomSelectionActionModeCallback = new CustomSelectionCallback();
        }
    }

    public class CustomSelectionCallback : Java.Lang.Object, ActionMode.ICallback
    {
        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            menu.Add(0, 101, 0, "Словарь"); // (Id, GroupId, Order, Title)
            return true; // true - to create menu
        }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            if (item.ItemId == 101)
            {

                // Логика нажатия. Можно через MessagingCenter или WeakReference Messenger 
                // отправить событие в общую часть кода.
                Msg.Toast.Make("Нажата моя кнопка!", CommunityToolkit.Maui.Core.ToastDuration.Short, 14).Show();
                mode.Finish(); // Closing menu
                return true;
            }
            return false;
        }

        public bool OnPrepareActionMode(ActionMode mode, IMenu menu) => false;
        public void OnDestroyActionMode(ActionMode mode) { }
    }
}
#endif