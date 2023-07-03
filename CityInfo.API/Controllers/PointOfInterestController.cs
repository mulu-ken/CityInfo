using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointOfInterestController : ControllerBase
    {
       private readonly ILogger _logger;
       private readonly ILocalMailService _mailService;
       private readonly CitiesDataStore _dataStore;

        public PointOfInterestController(ILogger<PointOfInterestController> logger, ILocalMailService localMail, CitiesDataStore dataStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _mailService = localMail ?? throw new ArgumentNullException(nameof(localMail)); 

            _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));   

        }

        [HttpGet]
        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int CityId)
        {
            var city = _dataStore.Cities.FirstOrDefault(city => city.Id == CityId);

            if (city == null)
            {
                return NotFound();
            }

            return Ok(city.PointsOfInterest);

        }

        [HttpGet("{pointofinterestId}", Name = "GetPointOfInterest")]
        public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointofinterestId)
        {
            try
            {                

                var city = _dataStore.Cities.FirstOrDefault(city => city.Id == cityId);

                if (city == null)
                {
                    return NotFound();
                }

                var pointOfInterest = city.PointsOfInterest.FirstOrDefault(point => point.Id == pointofinterestId);

                if (pointOfInterest == null)
                {
                    _logger.LogInformation($"The current', {pointofinterestId} doesn't exist");
                    return NotFound();
                }

                return Ok(pointOfInterest);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting point of interest", ex);
                return StatusCode(500, "A problem happened while handling your request");
            }         

            
        }


        [HttpPost]
        public ActionResult<PointOfInterestDto> CreatePointOfInterest(int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            var city = _dataStore.Cities.FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            //need to change later 
            var maxPointOfInterestId = _dataStore.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);


            var finalPointOfInterest = new PointOfInterestDto()
            {
                Id = ++maxPointOfInterestId,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description,
            };

            city.PointsOfInterest.Add(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest", new { cityId = cityId, pointofinterestId = finalPointOfInterest.Id },
            finalPointOfInterest);

        }


        [HttpPut("{pointOfInterestId}")]
        public ActionResult<PointOfInterestDto> UpdatePointOfInterest(int cityId, int pointOfInterestId, [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        {
            var thisCity = _dataStore.Cities.FirstOrDefault(c => c.Id == cityId);

            if (thisCity == null) { return NotFound(); }

            var thisPointOfInterest = thisCity.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);

            if (thisPointOfInterest == null) { return NotFound(nameof(thisPointOfInterest)); }

            thisPointOfInterest.Description = pointOfInterest.Description;
            thisPointOfInterest.Name = pointOfInterest.Name;

            return (NoContent());
        }

        [HttpPatch("{pointOfInterestId}")]

        public ActionResult<PointOfInterestDto>PartiallyUPdatePointOfInterestDto(int cityId, int pointOfInterestId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            var thisCity = _dataStore.Cities.FirstOrDefault( c => c.Id == cityId); 
            
            if (thisCity == null) { return NotFound(); }

            var thispointOfInterstFromStore = thisCity.PointsOfInterest.FirstOrDefault(p => p.Id== pointOfInterestId); 

            if (thispointOfInterstFromStore == null)
            {
                return NotFound(nameof(thispointOfInterstFromStore));
            }

            var pointOfInterestToPatch = new PointOfInterestForUpdateDto
            {
                Name = thispointOfInterstFromStore.Name,
                Description = thispointOfInterstFromStore.Description
            };

            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TryValidateModel(pointOfInterestToPatch))
            {
                return BadRequest(ModelState);
            }

            thispointOfInterstFromStore.Name = pointOfInterestToPatch.Name;
            thispointOfInterstFromStore.Description = pointOfInterestToPatch.Description;
            
            return (NoContent());

        }

        [HttpDelete("{pointOfInterestId}")]
        public ActionResult DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            var thisCity = _dataStore.Cities.FirstOrDefault(c => c.Id == cityId);

            if (thisCity == null) { return NotFound(); }

            var thispointOfInterstFromStore = thisCity.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);

            if (thispointOfInterstFromStore == null)
            {
                return NotFound();
            }

           
            thisCity.PointsOfInterest.Remove(thispointOfInterstFromStore);

            _mailService.Send("Point of interest deleted.", $"Point of interest {thispointOfInterstFromStore.Name} with id {thispointOfInterstFromStore.Id} was deleted");

            return NoContent();

        }


    }
}
