using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace MediaFusionPlayer
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Регистрируем сервисы
            services.AddSingleton<Core.Interfaces.IFileService, Infrastructure.FileServices.FileService>();
            services.AddSingleton<Core.Interfaces.IPlaylistService, Infrastructure.Services.PlaylistService>();
            services.AddSingleton<Core.Interfaces.IEqualizerService, Infrastructure.Services.EqualizerService>();
            services.AddSingleton<Core.Interfaces.IMediaPlayerService, Infrastructure.Services.MediaPlayerService>();
            services.AddSingleton<Core.Interfaces.IVideoPlayerService, Infrastructure.Services.VideoPlayerService>();

            // Регистрируем ViewModels
            services.AddSingleton<Presentation.ViewModels.MainViewModel>();
            services.AddSingleton<Presentation.ViewModels.EqualizerViewModel>(); // Изменяем на Singleton

            // Регистрируем окна
            services.AddSingleton<Presentation.Views.MainWindow>();
            services.AddSingleton<Presentation.Views.EqualizerWindow>(); // Регистрируем окно эквалайзера
        }

        // ДОБАВЬ это свойство для доступа к сервисам из других частей приложения
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services => _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetRequiredService<Presentation.Views.MainWindow>();
            mainWindow.DataContext = _serviceProvider.GetRequiredService<Presentation.ViewModels.MainViewModel>();
            mainWindow.Show();
        }
    }
}