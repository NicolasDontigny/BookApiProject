using BookApiProject.Models;
using BookApiProject.Dtos;
using BookApiProject.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : Controller
    {
        private ICategoryRepository _categoryRepository;
        private IBookRepository _bookRepository;

        public CategoriesController(ICategoryRepository categoryRepository, IBookRepository bookRepository)
        {
            _categoryRepository = categoryRepository;
            _bookRepository = bookRepository;
        }

        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CategoryDto>))]
        public IActionResult GetCategories()
        {
            var categories = _categoryRepository.GetCategories().ToList();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoriesDto = new List<CategoryDto>();
            foreach(var category in categories)
            {
                categoriesDto.Add(new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name
                });
            }

            return Ok(categoriesDto);
        }

        // api/categories/categoryId
        [HttpGet("{categoryId}", Name = "GetCategory")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(CategoryDto))]
        public IActionResult GetCategory(int categoryId)
        {
            if (!_categoryRepository.CategoryExists(categoryId))
            {
                return NotFound();
            }

            var category = _categoryRepository.GetCategory(categoryId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };

            return Ok(categoryDto);
        }

        // api/categories/books/bookId
        // Get all categories for a book
        [HttpGet("books/{bookId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CategoryDto>))]
        public IActionResult GetCategoriesOfABook(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
            {
                return NotFound();
            }

            var categories = _categoryRepository.GetCategoriesOfABook(bookId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoriesDto = new List<CategoryDto>();
            foreach (var category in categories)
            {
                categoriesDto.Add(new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name
                });
            }

            return Ok(categoriesDto);
        }

        // api/categories/categoryId/books
        // Get all books for a category
        [HttpGet("{categoryId}/books")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<BookDto>))]
        public IActionResult GetBooksForCategory(int categoryId)
        {
            if (!_categoryRepository.CategoryExists(categoryId))
            {
               return NotFound();
            }

            var books = _categoryRepository.GetBooksForCategory(categoryId);

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

        // api/categories
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        [ProducesResponseType(201, Type = typeof(Category))]
        public IActionResult CreateCategory([FromBody]Category newCategory)
        {
            if (newCategory == null)
            {
                return BadRequest(ModelState);
            }

            // Check if another Category with the same name exists
            var category = _categoryRepository.GetCategories().Where(c => c.Name.Trim().ToUpper() == newCategory.Name.Trim().ToUpper()).FirstOrDefault();
            if (category != null)
            {
                ModelState.AddModelError("", $"Category {category.Name} already exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Try to create the country and add it to the database
            if (!_categoryRepository.CreateCategory(newCategory))
            {
                // if it wasn't successful, return an error
                ModelState.AddModelError("", $"Something went wrong while creating {newCategory.Name}");
                return StatusCode(500, ModelState);
            }

            // If we have reached this point, the category has been successfully added to the database
            return CreatedAtRoute("GetCategory", new { categoryId = newCategory.Id }, newCategory);
        }

        // api/categories/categoryId
        [HttpPut("{categoryId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        public IActionResult UpdateCategory(int categoryId, [FromBody]Category categoryToUpdate)
        {
            if (categoryToUpdate == null)
            {
                return BadRequest(ModelState);
            }

            // Validate that the categoryToUpdate from the body has the same ID as the categoryId in the URI
            if (categoryId != categoryToUpdate.Id)
            {
                return BadRequest(ModelState);
            }

            // Validate that the category to update actually exists in the database
            if (!_categoryRepository.CategoryExists(categoryToUpdate.Id))
            {
                return NotFound();
            }

            // Check if another Category with the same name exists
            if (_categoryRepository.IsDuplicateCategoryName(categoryId, categoryToUpdate.Name))
            {
                ModelState.AddModelError("", $"Category {categoryToUpdate.Name} already exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Try to update the country in the database
            if (!_categoryRepository.UpdateCategory(categoryToUpdate))
            {
                // if it wasn't successful, return an error
                ModelState.AddModelError("", $"Something went wrong while updating {categoryToUpdate.Name}");
                return StatusCode(500, ModelState);
            }

            // If we have reached this point, the category has been successfully updated to the database
            return NoContent();
        }

        // api/categories/categoryId
        [HttpDelete("{categoryId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        public IActionResult DeleteCategory(int categoryId)
        {
            // Validate that the category to delete actually exists in the database
            if (!_categoryRepository.CategoryExists(categoryId))
            {
                return NotFound();
            }

            var categoryToDelete = _categoryRepository.GetCategory(categoryId);

            // Check if a Book belongs to that category; if so, we cannot delete the category
            if (_categoryRepository.GetBooksForCategory(categoryId).Count() > 0)
            {
                ModelState.AddModelError("", $"Category {categoryToDelete.Name} " +
                                               "cannot be deleted because it is used by at least one book");
                return StatusCode(409, ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Try to delete the country in the database
            if (!_categoryRepository.DeleteCategory(categoryToDelete))
            {
                // if it wasn't successful, return an error
                ModelState.AddModelError("", $"Something went wrong while deleting {categoryToDelete.Name}");
                return StatusCode(500, ModelState);
            }

            // If we have reached this point, the category has been successfully updated to the database
            return NoContent();
        }
    }
}
