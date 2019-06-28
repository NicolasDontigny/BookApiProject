using BookApiProject.Services;
using BookApiProject.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApiProject.Models;
using System.Web.Http.Cors;

namespace BookApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : Controller
    {
        private IBookRepository _bookRepository;
        private IAuthorRepository _authorRepository;
        private ICategoryRepository _categoryRepository;
        private IReviewRepository _reviewRepository;

        public BooksController(IBookRepository bookRepository, IAuthorRepository authorRepository, ICategoryRepository categoryRepository, IReviewRepository reviewRepository)
        {
            _authorRepository = authorRepository;
            _categoryRepository = categoryRepository;
            _bookRepository = bookRepository;
            _reviewRepository = reviewRepository;
        }

        // api/books
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<BookDto>))]
        [ProducesResponseType(400)]
        public IActionResult GetBooks()
        {
            var books = _bookRepository.GetBooks();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var booksDto = new List<BookDto>();

            foreach(var book in books)
            {
                booksDto.Add(new BookDto
                {
                    Id = book.Id,
                    Isbn = book.Isbn,
                    Title = book.Title,
                    DatePublished = book.DatePublished
                });
            }

            return Ok(booksDto);
        }

        // api/books/bookId
        [HttpGet("{bookId}", Name = "GetBook")]
        [ProducesResponseType(200, Type = typeof(BookDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetBook(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
            {
                return NotFound();
            }

            var book = _bookRepository.GetBook(bookId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDto = new BookDto
            {
                Id = book.Id,
                Isbn = book.Isbn,
                Title = book.Title,
                DatePublished = book.DatePublished
            };

            return Ok(bookDto);
        }

        // api/books/isbn/bookIsbn
        [HttpGet("isbn/{bookIsbn}")]
        [ProducesResponseType(200, Type = typeof(BookDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetBook(string bookIsbn)
        {
            if (!_bookRepository.BookExists(bookIsbn))
            {
                return NotFound();
            }

            var book = _bookRepository.GetBook(bookIsbn);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDto = new BookDto
            {
                Id = book.Id,
                Isbn = book.Isbn,
                Title = book.Title,
                DatePublished = book.DatePublished
            };

            return Ok(bookDto);
        }

        [HttpGet("rating/{bookId}")]
        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetBookRating(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
            {
                return NotFound();
            }

            var rating = _bookRepository.GetBookRating(bookId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(rating);
        }

        // api/books?authId=1&authId=2&catId=1&catId=2
        // To get the list of author and category Ids, we need to get them from the URI
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(Book))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        public IActionResult CreateBook([FromQuery]List<int> authId, [FromQuery]List<int> catId, 
                                        [FromBody]Book newBook)
        {
            var statusCode = ValidateBook(authId, catId, newBook);

            if (!ModelState.IsValid)
            {
                return StatusCode(statusCode.StatusCode, ModelState);
            }

            // Now we have checked our validations, we can try adding the new book to the database
            if (!_bookRepository.CreateBook(authId, catId, newBook))
            {
                ModelState.AddModelError("", $"Something went wrong while saving the book " +
                                               $"{newBook.Title}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetBook", new { bookId = newBook.Id }, newBook);
        }

        // api/books/bookId?authId=1&authId=2&catId=1&catId=2
        // To get the list of author and category Ids, we need to get them from the URI
        [HttpPut("{bookId}")]
        [ProducesResponseType(201, Type = typeof(Book))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        public IActionResult UpdateBook(int bookId, [FromQuery]List<int> authId, [FromQuery]List<int> catId,
                                        [FromBody]Book newBook)
        {
            // Check that the bookId from URI matches the book object id
            if (bookId != newBook.Id)
            {
                return BadRequest(ModelState);
            }

            // Check that the book exists in the database
            if (!_bookRepository.BookExists(bookId))
            {
                ModelState.AddModelError("", $"Book with id {bookId} doesn't exist");
                return StatusCode(404, ModelState);
            }

            // Validate Book Isbn, authors and categories
            var statusCode = ValidateBook(authId, catId, newBook);

            if (!ModelState.IsValid)
            {
                return StatusCode(statusCode.StatusCode, ModelState);
            }

            // Now we have checked our validations, we can try updating the book to the database
            if (!_bookRepository.UpdateBook(authId, catId, newBook))
            {
                ModelState.AddModelError("", $"Something went wrong while updating the book " +
                                               $"{newBook.Title}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        // api/books/bookId
        // To get the list of author and category Ids, we need to get them from the URI
        [HttpDelete("{bookId}")]
        [ProducesResponseType(201, Type = typeof(Book))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult UpdateBook(int bookId)
        {
            // Check that the book exists in the database
            if (!_bookRepository.BookExists(bookId))
            {
                ModelState.AddModelError("", $"Book with id {bookId} doesn't exi st");
                return StatusCode(404, ModelState);
            }

            var bookToDelete = _bookRepository.GetBook(bookId);
            var reviewsToDelete = _reviewRepository.GetReviewsOfABook(bookId).ToList();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Now we have checked our validations, we can try updating the book to the database
            if (!_bookRepository.DeleteBook(bookToDelete))
            {
                ModelState.AddModelError("", $"Something went wrong while deleting the book " +
                                               $"{bookToDelete.Title}");
                return StatusCode(500, ModelState);
            }

            if (!_reviewRepository.DeleteReviews(reviewsToDelete))
            {
                ModelState.AddModelError("", $"Something went wrong while deleting reviews of the book " +
                                              $"{bookToDelete.Title}");
            }

            return NoContent();
        }

        private StatusCodeResult ValidateBook(List<int> authId, List<int> catId, Book book)
        {
            if (book == null || authId.Count() <= 0 || catId.Count() <= 0)
            {
                ModelState.AddModelError("", "Missing book, author or category");
                return StatusCode(400);
            }

            if (_bookRepository.IsDuplicateISBN(book.Id, book.Isbn))
            {
                ModelState.AddModelError("", "Duplicate ISBN");
                return StatusCode(422);
            }

            foreach(var id in authId)
            {
                if (!_authorRepository.AuthorExists(id))
                {
                    ModelState.AddModelError("", $"Author with id {id} doesn't exist");
                    return StatusCode(404);
                }
            }

            foreach (var id in catId)
            {
                if (!_categoryRepository.CategoryExists(id))
                {
                    ModelState.AddModelError("", $"Category with id {id} doesn't exist");
                    return StatusCode(404);
                }
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Critical Error");
                return BadRequest();
            }

            return NoContent();
        }
    }
}
