using CityInfo.API.Entities;

namespace CityInfo.API.Services
{
    public interface ICityInfoRepository
    {
        Task<IEnumerable<City>> GetCitiesAsync();
        Task<IEnumerable<City>> GetCitiesAsync(string? name, string? searchQuery, int pageNumber, int pageSize);
        Task<City?> GetCityAsync(int cityId, bool includePointOfInterest);

        Task<bool> CityExistsAsync(int cityId);
        Task<IEnumerable<PointOfInterest>> GetPointsOfInterestAsync(int cityId);    
        Task<PointOfInterest?> GetPointsOfInterestForCityAsync(int cityId, int pointOfInterestId);   

        Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest);

        Task<bool> SaveChangesAsync();

        void DeletePointOfInterest(PointOfInterest pointOfInterest);    

    }
}

