namespace Musify.Dtos.ShopItemDtos
{
    public record ShopItemReadMinimalDto(int Id, int InstrumentId, decimal Price, int Stock, string Condition);
}
