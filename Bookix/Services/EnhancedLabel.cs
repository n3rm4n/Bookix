using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.PlatformConfiguration;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using WinRT.Interop;
#endif

namespace Bookix.Services
{
    public class EnhancedLabel : Label
    {
        public bool IsTextSelectionEnabled { get; set; } = false;
        static EnhancedLabel()
        {
            Microsoft.Maui.Handlers.LabelHandler.Mapper.AppendToMapping(
                nameof(EnhancedLabel),
                (handler, view) =>
                {
                    if (view is Bookix.Services.EnhancedLabel selLabel && selLabel.IsTextSelectionEnabled)
                    {
#if ANDROID
                        handler.PlatformView.SetTextIsSelectable(true);
                        //handler.PlatformView.LongClickable = true;
                        //handler.PlatformView.MovementMethod = Android.Text.Method.ScrollingMovementMethod.Instance;


#endif
#if WINDOWS
                        handler.PlatformView.IsTextSelectionEnabled = true;
#endif
                    }
                });
        }
    }
}
