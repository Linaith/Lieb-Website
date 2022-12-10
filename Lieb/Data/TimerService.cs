using Lieb.Models.GuildWars2.Raid;

namespace Lieb.Data
{
    public class TimerService : IHostedService, IDisposable 
    {
        private Timer _minuteTimer = null!;
        private Timer _fiveMinuteTimer = null!;
        private Timer _dailyTimer = null!;
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
            _dailyTimer = new Timer(CleanUpDatabase, null, TimeSpan.Zero,
                TimeSpan.FromDays(1));

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
                    DateTime UTCEndTime = TimeZoneInfo.ConvertTimeToUtc(template.EndTime, timeZone);
                    if(template.Interval > 0 && UTCEndTime.AddDays(-template.CreateDaysBefore) < DateTime.UtcNow)
                    {
                        raidTemplateService.CreateRaidFromTemplate(template.RaidTemplateId).Wait();
                    }
                }

                var raidService =
                    scope.ServiceProvider
                        .GetRequiredService<RaidService>();
                await raidService.SendReminders();
                await raidService.RemoveMaybes();
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

        private async void CleanUpDatabase(object? state)
        {
            using (var scope = _services.CreateScope())
            {
                var userService =
                    scope.ServiceProvider
                        .GetRequiredService<UserService>();
                await userService.DeleteInactiveUsers();
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
