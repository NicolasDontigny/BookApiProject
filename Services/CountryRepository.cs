using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApiProject.Models;

namespace BookApiProject.Services
{
    public class CountryRepository : ICountryRepository
    {
        // Create a variable of type BookDbContext, called _countryContext (appended with _ since it is a private variable)
        private BookDbContext _countryContext;

        // Initialize the variable in the constructor
        public CountryRepository(BookDbContext countryContext)
        {
            _countryContext = countryContext;
        }
        public ICollection<Author> GetAuthorsFromACountry(int countryId)
        {
            return _countryContext.Authors.Where(c => c.Country.Id == countryId).ToList();
        }

        public ICollection<Country> GetCountries()
        {
            // Call the "Countries" dbset from BookDbContext.cs, sorted alphabetically by country name
            return _countryContext.Countries.OrderBy(c => c.Name).ToList(); // Convert it from a Queryable Object to a List (matching the Collection return type)
        }

        public Country GetCountry(int countryId)
        {
            // Filter the country that matches the countryId
            return _countryContext.Countries.Where(c => c.Id == countryId).FirstOrDefault(); // Returns either the first country that matches the id, or the default NULL
        }

        public Country GetCountryOfAnAuthor(int authorId)
        {
            // Get the matching author, but select his country to return it
            return _countryContext.Authors.Where(a => a.Id == authorId).Select(c => c.Country).FirstOrDefault();
        }

        public bool CountryExists(int countryId)
        {
            // Check if there is a country with that ID in the database. Any returns true or false
            return _countryContext.Countries.Any(c => c.Id == countryId);
        }

        public bool IsDuplicateCountryName(int countryId, string countryName)
        {
            var country = _countryContext.Countries.Where(c => c.Id != countryId && c.Name.Trim().ToUpper() == countryName.Trim().ToUpper()).FirstOrDefault();

            return country != null;
        }

        public bool CreateCountry(Country country)
        {
            _countryContext.Add(country);
            return Save();
        }

        public bool UpdateCountry(Country country)
        {
            _countryContext.Update(country);
            return Save();
        }

        public bool DeleteCountry(Country country)
        {
            _countryContext.Remove(country);
            // return the boolean from the Save method to check if the changes were successful
            return Save();
        }

        public bool Save()
        {
            // SaveChanges method returns an integer, which is the number of changes that occurred. If it is a negative number, something went wrong
            // If it is 0, then no changes happened, but it is not an error
            var saved = _countryContext.SaveChanges();

            return saved >= 0 ? true : false;
        }
    }
}
