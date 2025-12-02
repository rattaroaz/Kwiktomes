using Kwikbooks.Data;

namespace Kwikbooks
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private static bool _databaseInitialized = false;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            
            // Initialize database synchronously to ensure it's ready before app starts
            if (!_databaseInitialized)
            {
                Task.Run(async () => await InitializeDatabaseAsync()).GetAwaiter().GetResult();
                _databaseInitialized = true;
            }
        }

        private async Task InitializeDatabaseAsync()
        {
            await DatabaseInitializer.InitializeAsync(_serviceProvider);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "Kwikbooks" };
        }
    }
}
