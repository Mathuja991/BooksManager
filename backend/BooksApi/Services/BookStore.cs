using BooksApi.Models;

namespace BooksApi.Services;

public sealed class BookStore
{
    private readonly object _gate = new();
    private readonly List<BookDto> _books = [];
    private int _nextId = 1;

    public BookStore()
    {
        _books.Add(
            new BookDto
            {
                Id = _nextId++,
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Isbn = "9780132350884",
                PublicationDate = "2008-08-01",
            });
        _books.Add(
            new BookDto
            {
                Id = _nextId++,
                Title = "The Pragmatic Programmer",
                Author = "Andrew Hunt, David Thomas",
                Isbn = "9780201616224",
                PublicationDate = "1999-10-30",
            });
    }

    public IReadOnlyList<BookDto> GetAll()
    {
        lock (_gate)
        {
            return _books
                .OrderBy(b => b.Id)
                .Select(Clone)
                .ToList();
        }
    }

    public BookDto? GetById(int id)
    {
        lock (_gate)
        {
            var found = _books.FirstOrDefault(b => b.Id == id);
            return found is null ? null : Clone(found);
        }
    }

    public BookDto Create(BookUpsertDto dto)
    {
        lock (_gate)
        {
            var created = new BookDto
            {
                Id = _nextId++,
                Title = dto.Title.Trim(),
                Author = dto.Author.Trim(),
                Isbn = dto.Isbn.Trim(),
                PublicationDate = dto.PublicationDate.Trim(),
            };

            _books.Add(created);
            return Clone(created);
        }
    }

    public BookDto? Update(int id, BookUpsertDto dto)
    {
        lock (_gate)
        {
            var idx = _books.FindIndex(b => b.Id == id);
            if (idx < 0) return null;

            var updated = new BookDto
            {
                Id = id,
                Title = dto.Title.Trim(),
                Author = dto.Author.Trim(),
                Isbn = dto.Isbn.Trim(),
                PublicationDate = dto.PublicationDate.Trim(),
            };

            _books[idx] = updated;
            return Clone(updated);
        }
    }

    public bool Delete(int id)
    {
        lock (_gate)
        {
            var idx = _books.FindIndex(b => b.Id == id);
            if (idx < 0) return false;
            _books.RemoveAt(idx);
            return true;
        }
    }

    private static BookDto Clone(BookDto book) =>
        new()
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Isbn = book.Isbn,
            PublicationDate = book.PublicationDate,
        };
}
