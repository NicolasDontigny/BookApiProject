using BookApiProject.Dtos;
using BookApiProject.Models;
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
    public class ReviewsController : Controller
    {
        private IReviewRepository _reviewRepository;
        private IReviewerRepository _reviewerRepository;
        private IBookRepository _bookRepository;

        public ReviewsController(IReviewRepository reviewRepository, IBookRepository bookRepository, IReviewerRepository reviewerRepository)
        {
            _reviewRepository = reviewRepository;
            _reviewerRepository = reviewerRepository;
            _bookRepository = bookRepository;
        }

        // api/reviews
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDto>))]
        [ProducesResponseType(400)]
        public IActionResult GetReviews()
        {
            var reviews = _reviewRepository.GetReviews();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reviewsDto = new List<ReviewDto>();

            foreach(var review in reviews)
            {
                reviewsDto.Add(new ReviewDto
                {
                    Id = review.Id,
                    Headline = review.Headline,
                    ReviewText = review.ReviewText,
                    Rating = review.Rating
                });
            }

            return Ok(reviewsDto);
        }

        // api/reviews/reviewId
        [HttpGet("{reviewId}", Name = "GetReview")]
        [ProducesResponseType(200, Type = typeof(ReviewDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetReview(int reviewId)
        {
            if (!_reviewRepository.ReviewExists(reviewId))
            {
                return NotFound();
            }

            var review = _reviewRepository.GetReview(reviewId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reviewDto = new ReviewDto
            {
                Id = review.Id,
                Headline = review.Headline,
                ReviewText = review.ReviewText,
                Rating = review.Rating
            };

            return Ok(reviewDto);
        }

        // api/reviews/books/bookId
        [HttpGet("books/{bookId}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDto>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetReviewsOfABook(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
            {
                return NotFound();
            }

            var reviews = _reviewRepository.GetReviewsOfABook(bookId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reviewsDto = new List<ReviewDto>();
            foreach(var review in reviews)
            {
                reviewsDto.Add(new ReviewDto
                {
                    Id = review.Id,
                    Headline = review.Headline,
                    ReviewText = review.ReviewText,
                    Rating = review.Rating
                });
            }
           
            return Ok(reviewsDto);
        }

        // api/reviews/reviewId/book
        [HttpGet("{reviewId}/book")]
        [ProducesResponseType(200, Type = typeof(BookDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetBookOfAReview(int reviewId)
        {
            if (!_reviewRepository.ReviewExists(reviewId))
            {
                return NotFound();
            }

            var book = _reviewRepository.GetBookOfAReview(reviewId);

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

        // api/reviews
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)] // Server Error
        [ProducesResponseType(201, Type = typeof(Review))]
        public IActionResult CreateReview([FromBody]Review newReview)
        {
            if (newReview == null)
            {
                return BadRequest(ModelState);
            }

            // Check if the reviewer associated to the new Review are valid, and for now I simply want to add an Error to ModelState
            if (!_reviewerRepository.ReviewerExists(newReview.Reviewer.Id))
            {
                ModelState.AddModelError("", "Reviewer doesn't exist");
            }

            // Check if the book associated to the new Review are valid, and for now I simply want to add an Error to ModelState
            if (!_bookRepository.BookExists(newReview.Book.Id))
            {
                ModelState.AddModelError("", "Book doesn't exist");
            }

            // Then, if the ModelState is not Valid after checking the book and review, return a 404
            if (!ModelState.IsValid)
            {
                return StatusCode(404, ModelState);
            }

            // In the Body Request, there will probably only be the book ID and the reviewer ID, not the whole objects
            // So, I need to retrieve them and store them in the newReview
            newReview.Book = _bookRepository.GetBook(newReview.Book.Id);
            newReview.Reviewer = _reviewerRepository.GetReviewer(newReview.Reviewer.Id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Try to create the review and add it to the database
            if (!_reviewRepository.CreateReview(newReview))
            {
                // if it wasn't successful, return an error
                ModelState.AddModelError("", $"Something went wrong while creating the review");
                return StatusCode(500, ModelState);
            }

            // If we have reached this point, the category has been successfully added to the database
            return CreatedAtRoute("GetReview", new { reviewId = newReview.Id }, newReview);
        }

        // api/reviews/reviewId
        [HttpPut("{reviewId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)] // Server Error
        public IActionResult UpdateReview(int reviewId, [FromBody]Review updatedReview)
        {
            if (updatedReview == null)
            {
                return BadRequest(ModelState);
            }

            // Check that the updated Review's Id matches the ID in the URI
            if (reviewId != updatedReview.Id)
            {
                // Other Way to send a Bad Request
                return StatusCode(400, ModelState);
            }

            // Check that the review actually exists in the database
            if (!_reviewRepository.ReviewExists(reviewId))
            {
                ModelState.AddModelError("", "Review doesn't exist");
            }

            // Check if the reviewer associated to the updated Review are valid, and for now I simply want to add an Error to ModelState
            if (!_reviewerRepository.ReviewerExists(updatedReview.Reviewer.Id))
            {
                ModelState.AddModelError("", "Reviewer doesn't exist");
            }

            // Check if the book associated to the updated Review are valid, and for now I simply want to add an Error to ModelState
            if (!_bookRepository.BookExists(updatedReview.Book.Id))
            {
                ModelState.AddModelError("", "Book doesn't exist");
            }

            // Then, if the ModelState is not Valid after checking the book and review, return a 404
            if (!ModelState.IsValid)
            {
                return StatusCode(404, ModelState);
            }

            // In the Body Request, there will probably only be the book ID and the reviewer ID, not the whole objects
            // So, I need to retrieve them and store them in the updatedReview
            updatedReview.Book = _bookRepository.GetBook(updatedReview.Book.Id);
            updatedReview.Reviewer = _reviewerRepository.GetReviewer(updatedReview.Reviewer.Id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Try to update the review and save it to the database
            if (!_reviewRepository.UpdateReview(updatedReview))
            {
                // if it wasn't successful, return an error
                ModelState.AddModelError("", $"Something went wrong while updating the review");
                return StatusCode(500, ModelState);
            }

            // If we have reached this point, the category has been successfully added to the database
            return NoContent();
        }

        // api/reviews/reviewId
        [HttpDelete("{reviewId}")]
        [ProducesResponseType(204)] // no content
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)] // Server Error
        public IActionResult DeleteReview(int reviewId)
        {
            // Check that the review actually exists in the database
            if (!_reviewRepository.ReviewExists(reviewId))
            {
                return NotFound();
            }

            var reviewToDelete = _reviewRepository.GetReview(reviewId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Try to delete the review from the database
            if (!_reviewRepository.DeleteReview(reviewToDelete))
            {
                // if it wasn't successful, return an error
                ModelState.AddModelError("", $"Something went wrong while deleting the review");
                return StatusCode(500, ModelState);
            }

            // If we have reached this point, the category has been successfully deleted from the database
            return NoContent();
        }
    }
}
