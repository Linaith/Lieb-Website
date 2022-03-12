using Microsoft.JSInterop;

namespace Lieb.Data
{
    public class TimeZoneService
    {
        private readonly IJSRuntime _jsRuntime;

        public TimeZoneService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async ValueTask<DateTimeOffset> GetLocalDateTime(DateTimeOffset dateTime)
        {
                int offsetInMinutes = await _jsRuntime.InvokeAsync<int>("GetTimezoneValue", dateTime);
                TimeSpan userOffset = TimeSpan.FromMinutes(-offsetInMinutes);

            return dateTime.ToOffset(userOffset);
        }

        public async ValueTask<DateTimeOffset> GetUTCDateTime(DateTimeOffset dateTime)
        {
            int offsetInMinutes = await _jsRuntime.InvokeAsync<int>("GetTimezoneValue", dateTime);
            TimeSpan userOffset = TimeSpan.FromMinutes(-offsetInMinutes);

            return new DateTimeOffset(dateTime.DateTime.AddMinutes(offsetInMinutes), new TimeSpan(0));
        }

        public async ValueTask<string> GetUserTimeZone()
        {
             return await _jsRuntime.InvokeAsync<string>("GetTimezone");
        }
    }
}
