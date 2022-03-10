using Microsoft.JSInterop;

namespace Lieb.Data
{
    public class TimeZoneService
    {
        private readonly IJSRuntime _jsRuntime;

        private TimeSpan? _userOffset;
        private int _offsetInMinutes;

        public TimeZoneService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async ValueTask<DateTimeOffset> GetLocalDateTime(DateTimeOffset dateTime)
        {
            if (_userOffset == null)
            {
                _offsetInMinutes = await _jsRuntime.InvokeAsync<int>("GetTimezoneValue");
                _userOffset = TimeSpan.FromMinutes(-_offsetInMinutes);
            }

            return dateTime.ToOffset(_userOffset.Value);
        }

        public async ValueTask<DateTimeOffset> GetUTCDateTime(DateTimeOffset dateTime)
        {
            if (_userOffset == null)
            {
                _offsetInMinutes = await _jsRuntime.InvokeAsync<int>("GetTimezoneValue");
                _userOffset = TimeSpan.FromMinutes(-_offsetInMinutes);
            }

            return new DateTimeOffset(dateTime.DateTime.AddMinutes(_offsetInMinutes), new TimeSpan(0));
        }

        public async ValueTask<string> GetUserTimeZone()
        {
             return await _jsRuntime.InvokeAsync<string>("GetTimezone");
        }
    }
}
