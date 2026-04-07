using Bookix.Services;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Handlers;
using System.Text;
using Microsoft.Maui.LifecycleEvents;
using The49.Maui.BottomSheet;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using WinRT.Interop;
#endif

namespace Bookix
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureMauiHandlers(handlers =>
                {
#if ANDROID
                    handlers.AddHandler(typeof(EnhancedLabel), typeof(Bookix.Services.EnhancedLabelHandler));
#endif
                })
                .ConfigureLifecycleEvents(events =>
                {
#if WINDOWS
                    events.AddWindows(windows =>
                    {
                        windows.OnWindowCreated(window =>
                        {
                            var nativeWindow = WindowNative.GetWindowHandle(window);
                            var windowId = Win32Interop.GetWindowIdFromWindow(nativeWindow);
                            var appWindow = AppWindow.GetFromWindowId(windowId);
                            // Set the initial size of the window
                            appWindow.Resize(new SizeInt32(720, 1280));
                            // Optional: Set the title of the window
                            appWindow.Title = "Bookix";
                        });
                    });
#endif
                })
                .UseBottomSheet()
                .UseMauiCommunityToolkit(options =>
                {
                    options.SetShouldEnableSnackbarOnWindows(true);
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Pusia Bold.otf", "PusiaBold");
                    fonts.AddFont("Merriweather-Italic", "MerriweatherItalic");
                    fonts.AddFont("Merriweather-Regular", "MerriweatherRegular");
                });

//#if ANDROID
//            Bookix.Platforms.Android.EnhancedLabelHandler.Register();
//#endif
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}  
