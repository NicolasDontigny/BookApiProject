using BookApiProject.Services;
using BookApiProject.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApiProject.Models;

namespace BookApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : Controller
    {
        private IAuthorRepository _authorRepository;
        private IBookRepository _bookRepository;
        private ICountryRepository _countryRepository;

        public AuthorsController(IAuthorRepository authorRepository, IBookRepository bookRepository, ICountryRepository countryRepository)
        {
            _authorRepository = authorRepository;
            _bookRepository = bookRepository;
            _countryRepository = countryRepository;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<AuthorDto>))]
        [ProducesResponseType(400)]
        public IActionResult GetAuthors()
        {
            var authors = _authorRepository.GetAuthors();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authorsDto = new List<AuthorDto>();
            foreach(var author in authors)
            {
                authorsDto.Add(new AuthorDto
                {
                    Id = author.Id,
                    FirstName = author.FirstName,
                    LastName = author.LastName
                });
            }

            return Ok(authorsDto);
        }

        // api/authors/authorId
        [HttpGet("{authorId}", Name = "GetAuthor")]
        [ProducesResponseType(200, Type = typeof(AuthorDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var author = _authorRepository.GetAuthor(authorId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authorDto = new AuthorDto
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName
            };
           
            return Ok(authorDto);
        }

        // api/authors/books/bookId
        [HttpGet("books/{bookId}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<AuthorDto>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetAuthorsOfABook(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
            {
                return NotFound();
            }

            var authors = _authorRepository.GetAuthorsOfABook(bookId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authorsDto = new List<AuthorDto>();
            foreach(var author in authors)
            {
                authorsDto.Add(new AuthorDto
                {
                    Id = author.Id,
                    FirstName = author.FirstName,
                    LastName = author.LastName
                });
            }
           
            return Ok(authorsDto);
        }

        // api/authors/authorId/books
        [HttpGet("{authorId}/books")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<BookDto>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetBooksByAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var books = _authorRepository.GetBooksByAuthor(authorId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var booksDto = new List<BookDto>();
            foreach (var book in books)
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

        // api/authors
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(Author))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult CreateAuthor(Author newAuthor)
        {
            if (newAuthor == null)
            {
                return BadRequest(ModelState);
            }

            // Check if the Author's Country is Valid
            if (!_countryRepository.CountryExists(newAuthor.Country.Id))
            {
                ModelState.AddModelError("", "Country doesn't exist");
                return StatusCode(404, ModelState);
            }

            // Store the FULL Country object in newAuthor
            newAuthor.Country = _countryRepository.GetCountry(newAuthor.Country.Id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Then I can try to create the Author and add it to the database
            if (!_authorRepository.CreateAuthor(newAuthor))
            {
                ModelState.AddModelError("", $"Something went wrong while trying to create {newAuthor.FirstName} {newAuthor.LastName}");
                return StatusCode(500, ModelState);
            }

            // If we reach this point, the author has been added to the database. I can return the new author object
            return CreatedAtRoute("GetAuthor", new { authorId = newAuthor.Id }, newAuthor);
        }

        // api/authors/authorId
        [HttpPut("{authorId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public IActionResult UpdateAuthor(int authorId, [FromBody]Author updatedAuthor)
        {
            if (updatedAuthor == null)
            {
                return BadRequest(ModelState);
            }

            // Check if authorId from URI matches the authorId from Body
            if (authorId != updatedAuthor.Id)
            {
                return BadRequest(ModelState);
            }

            // Check if author exists in the database
            if (_authorRepository.GetAuthor(authorId) == null)
            {
                ModelState.AddModelError("", "Author doesn't exist");
            }

            // Check if country exists in the database
            if (_countryRepository.GetCountry(updatedAuthor.Country.Id) == null)
            {
                ModelState.AddModelError("", "Country doesn't exist");
            }

            // Check if an error was returned in ModelState
            if (!ModelState.IsValid)
            {
                // Return a Not Found page with the errors added
                return StatusCode(404, ModelState);
            }

            // Store the full Country Object in the updated Author object
            updatedAuthor.Country = _countryRepository.GetCountry(updatedAuthor.Country.Id);
            var author = _authorRepository.GetAuthor(authorId);
            author.FirstName = updatedAuthor.FirstName;
            author.LastName = updatedAuthor.LastName;
            author.Country = _countryRepository.GetCountry(updatedAuthor.Country.Id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // At this point, I can try updating the author to the database
            if (!_authorRepository.UpdateAuthor(author))
            {
                ModelState.AddModelError("", $"Something went wrong while updating {author.FirstName} {author.LastName}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        // api/authors/authorId
        [HttpDelete("{authorId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public IActionResult DeleteAuthor(int authorId)
        {
            // Check if author exists
            if (!_authorRepository.AuthorExists(authorId))
            {
                ModelState.AddModelError("", "Author doesn't exist");
                return StatusCode(404, ModelState);
            }

            // Get the author object from the database
            var authorToDelete = _authorRepository.GetAuthor(authorId);

            // If a book belongs to that author, don't delete the author!
            if (_authorRepository.GetBooksByAuthor(authorId).Count() > 0)
            {
                ModelState.AddModelError("", $"Author {authorToDelete.FirstName} {authorToDelete.LastName} " +
                                            "cannot be deleted because at least one book belongs to it");
                return StatusCode(409, ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Try deleting the author
            if (!_authorRepository.DeleteAuthor(authorToDelete))
            {
                ModelState.AddModelError("", $"Something went wrong while deleting {authorToDelete.FirstName} {authorToDelete.LastName}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}
