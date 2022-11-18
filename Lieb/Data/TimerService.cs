using Lieb.Models.GuildWars2.Raid;

namespace Lieb.Data
{
    public class TimerService : IHostedService, IDisposable 
    {
        private Timer _minuteTimer = null!;
        private Timer _fiveMinuteTimer = null!;
        private IServiceProvider _services;

        public TimerService(IServiceProvider services)
        {
            _services = services;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _minuteTimer = new Timer(CheckRaids, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(1));
            _fiveMinuteTimer = new Timer(CleanUpRaids, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(5));

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

        private async void CleanUpRaids(object? state)
        {
            using (var scope = _services.CreateScope())
            {
                var raidService =
                    scope.ServiceProvider
                        .GetRequiredService<RaidService>();
                await raidService.CleanUpRaids();
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _minuteTimer?.Change(Timeout.Infinite, 0);
            _fiveMinuteTimer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _minuteTimer?.Dispose();
            _fiveMinuteTimer?.Dispose();
        }
    }
}
