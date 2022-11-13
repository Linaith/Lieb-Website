using Lieb.Models.GuildWars2.Raid;

namespace Lieb.Data
{
    public class TimerService : IHostedService, IDisposable 
    {
        private Timer _timer = null!;
        private IServiceProvider _services;

        public TimerService(IServiceProvider services)
        {
            _services = services;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(CheckRaids, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private async void CheckRaids(object? state)
        {
            using (var scope = _services.CreateScope())
            {
                var raidTemplateService =
                    scope.ServiceProvider
                        .GetRequiredService<RaidTemplateService>();

                foreach(RaidTemplate template in raidTemplateService.GetTemplates())
                {
                    TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(template.TimeZone);
                    DateTime UTCStartTime = TimeZoneInfo.ConvertTimeToUtc(template.StartTime, timeZone);
                    if(UTCStartTime.AddDays(-template.CreateDaysBefore).AddHours(1) < DateTime.UtcNow)
                    {
                        raidTemplateService.CreateRaidFromTemplate(template.RaidTemplateId).Wait();
                    }
                }

                var raidService =
                    scope.ServiceProvider
                        .GetRequiredService<RaidService>();
                await raidService.SendReminders();
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
