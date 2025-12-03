using Microsoft.Extensions.Logging;
using Kwiktomes.Data;
using Kwiktomes.Extensions;

namespace Kwiktomes
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Add Blazor WebView
            builder.Services.AddMauiBlazorWebView();

            // Configure dependency injection
            builder.Services.AddKwiktomesDatabase();
            builder.Services.AddKwiktomesServices();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
