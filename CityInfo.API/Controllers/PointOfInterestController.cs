using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;


namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/cities/{cityId}/pointsofinterest")]
    [ApiVersion("2.0")]
    [Authorize(Policy = "MustBeFromSeattle")]

    public class PointOfInterestController : ControllerBase
    {
       private readonly ILogger _logger;
       private readonly ILocalMailService _mailService;
       private readonly ICityInfoRepository _cityInfoRepository;
       private readonly IMapper _mappper;

       public PointOfInterestController(ILogger<PointOfInterestController> logger, ILocalMailService localMail, ICityInfoRepository cityInfoRepository, IMapper mappper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _mailService = localMail ?? throw new ArgumentNullException(nameof(localMail));
            
            _cityInfoRepository = cityInfoRepository ?? 
                                  throw new ArgumentNullException(nameof(cityInfoRepository));
            _mappper = mappper ?? throw new ArgumentNullException(nameof(mappper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
        {

            var cityName = User.Claims.FirstOrDefault(c => c.Type == "city")?.Value;

            //if (!await _cityInfoRepository.CityNameMathcesCityId(cityName, cityId))
            //{
            //    return Forbid();
            //}

            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound();
            }

            var allPointOfInterest = await _cityInfoRepository.GetPointsOfInterestAsync(cityId);

            return Ok(_mappper.Map<IEnumerable<PointOfInterestDto>>(allPointOfInterest));

        }

        [HttpGet("{pointofinterestId}", Name = "GetPointOfInterest")]
        public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int cityId, int pointofinterestId)
        {


            try
            {


                if (!await _cityInfoRepository.CityExistsAsync(cityId))
                {
                    _logger.LogInformation($"The city doesn't exist and hence point of interest can't be found");
                    return NotFound();
                }

                var pointOfInterest =
                    await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId, pointofinterestId);

                if (pointOfInterest == null)
                {
                    _logger.LogInformation($"The current', {pointofinterestId} doesn't exist");
                    return NotFound();
                }

                return Ok(_mappper.Map<PointOfInterestDto>(pointOfInterest));
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting point of interest", ex);
                return StatusCode(500, "A problem happened while handling your request");
            }


        }


        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
        

            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogInformation($"City with Id of {cityId} coudn't be found");
                return NotFound();
            }

            var finaPointOfInterest = _mappper.Map<Entities.PointOfInterest>(pointOfInterest);

            await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, finaPointOfInterest);

            await _cityInfoRepository.SaveChangesAsync();

            var createdPointOfInterestToReturn = _mappper.Map<PointOfInterestDto>(finaPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest",
                new { cityId = cityId, pointOfInterestId = createdPointOfInterestToReturn.Id },
                createdPointOfInterestToReturn);
        }


        [HttpPut("{pointOfInterestId}")]
        public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId, [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        {
     

            if (!await _cityInfoRepository.CityExistsAsync(cityId)) { return NotFound(); }

            var thisPointOfInterest = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId, pointOfInterestId);

            if (thisPointOfInterest is null)
            {
                return NotFound();
            }

            _mappper.Map(pointOfInterest,thisPointOfInterest);

            await _cityInfoRepository.SaveChangesAsync();

            return (NoContent());
        }

        [HttpPatch("{pointOfInterestId}")]

        public async Task<ActionResult> PartiallyUPdatePointOfInterestDto(int cityId, int pointOfInterestId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {


            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var thispointOfInterstFromStore = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId, pointOfInterestId);

            if (thispointOfInterstFromStore == null)
            {
                return NotFound(nameof(thispointOfInterstFromStore));
            }

            var pointOfInterstToPatch = _mappper.Map<PointOfInterestForUpdateDto>(thispointOfInterstFromStore);

            patchDocument.ApplyTo(pointOfInterstToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TryValidateModel(pointOfInterstToPatch))
            {
                return BadRequest(ModelState);
            }

            _mappper.Map(pointOfInterstToPatch, thispointOfInterstFromStore);
            await _cityInfoRepository.SaveChangesAsync();
            return (NoContent());

        }

        [HttpDelete("{pointOfInterestId}")]
        public async Task<ActionResult>DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId)) { return NotFound(); }

            var thispointOfInterstEntity =
               await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId, pointOfInterestId);

            if (thispointOfInterstEntity == null)
            {
                return NotFound();
            }

            _cityInfoRepository.DeletePointOfInterest(thispointOfInterstEntity);
            await _cityInfoRepository.SaveChangesAsync();

            _mailService.Send("Point of interest deleted.", $"Point of interest {thispointOfInterstEntity.Name} with id {thispointOfInterstEntity.Id} was deleted");

            return NoContent();

        }


    }
}
