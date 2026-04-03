//using System;
//using System.Reflection;
//using Microsoft.Maui.Controls;
//using Microsoft.Maui.Dispatching;
//using Microsoft.Maui.ApplicationModel;
//#if WINDOWS
//using Microsoft.UI.Xaml.Controls;
//using Microsoft.UI.Xaml.Input;
//#endif

//namespace Bookix.Services
//{
//    // Поведение для Label: на Windows подписывается на PointerReleased нативного TextBlock
//    // и обрезает завершающие пробелы в выделении.
//    public class TrimLabelSelectionBehavior : Behavior<Label>
//    {
//        Label? _associated;
//#if WINDOWS
//        TextBlock? _tb;
//#endif

//        protected override void OnAttachedTo(Label bindable)
//        {
//            base.OnAttachedTo(bindable);
//            _associated = bindable;
//            bindable.HandlerChanged += OnHandlerChanged;
//            TrySetupHandler(bindable);
//        }

//        void OnHandlerChanged(object? sender, EventArgs e) => TrySetupHandler(_associated);

//        void TrySetupHandler(Label? label)
//        {
//            if (label == null) return;
//#if WINDOWS
//            try
//            {
//                if (label.Handler?.PlatformView is TextBlock tb)
//                {
//                    // Отписываем старый обработчик на случай повторной инициализации
//                    if (_tb != null && _tb != tb)
//                    {
//                        try { _tb.PointerReleased -= OnPointerReleased; } catch { }
//                    }

//                    _tb = tb;
//                    _tb.IsTextSelectionEnabled = true;
//                    _tb.PointerReleased += OnPointerReleased;
//                }
//                else
//                {
//                    Report("TrySetupHandler: PlatformView is not TextBlock or handler not ready", null);
//                }
//            }
//            catch (Exception ex)
//            {
//                Report("TrySetupHandler exception", ex);
//            }
//#endif
//        }

//#if WINDOWS
//        void OnPointerReleased(object sender, PointerRoutedEventArgs e)
//        {
//            try
//            {
//                TrimSelection(_tb);
//            }
//            catch (Exception ex)
//            {
//                Report("OnPointerReleased exception", ex);
//            }
//        }

//        void TrimSelection(TextBlock? tb)
//        {
//            if (tb == null) return;

//            var type = tb.GetType();
//            var propSelectedText = type.GetProperty("SelectedText", BindingFlags.Public | BindingFlags.Instance);
//            var propSelectionStart = type.GetProperty("SelectionStart", BindingFlags.Public | BindingFlags.Instance);
//            var propSelectionLength = type.GetProperty("SelectionLength", BindingFlags.Public | BindingFlags.Instance);
//            var propText = type.GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);

//            if (propSelectedText == null || propSelectionStart == null || propSelectionLength == null || propText == null)
//            {
//                Report("TrimSelection: required properties not found via reflection", null);
//                return;
//            }

//            string? selected = propSelectedText.GetValue(tb) as string;
//            if (string.IsNullOrEmpty(selected)) return;

//            if (selected.EndsWith(" "))
//            {
//                int start = (int)propSelectionStart.GetValue(tb)!;
//                int length = (int)propSelectionLength.GetValue(tb)!;
//                string text = propText.GetValue(tb) as string ?? string.Empty;

//                if (length <= 0 || start < 0 || start + length > text.Length) return;

//                int shrink = 0;
//                while (length - shrink > 0 && text[start + length - shrink - 1] == ' ') shrink++;

//                if (shrink > 0 && length - shrink > 0)
//                {
//                    propSelectionLength.SetValue(tb, length - shrink);
//                }
//            }
//        }
//#endif

//        protected override void OnDetachingFrom(Label bindable)
//        {
//            base.OnDetachingFrom(bindable);
//            bindable.HandlerChanged -= OnHandlerChanged;
//#if WINDOWS
//            if (_tb != null)
//            {
//                try { _tb.PointerReleased -= OnPointerReleased; } catch { }
//                _tb = null;
//            }
//#endif
//            _associated = null;
//        }

//        void Report(string where, Exception? ex)
//        {
//            try
//            {
//                var page = Application.Current?.MainPage;
//                if (page != null)
//                {
//                    MainThread.BeginInvokeOnMainThread(async () =>
//                    {
//                        var msg = ex != null ? $"{where}: {ex.Message}" : where;
//                        await page.DisplayAlert("TrimLabelSelectionBehavior", msg, "OK");
//                    });
//                }
//                System.Diagnostics.Debug.WriteLine($"TrimLabelSelectionBehavior: {where} {ex}");
//            }
//            catch { }
//        }
//    }
//}