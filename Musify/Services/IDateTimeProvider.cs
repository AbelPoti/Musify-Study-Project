namespace Musify.Services
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow { get; }
    }
}
