using Core.Services.Helper.Interface;

namespace Core.Services.Helper;

public class TimeService : ITimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}