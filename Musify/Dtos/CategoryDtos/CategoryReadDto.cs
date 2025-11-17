namespace Musify.Dtos.CategoryDtos
{
    public record CategoryReadDto
    {
        public required int Id { get; init; }

        public required string Name { get; init; }

        public int? ParentId { get; init; }
    }
}
