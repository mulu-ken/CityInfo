using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;


namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    [Authorize(Policy = "MustBeFromSeattle")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;
        private const int MaxCitiesPageSize = 20;


        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities([FromQuery]string? name, string? searchQuery,int pageNumber = 1, int pageSize = 10 )
        {
            if (pageSize > MaxCitiesPageSize)
            {
                pageSize = MaxCitiesPageSize;
            }

            var (allCities, paginationMetadata) = await _cityInfoRepository.GetCitiesAsync(name, searchQuery, pageNumber, pageSize);

            Response.Headers.Add("X-pagination", JsonSerializer.Serialize(paginationMetadata));
            

            return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(allCities));
        } 


      [HttpGet("{id}")]
        public async Task<ActionResult<CityDto>> GetCity(
            int id, bool includePointsOfInterest = false)
        {
            var city = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);
            if (city == null)
            {
                return NotFound();
            }

            if (includePointsOfInterest)
            {
                return Ok(_mapper.Map<CityDto>(city));
            }

            return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(city));
        }
    }

}