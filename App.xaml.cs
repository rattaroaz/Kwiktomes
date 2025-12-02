using Kwikbooks.Data;

namespace Kwikbooks
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            
            // Initialize database on startup
            InitializeDatabaseAsync().ConfigureAwait(false);
        }

        private async Task InitializeDatabaseAsync()
        {
            await DatabaseInitializer.InitializeAsync(_serviceProvider);
            await DatabaseInitializer.SeedDataAsync(_serviceProvider);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "Kwikbooks" };
        }
    }
}
