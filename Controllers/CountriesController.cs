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
    // The route will match the name of the controller, even if it is changed
    // This is the namespace of every countries route (every route will start with api/countries")
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : Controller
    {
        private ICountryRepository _countryRepository;
        private IAuthorRepository _authorRepository;

        // Instantiate the _countryRepository variable in the constructor
        public CountriesController(ICountryRepository countryRepository, IAuthorRepository authorRepository)
        {
            _countryRepository = countryRepository;
            _authorRepository = authorRepository;
        }

        // Specify the Request to call that method. Then we need to create a route to this action (URI //api/countries)
        [HttpGet]
        // Specifi the only Response Types I want returned, with the type returned (not necessary, but good for debugging and documentation
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CountryDto>))]
        public IActionResult GetCountries()
        {
            var countries = _countryRepository.GetCountries().ToList();

            // Check if Model State is valid or invalid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var countriesDto = new List<CountryDto>();
            foreach(var country in countries)
            {
                // for each country object, we create a new Dto with only its Id and Name, because we don't want to display authors in GetCountries
                countriesDto.Add(new CountryDto
                {
                    Id = country.Id,
                    Name = country.Name
                });
            }

            // Return status code with countries
            return Ok(countriesDto);
        }

        // Specify the Request to call that method. Then we need to create a route to this action (URI //api/countries)
        // api/countries/countryId
        // Give the Route a name, so I can refer to it later to redirect
        [HttpGet("{countryId}", Name = "GetCountry")] // adds this path to the Route Namespace defined at the top
        // Specifi the only Response Types I want returned, with the type returned (not necessary, but good for debugging and documentation
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CountryDto>))]
        public IActionResult GetCountry(int countryId)
        {
            // Return Not Found Page if the country doesn't exist in the Database
            if (!_countryRepository.CountryExists(countryId))
            {
                return NotFound();
            }

            var country = _countryRepository.GetCountry(countryId);

            // Check if Model State is valid or invalid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var countryDto = new CountryDto()
            {
                Id = country.Id,
                Name = country.Name
            };

            // Return status code with countries
            return Ok(countryDto);
        }

        // Specify the Request to call that method. Then we need to create a route to this action (URI //api/countries)
        // api/countries/authors/authorId
        [HttpGet("authors/{authorId}")] // adds this path to the Route Namespace defined at the top
        // Specifi the only Response Types I want returned, with the type returned (not necessary, but good for debugging and documentation
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CountryDto>))]
        public IActionResult GetCountryOfAnAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var country = _countryRepository.GetCountryOfAnAuthor(authorId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var countryDto = new CountryDto()
            {
                Id = country.Id,
                Name = country.Name
            };

            // Return status code with countries
            return Ok(countryDto);
        }


        // TODO implement GetAuthorsOfACountry
        // api/countries/countryId/authors
        [HttpGet("{countryid}/authors")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<AuthorDto>))]
        [ProducesResponseType(400)]
        public IActionResult GetAuthorsFromACountry(int countryId)
        {
            if (!_countryRepository.CountryExists(countryId))
                return NotFound();

            var authors = _countryRepository.GetAuthorsFromACountry(countryId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

        // api/countries

        [HttpPost]
        // If it is created successfully, the return type will be the Country, because we redirect to that Route
        [ProducesResponseType(201, Type = typeof(Country))]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        // The Country object will come from the "body" of the post request
        public IActionResult CreateCountry([FromBody]Country countryToCreate)
        {
            // Id will be automatically added by the Database

            // Check if country is null
            if (countryToCreate == null)
            {
                return BadRequest(ModelState);
            }

            // Check if another country with that same name already exists (Duplicate Name)
            var country = _countryRepository.GetCountries()
                            .Where(c => c.Name.Trim().ToUpper() == countryToCreate.Name.Trim().ToUpper()).FirstOrDefault();

            // If it does,
            if (country != null)
            {
                // return a custom error message after adding the found country to the ModelState
                ModelState.AddModelError("", $"Country {country.Name} already exists");
                // return an Unprocessable Entity
                // ModelState is the response I see in Postman when the Status returns 422. It could be whatever I want
                // Returning ModelState is good for Debugging
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the country creation in the Database was successfull
            if (!_countryRepository.CreateCountry(countryToCreate))
            {
                // If the function returns null, add another error
                ModelState.AddModelError("", $"Something went wrong saving {countryToCreate.Name}");
                return StatusCode(500, ModelState);
            }

            // If we reach that point, the country was successfully created, so we can redirect to that new country's Route
            // Pass in the countryId as a parameter to the Route (country is now saved in the database so has an id)
            // Pass in the actual country object as well
            return CreatedAtRoute("GetCountry", new { countryId = countryToCreate.Id }, countryToCreate);
        }

        // api/countries/countryId
        [HttpPut("{countryId}")]
        [ProducesResponseType(204)] // no content, so no type
        [ProducesResponseType(409)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        // The Country object will come from the "body" of the post request
        // Use the countryId passed in the URI to check if the user is trying to update the correct country
        public IActionResult UpdateCountry(int countryId, [FromBody]Country updatedCountryInfo)
        {
            if (updatedCountryInfo == null)
            {
                return BadRequest(ModelState);
            }

            // Validate that the country passed in the body is the same as the one in the URI params
            if (countryId != updatedCountryInfo.Id)
            {
                return BadRequest(ModelState);
            }

            // Make sure that the country that the user wants to update actually exists in the database
            if (!_countryRepository.CountryExists(countryId))
            {
                return NotFound();
            }

            // Validate that there is not another country with that new updated name
            if (_countryRepository.IsDuplicateCountryName(countryId, updatedCountryInfo.Name))
            {
                ModelState.AddModelError("", $"Country {updatedCountryInfo.Name} already exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // If we reach that point, we can try updating the Country
            if (!_countryRepository.UpdateCountry(updatedCountryInfo))
            {
                ModelState.AddModelError("", $"Something went wrong updating {updatedCountryInfo.Name}");
                return StatusCode(500, ModelState);
            }

            // If we reach that point, the country was successfully updated
            // When updating, we generally don't return anything because we did not create anything new

            return NoContent();
        }

        // api/countries/countryId
        [HttpDelete("{countryId}")]
        [ProducesResponseType(204)] // no content, so no type
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        // We don't need to pass in a country object, because we are not sending any country Info, we just want to grab the country and delete it
        public IActionResult DeleteCountry(int countryId)
        {
            // Make sure that the country that the user wants to delete actually exists in the database
            if (!_countryRepository.CountryExists(countryId))
            {
                return NotFound();
            }

            var countryToDelete = _countryRepository.GetCountry(countryId);

            // Verify if an author is associated to that country, then we don't want to delete the country
            if (_countryRepository.GetAuthorsFromACountry(countryId).Count > 0)
            {
                ModelState.AddModelError("", $"Country {countryToDelete.Name} " + 
                                               "cannot be deleted because it is used by at least one author");
                // Return a 409, which is Conflict because of the foreign Key
                return StatusCode(409, ModelState); 
            }
            

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // If we reach that point, we can try deleting the Country
            if (!_countryRepository.DeleteCountry(countryToDelete))
            {
                // If there is somehow an error, return an 500 error
                ModelState.AddModelError("", $"Something went wrong deleting {countryToDelete.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
            
        }
    }
}
