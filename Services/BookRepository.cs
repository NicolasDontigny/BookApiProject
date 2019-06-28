using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApiProject.Models;

namespace BookApiProject.Services
{
    public class BookRepository : IBookRepository
    {
        private BookDbContext _bookContext;

        public BookRepository(BookDbContext bookContext)
        {
            _bookContext = bookContext;
        }

        public bool BookExists(int bookId)
        {
            return _bookContext.Books.Any(b => b.Id == bookId);
        }

        public bool BookExists(string isbn)
        {
            return _bookContext.Books.Any(b => b.Isbn == isbn);
        }

        public bool CreateBook(List<int> authorsId, List<int> categoriesId, Book book)
        {
            // Since a Book can have many authors, and many categories, we pass the author and category Ids in the arguments
            // Now we can retrieve all the authors and categories we want to associate to our new book
            var authors = _bookContext.Authors.Where(a => authorsId.Contains(a.Id)).ToList();
            var categories = _bookContext.Categories.Where(c => categoriesId.Contains(c.Id)).ToList();
            
            // Then, we need to create a BookAuthor for each author, because that's how books and authors are linked together in the database
            foreach(var author in authors)
            {
                var bookAuthor = new BookAuthor
                {
                    Author = author,
                    Book = book
                };

                // Once we have created the BookAuthor, add it to the database
                _bookContext.Add(bookAuthor);
            }

            // Then, we need to create a BookCategory for each category, because that's how books and categories are linked together in the database
            foreach (var category in categories)
            {
                var bookCategory = new BookCategory
                {
                    Category = category,
                    Book = book
                };

                // Once we have created the BookCategory, add it to the database
                _bookContext.Add(bookCategory);
            }

            _bookContext.Add(book);
            return Save();
        }

        public bool DeleteBook(Book book)
        {
            _bookContext.Remove(book);
            return Save();
        }

        public Book GetBook(int bookId)
        {
            return _bookContext.Books.Where(b => b.Id == bookId).FirstOrDefault();
        }

        public Book GetBook(string isbn)
        {
            return _bookContext.Books.Where(b => b.Isbn == isbn).FirstOrDefault();
        }

        public decimal GetBookRating(int bookId)
        {
            var reviews = _bookContext.Reviews.Where(r => r.Book.Id == bookId).ToList();

            if (reviews.Count() <= 0)
            {
                return 0;
            }

            return ((decimal)reviews.Sum(r => r.Rating) / reviews.Count());
        }

        public ICollection<Book> GetBooks()
        {
            return _bookContext.Books.OrderBy(b => b.Title).ToList();
        }

        public bool IsDuplicateISBN(int bookId, string isbn)
        {
            return _bookContext.Books.Any(b => b.Isbn.Trim().ToUpper() == isbn.Trim().ToUpper() && b.Id != bookId);
        }

        public bool Save()
        {
            var saved = _bookContext.SaveChanges();
            return saved >= 0;
        }

        public bool UpdateBook(List<int> authorsId, List<int> categoriesId, Book book)
        {
            // Since a Book can have many authors, and many categories, we pass the author and category Ids in the arguments
            // Now we can retrieve all the authors and categories we want to associate to our updated book
            // The easiest way to update all relationships will be to delete all BookAuthors and BookCategories associated to that book, and create new ones
            var authors = _bookContext.Authors.Where(a => authorsId.Contains(a.Id)).ToList();
            var categories = _bookContext.Categories.Where(c => categoriesId.Contains(c.Id)).ToList();

            var bookAuthorsToDelete = _bookContext.BookAuthors.Where(ba => ba.BookId == book.Id).ToList();
            var bookCategoriesToDelete = _bookContext.BookCategories.Where(ba => ba.BookId == book.Id).ToList();

            _bookContext.RemoveRange(bookAuthorsToDelete);
            _bookContext.RemoveRange(bookCategoriesToDelete);

            // Then, we need to create a BookAuthor for each author, because that's how books and authors are linked together in the database
            foreach (var author in authors)
            {
                var bookAuthor = new BookAuthor
                {
                    Author = author,
                    Book = book
                };

                // Once we have created the BookAuthor, add it to the database
                _bookContext.Add(bookAuthor);
            }

            // Then, we need to create a BookCategory for each category, because that's how books and categories are linked together in the database
            foreach (var category in categories)
            {
                var bookCategory = new BookCategory
                {
                    Category = category,
                    Book = book
                };

                // Once we have created the BookCategory, add it to the database
                _bookContext.Add(bookCategory);
            }

            _bookContext.Update(book);
            return Save();
        }
    }
}
