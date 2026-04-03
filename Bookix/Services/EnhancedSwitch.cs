using Microsoft.Maui.Handlers;

namespace Bookix.Services
{
    public class EnhancedSwitch : Switch
    {
        static EnhancedSwitch()
        {
            Microsoft.Maui.Handlers.SwitchHandler.Mapper.AppendToMapping(nameof(EnhancedSwitch), (handler, view) =>
            {
                if (view is EnhancedSwitch es)
                {
                    es.MinimumWidthRequest = 0;
                }
            });
        }
    }
}
