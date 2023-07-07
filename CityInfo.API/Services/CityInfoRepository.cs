using System.Collections.Immutable;
using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Services
{
    public class CityInfoRepository:ICityInfoRepository
    {
        private readonly CityInfoContext _context;

        public CityInfoRepository(CityInfoContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); 
        }
        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
        
            return await _context.Cities.OrderBy(c => c.Name).ToListAsync();
        }


        public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? name, string? searchQuery, int pageNumber, int pageSize)
        {
            var query = _context.Cities.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                query = query.Where(c =>
                    c.Name.Contains(searchQuery) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.Contains(searchQuery)));
                
            }

            if (!string.IsNullOrEmpty(name))
            {
                name = name.Trim();
                query = query.Where(c => c.Name == name);

            }

            var totalItemCount = await query.CountAsync();

            var paginationMetadata = new PaginationMetadata(totalItemCount, pageSize, pageNumber);

            var collectionToReutn =  await query.OrderBy(c => c.Name).Skip(pageSize* (pageNumber - 1)).Take(pageSize).ToListAsync();

            return (collectionToReutn, paginationMetadata);
        }

        public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
        {

            if (includePointsOfInterest)
            {
                return await _context.Cities.Include(c => c.PointOfInterest).
                    Where(c => c.Id == cityId).FirstOrDefaultAsync();
            }
            return await _context.Cities.Where(c => c.Id == cityId).FirstOrDefaultAsync();
        }


        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestAsync(int cityId)
        {
            return await _context.PointsOfInterest.Where(p => p.CityId == cityId).ToListAsync();
        }


        public async Task<bool> CityExistsAsync(int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.Id == cityId);
        }

        public async Task<PointOfInterest?> GetPointsOfInterestForCityAsync(int cityId, int pointOfInterestId)
        {
            return await _context.PointsOfInterest
                .Where(p => p.CityId == cityId && p.Id == pointOfInterestId).FirstOrDefaultAsync();
        }


        public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId, false);
            if (city != null)
            {
                city.PointOfInterest.Add(pointOfInterest);
            }


        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            _context.PointsOfInterest.Remove(pointOfInterest);
        }

        public async Task<bool> CityNameMathcesCityId(string? cityName, int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.Id ==cityId && c.Name == cityName);
        }
    }

    
}
