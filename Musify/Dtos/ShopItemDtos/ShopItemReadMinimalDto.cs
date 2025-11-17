namespace Musify.Dtos.ShopItemDtos
{
    public record ShopItemReadMinimalDto
    {
        public required int Id { get; init; }

        public required int InstrumentId { get; init; }

        public required decimal Price { get; init; }

        public required int Stock { get; init; }

        public required string Condition { get; init; }
    }
}
