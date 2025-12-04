using MediaFusionPlayer.Core.Interfaces;
using MediaFusionPlayer.Infrastructure.FileServices;
using MediaFusionPlayer.Infrastructure.Services;
using MediaFusionPlayer.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;

namespace MediaFusionPlayer
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Регистрируем сервисы
                    services.AddSingleton<IFileService, FileService>();
                    services.AddSingleton<IPlaylistService, PlaylistService>();
                    services.AddSingleton<IMediaPlayerService, MediaPlayerService>();
                    services.AddSingleton<IVideoPlayerService, VideoPlayerService>();
                    services.AddSingleton<VideoSyncService>();
                    services.AddSingleton<MainViewModel>();

                    // Главное окно — создаём через DI
                    services.AddSingleton<Presentation.Views.MainWindow>(sp =>
                    {
                        var window = new Presentation.Views.MainWindow
                        {
                            DataContext = sp.GetRequiredService<MainViewModel>()
                        };
                        return window;
                    });
                })
                .Build();
        }

        // Обработчик события Startup (вместо StartupUri)
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = _host.Services.GetRequiredService<Presentation.Views.MainWindow>();
            mainWindow.Show();
            mainWindow.Activate();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
                await _host.StopAsync();

            base.OnExit(e);
        }
    }
}