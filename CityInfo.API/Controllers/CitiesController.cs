using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController:ControllerBase
    {
        private readonly CitiesDataStore _dataStore;

        public CitiesController(CitiesDataStore dataStore)
        {
            _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        }

        [HttpGet]
        public ActionResult<CityDto> GetCities()
        {
            var allCities = _dataStore.Cities;

            return Ok(allCities);
        }


        [HttpGet("{id}")]
        public ActionResult<CityDto> GetCity(int id) {

            var filteredCity = _dataStore.Cities.FirstOrDefault(city => city.Id == id);

            if (filteredCity == null) {
            return NotFound();
            }

            return Ok(filteredCity);
        }
    }
}
