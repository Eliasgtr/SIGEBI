namespace Sigebi.Domain.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
}
