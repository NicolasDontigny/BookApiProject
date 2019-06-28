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
    public class ReviewersController : Controller
    {
        private IReviewerRepository _reviewerRepository;
        private IReviewRepository _reviewRepository;

        public ReviewersController(IReviewerRepository reviewerRepository, IReviewRepository reviewRepository)
        {
            _reviewerRepository = reviewerRepository;
            _reviewRepository = reviewRepository;
        }

        // api/reviewers
        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewerDto>))]
        public IActionResult GetReviewers()
        {
            var reviewers = _reviewerRepository.GetReviewers();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reviewersDto = new List<ReviewerDto>();
            foreach (var review in reviewers)
            {
                reviewersDto.Add(new ReviewerDto
                {
                    Id = review.Id,
                    FirstName = review.FirstName,
                    LastName = review.LastName
                });
            }

            return Ok(reviewersDto);
        }

        // api/reviewers/reviewerId
        [HttpGet("{reviewerId}", Name = "GetReviewer")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(ReviewerDto))]
        public IActionResult GetReviewer(int reviewerId)
        {
            if (!_reviewerRepository.ReviewerExists(reviewerId))
            {
                return NotFound();
            }

            var reviewer = _reviewerRepository.GetReviewer(reviewerId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reviewerDto = new ReviewerDto
            {
                Id = reviewer.Id,
                FirstName = reviewer.FirstName,
                LastName = reviewer.LastName,
            };

            return Ok(reviewerDto);
        }

        // api/reviewers/reviewId/reviewer
        [HttpGet("{reviewId}/reviewer")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(ReviewerDto))]
        public IActionResult GetReviewerOfAReview(int reviewId)
        {
            if (!_reviewRepository.ReviewExists(reviewId))
            {
                return NotFound();
            }

            var reviewer = _reviewerRepository.GetReviewerOfAReview(reviewId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reviewerDto = new ReviewerDto
            {
                Id = reviewer.Id,
                FirstName = reviewer.FirstName,
                LastName = reviewer.LastName,
            };

            return Ok(reviewerDto);
        }

        // api/reviewers/reviewerId/reviews
        [HttpGet("{reviewerId}/reviews")]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDto>))]
        public IActionResult GetReviewsByReviewer(int reviewerId)
        {
            if (!_reviewerRepository.ReviewerExists(reviewerId))
            {
                return NotFound();
            }

            var reviews = _reviewerRepository.GetReviewsByReviewer(reviewerId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reviewsDtos = new List<ReviewDto>();
            foreach(var review in reviews)
            {
                reviewsDtos.Add(new ReviewDto
                {
                    Id = review.Id,
                    Headline = review.Headline,
                    ReviewText = review.ReviewText,
                    Rating = review.Rating
                });
            }

            

            return Ok(reviewsDtos);
        }

        // api/reviewers
        [HttpPost]
        // If it is created successfully, the return type will be the Reviewer, because we redirect to that Route
        [ProducesResponseType(201, Type = typeof(Reviewer))]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        // The Reviewer object will come from the "body" of the post request
        public IActionResult CreateReviewer([FromBody]Reviewer newReviewer)
        {
            // Id will be automatically added by the Database

            // Check if reviewer is null
            if (newReviewer == null)
            {
                return BadRequest(ModelState);
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the country creation in the Database was successfull
            if (!_reviewerRepository.CreateReviewer(newReviewer))
            {
                // If the function returns null, add another error
                ModelState.AddModelError("", $"Something went wrong saving {newReviewer.FirstName} {newReviewer.LastName}");
                return StatusCode(500, ModelState);
            }

            // If we reach that point, the reviewer was successfully created, so we can redirect to that new reviewer's Route
            // Pass in the reviewerId as a parameter to the Route (reviewer is now saved in the database so has an id)
            // Pass in the actual reviewer object as well
            return CreatedAtRoute("GetReviewer", new { reviewerId = newReviewer.Id }, newReviewer);
        }

        // api/reviewers/reviewerId
        [HttpPut("{reviewerId}")]
        [ProducesResponseType(204)] // no content, so no type
        [ProducesResponseType(409)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        // The Reviewer object will come from the "body" of the post request
        // Use the reviewerId passed in the URI to check if the user is trying to update the correct reviewer
        public IActionResult UpdateReviewer(int reviewerId, [FromBody]Reviewer updatedReviewer)
        {
            if (updatedReviewer == null)
            {
                return BadRequest(ModelState);
            }

            // Validate that the country passed in the body is the same as the one in the URI params
            if (reviewerId != updatedReviewer.Id)
            {
                return BadRequest(ModelState);
            }

            // Make sure that the country that the user wants to update actually exists in the database
            if (!_reviewerRepository.ReviewerExists(reviewerId))
            {
                return NotFound();
            }
                       
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // If we reach that point, we can try updating the Reviewer
            if (!_reviewerRepository.UpdateReviewer(updatedReviewer))
            {
                ModelState.AddModelError("", $"Something went wrong updating {updatedReviewer.FirstName} {updatedReviewer.LastName}");
                return StatusCode(500, ModelState);
            }

            // If we reach that point, the reviewer was successfully updated
            // When updating, we generally don't return anything because we did not create anything new
            return NoContent();
        }

        // api/reviewers/reviewerId
        [HttpDelete("{reviewerId}")]
        [ProducesResponseType(204)] // no content, so no type
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]

        public IActionResult DeleteReviewer(int reviewerId)
        {
            // Make sure that the reviewer that the user wants to delete actually exists in the database
            if (!_reviewerRepository.ReviewerExists(reviewerId))
            {
                return NotFound();
            }
            
            var reviewerToDelete = _reviewerRepository.GetReviewer(reviewerId);
            var reviewsToDelete = _reviewerRepository.GetReviewsByReviewer(reviewerId).ToList();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // If we reach that point, we can try deleting the Reviewer
            if (!_reviewerRepository.DeleteReviewer(reviewerToDelete))
            {
                // If there is somehow an error, return an 500 error
                ModelState.AddModelError("", $"Something went wrong deleting {reviewerToDelete.FirstName} {reviewerToDelete.LastName}");
                return StatusCode(500, ModelState);
            }

            // Now that we have successfully deleted the Reviewer, we want to delete all of his reviews
            if (!_reviewRepository.DeleteReviews(reviewsToDelete))
            {
                // If there is somehow an error, return an 500 error
                ModelState.AddModelError("", $"Something went wrong deleting the reviews created by {reviewerToDelete.FirstName} {reviewerToDelete.LastName}");
                return StatusCode(500, ModelState);
            }

            return NoContent();

        }
    }
}
