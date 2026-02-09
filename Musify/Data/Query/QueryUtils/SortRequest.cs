namespace Musify.Data.Query.QueryUtils
{
    public class SortRequest
    {
        public string? SortBy { get; init; }

        public bool Descending { get; init; } = false;
    }
}