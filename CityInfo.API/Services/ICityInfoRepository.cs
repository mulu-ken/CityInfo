using CityInfo.API.Entities;

namespace CityInfo.API.Services
{
    public interface ICityInfoRepository
    {
        Task<IEnumerable<City>> GetCitiesAsync();
        Task<City>? GetCityAsync(int cityId, bool includePointOfInterest);

        Task<IEnumerable<PointOfInterest>> GetPointsOfInterestAsync(int cityId);    
        Task<PointOfInterest> GetPointsOfInterestForCityAsync(int cityId, int pointOfInterestId);    

    }
}
