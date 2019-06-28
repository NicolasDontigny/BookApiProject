using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookApiProject.Dtos
{
    // Data-transfer Objects
    public class CountryDto
    {
        // Select the Country properties we want to display on a certain page
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
