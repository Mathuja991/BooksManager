namespace BooksApi.Models;

public sealed class BookUpsertDto
{
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string Isbn { get; set; }
    public required string PublicationDate { get; set; }
}
