
using AssetsManagementSystem.DTOs.LocationDTOs;
using AssetsManagementSystem.Models.DbSets;

namespace AssetsManagementSystem.Services.Locations
{
    public class LocationService : BaseClassForServices
    {
        public LocationService(IUnitOfWork unitOfWork, 
            Others.Interfaces.IAutoMapper.IMapper mapper,
            IHttpContextAccessor httpContextAccessor) 
            : base(unitOfWork, mapper, httpContextAccessor)
        {

        }

        #region Adding a new location
        public async Task AddLocationAsync(AddLocationRequestDTO addLocationRequest)
        {
            if (addLocationRequest == null)
            {
                throw new ArgumentNullException(nameof(addLocationRequest), "Location details cannot be null.");
            }

            var existingLocation = await UnitOfWork.readRepository<Location>()
                                       .GetAsync(l =>  l.Barcode==addLocationRequest.Barcode);//l.Name == addLocationRequest.Name ||

            if (existingLocation != null)
            {
                throw new InvalidOperationException("A location with the same name already exists.");
            }

            var location = Mapper.Map<Location,AddLocationRequestDTO>(addLocationRequest);

            location.AddedOnDate = DateTime.Now;

            await UnitOfWork.writeRepository<Location>().AddAsync(location);

            await UnitOfWork.SaveChangeAsync();
        }
        #endregion

        #region Retrieve a location by ID
        public async Task<GetLocationRequestDTO> GetLocationByIdAsync(string locationBarcode)
        {
            if (string.IsNullOrEmpty(locationBarcode))
            {
                throw new ArgumentException("Invalid location Barcode.");
            }

            var location = await UnitOfWork.readRepository<Location>()
                .GetAsync(l => l.Barcode == locationBarcode && (l.IsDeleted == false || l.IsDeleted == null));
            var getLocationRequestDTO = Mapper.Map<GetLocationRequestDTO,Location>(location);

            if (location == null)
            {
                throw new KeyNotFoundException("Location not found.");
            }

            return getLocationRequestDTO;
        }
        #endregion

        #region Retrieve all locations
        public async Task<IEnumerable<GetLocationRequestDTO>> GetAllLocationsAsync()
        {
            var Locations = await UnitOfWork.readRepository<Location>()
               .GetAllAsync(predicate: l=> (l.IsDeleted == false || l.IsDeleted == null));

            var GetLocationRequestDTOs = Mapper.Map<GetLocationRequestDTO,Location>(Locations);

            return GetLocationRequestDTOs;
        }
        #endregion


        #region Retrieve all locations
        public async Task<IEnumerable<GetLocationRequestDTO>> GetAllByPaginationLocationsAsync(int currentPage = 1, int pageSize = 10)
        {
            var Locations = await UnitOfWork.readRepository<Location>()
               .GetAllByPagningAsync(predicate: l => (l.IsDeleted == false || l.IsDeleted == null), pageSize: pageSize, currentPage: currentPage);

            var GetLocationRequestDTOs = Mapper.Map<GetLocationRequestDTO, Location>(Locations);

            return GetLocationRequestDTOs;
        }
        #endregion



        #region Update a location
        public async Task UpdateLocationAsync(string locationBarcode, UpdateLocationRequestDTO updateLocationRequest)
        {
            if (updateLocationRequest == null)
            {
                throw new ArgumentNullException(nameof(updateLocationRequest), "Location details cannot be null.");
            }
            var updateLocation = await GetLocationByIdAsync(locationBarcode);

            var location = Mapper.Map<Location,GetLocationRequestDTO>(updateLocation);

            var existingLocation = await UnitOfWork.readRepository<Location>()
                                      .GetAsync(l => l.Name == updateLocationRequest.Name && l.Barcode != locationBarcode 
                                      && (l.IsDeleted == false || l.IsDeleted == null));

            if (existingLocation != null)
            {
                throw new InvalidOperationException("Another location with the same name already exists.");
            }

            location.Name = updateLocationRequest.Name;
            location.Address = updateLocationRequest.Address;
            location.UpdatedDate= DateTime.Now;
            location.AddedOnDate = location.AddedOnDate;
            await UnitOfWork.writeRepository<Location>().UpdateAsync(location.Id, location);

            await UnitOfWork.SaveChangeAsync();
        }
        #endregion

        #region Delete a location
        public async Task DeleteLocationAsync(string locationBarcode)
        {
            var updateLocation = await GetLocationByIdAsync(locationBarcode);

            var asset = await UnitOfWork.readRepository<Asset>()
              .GetAsync(predicate: a => a.LocationId == updateLocation.Id && (a.IsDeleted == false || a.IsDeleted == null));
          
            if (asset is not null)
                throw new InvalidOperationException("There are suppliers dependent on this supplier,Please Go and delete it first");
           
            var location = Mapper.Map<Location, GetLocationRequestDTO>(updateLocation);

            location.DeletedDate = DateTime.Now;
            location.IsDeleted = true;
            
            await UnitOfWork.writeRepository<Location>().UpdateAsync(location.Id, location);

            await UnitOfWork.SaveChangeAsync();
        }

        #endregion
         
    }
}
