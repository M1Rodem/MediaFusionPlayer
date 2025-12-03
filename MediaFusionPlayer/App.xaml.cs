// App.xaml.cs — финальная временная версия
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
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IFileService, FileService>();
                    services.AddSingleton<IPlaylistService, PlaylistService>();
                    services.AddSingleton<IMediaPlayerService, MediaPlayerService>();
                    services.AddSingleton<MainViewModel>();

                    services.AddSingleton<Presentation.Views.MainWindow>(sp => new Presentation.Views.MainWindow
                    {
                        DataContext = sp.GetRequiredService<MainViewModel>()
                    });
                })
                .Build();
        }


        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();
            _host.Services.GetRequiredService<Presentation.Views.MainWindow>().Show();
            base.OnStartup(e);

        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
                await _host.StopAsync();
            base.OnExit(e);
        }
    }
}