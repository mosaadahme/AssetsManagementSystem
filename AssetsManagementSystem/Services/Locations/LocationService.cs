using AssetsManagementSystem.DTOs.LocationDTOs;
using AssetsManagementSystem.Models.DbSets;
using Microsoft.EntityFrameworkCore;

namespace AssetsManagementSystem.Services.Locations
{
    public class LocationService : BaseClassForServices
    {
        public LocationService ( IUnitOfWork unitOfWork,
            Others.Interfaces.IAutoMapper.IMapper mapper,
            IHttpContextAccessor httpContextAccessor )
            : base ( unitOfWork, mapper, httpContextAccessor )
        {
        }

        #region Adding a new location
        public async Task AddLocationAsync ( AddLocationRequestDTO dto )
        {
            if ( dto == null ) throw new ArgumentNullException ( nameof ( dto ) );

            // Check Duplicate Barcode OR Name
            var existingLocation = await UnitOfWork.readRepository<Location> ( )
                 .GetAsync ( l => ( l.Barcode == dto.Barcode || l.Name == dto.Name )
                                && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( existingLocation != null )
            {
                if ( existingLocation.Barcode == dto.Barcode )
                    throw new InvalidOperationException ( $"Location with barcode '{dto.Barcode}' already exists." );
                if ( existingLocation.Name == dto.Name )
                    throw new InvalidOperationException ( $"Location with name '{dto.Name}' already exists." );
            }

            var location = Mapper.Map<Location> ( dto ); // AutoMapper handles mapping
            location.AddedOnDate = DateTime.Now;
            location.Name=dto.Name;
            location.Address=dto.Address;
            location.Barcode=dto.Barcode;
            location.Address=dto.Address;

            await UnitOfWork.writeRepository<Location> ( ).AddAsync ( location );
            await UnitOfWork.SaveChangeAsync ( );

            // Audit
            var auditTrail = new AuditTrail ( )
            {
                AddedOn = DateTime.Now,
                Action = "Added",
                EntityType = "Location",
                EntityName = location.Barcode,
                UserId = UserId ?? "System"
            };
            await UnitOfWork.writeRepository<AuditTrail> ( ).AddAsync ( auditTrail );
            await UnitOfWork.SaveChangeAsync ( );
        }
        #endregion

        #region Retrieve a location by Barcode (Manual Mapping)
        public async Task<GetLocationRequestDTO> GetLocationByBarcodeAsync ( string locationBarcode )
        {
            if ( string.IsNullOrEmpty ( locationBarcode ) ) throw new ArgumentException ( "Invalid location Barcode." );

            var location = await UnitOfWork.readRepository<Location> ( )
                .GetAsync ( l => l.Barcode == locationBarcode && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( location == null ) throw new KeyNotFoundException ( "Location not found." );

            // --- التحويل اليدوي ---
            return new GetLocationRequestDTO
            {
                Id = location.Id,
                Barcode = location.Barcode,
                Name = location.Name,
                Address = location.Address,
                AddedOnDate = location.AddedOnDate,
                UpdatedDate = location.UpdatedDate
            };
        }
        #endregion

        #region Retrieve all locations (Manual Mapping)
        public async Task<IEnumerable<GetLocationRequestDTO>> GetAllLocationsAsync ( )
        {
            var locations = await UnitOfWork.readRepository<Location> ( )
               .GetAllAsync ( predicate: l => ( l.IsDeleted == false || l.IsDeleted == null ) );

            // --- التحويل اليدوي باستخدام Select ---
            // ده بيحل مشكلة الـ IEnumerable error فوراً
            var dtos = locations.Select ( l => new GetLocationRequestDTO
            {
                Id = l.Id,
                Barcode = l.Barcode,
                Name = l.Name,
                Address = l.Address,
                AddedOnDate = l.AddedOnDate,
                UpdatedDate = l.UpdatedDate
            } ).ToList ( );

            return dtos;
        }
        #endregion

        #region Retrieve all locations Pagination (Manual Mapping)
        public async Task<IEnumerable<GetLocationRequestDTO>> GetAllByPaginationLocationsAsync ( int currentPage = 1, int pageSize = 10 )
        {
            var locations = await UnitOfWork.readRepository<Location> ( )
                .GetAllByPagningAsync ( predicate: l => ( l.IsDeleted == false || l.IsDeleted == null ), pageSize: pageSize, currentPage: currentPage );

            // --- التحويل اليدوي ---
            var dtos = locations.Select ( l => new GetLocationRequestDTO
            {
                Id = l.Id,
                Barcode = l.Barcode,
                Name = l.Name,
                Address = l.Address,
                AddedOnDate = l.AddedOnDate,
                UpdatedDate = l.UpdatedDate
            } ).ToList ( );

            return dtos;
        }
        #endregion


        //#region Retrieve all locations (Removed Audit)
        //public async Task<IEnumerable<GetLocationRequestDTO>> GetAllLocationsAsync ( )
        //{
        //    var locations = await UnitOfWork.readRepository<Location> ( )
        //        .GetAllAsync ( predicate: l => ( l.IsDeleted == false || l.IsDeleted == null ) );

        //    return Mapper.Map<IEnumerable<GetLocationRequestDTO>> ( locations );
        //}
        //#endregion

        //#region Retrieve all locations Pagination
        //public async Task<IEnumerable<GetLocationRequestDTO>> GetAllByPaginationLocationsAsync ( int currentPage = 1, int pageSize = 10 )
        //{
        //    var locations = await UnitOfWork.readRepository<Location> ( )
        //        .GetAllByPagningAsync ( predicate: l => ( l.IsDeleted == false || l.IsDeleted == null ), pageSize: pageSize, currentPage: currentPage );

        //    return Mapper.Map<IEnumerable<GetLocationRequestDTO>> ( locations );
        //}
        //#endregion

        #region Update a location (Fixed Logic)
        public async Task UpdateLocationAsync ( string barcode, UpdateLocationRequestDTO dto )
        {
            if ( dto == null ) throw new ArgumentNullException ( nameof ( dto ) );

            // 1. Get the Entity directly (Not DTO)
            var location = await UnitOfWork.readRepository<Location> ( )
                .GetAsync ( l => l.Barcode == barcode && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( location == null ) throw new KeyNotFoundException ( "Location not found." );

            // 2. Check duplicate name (excluding current location)
            var duplicateName = await UnitOfWork.readRepository<Location> ( )
                .GetAsync ( l => l.Name == dto.Name && l.Id != location.Id
                              && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( duplicateName != null )
                throw new InvalidOperationException ( "Another location with the same name already exists." );

            // 3. Update Fields
            location.Name = dto.Name;
            location.Address = dto.Address;
            location.UpdatedDate = DateTime.Now;

            await UnitOfWork.writeRepository<Location> ( ).UpdateAsync ( location.Id, location );
            await UnitOfWork.SaveChangeAsync ( );

            // Audit
            var auditTrail = new AuditTrail ( )
            {
                AddedOn = DateTime.Now,
                Action = "Update",
                EntityType = "Location",
                EntityName = location.Barcode,
                UserId = UserId ?? "System"
            };
            await UnitOfWork.writeRepository<AuditTrail> ( ).AddAsync ( auditTrail );
            await UnitOfWork.SaveChangeAsync ( );
        }
        #endregion

        #region Delete a location (Fixed Logic & Error Message)
        public async Task DeleteLocationAsync ( string barcode )
        {
            // 1. Get Entity
            var location = await UnitOfWork.readRepository<Location> ( )
                .GetAsync ( l => l.Barcode == barcode && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( location == null ) throw new KeyNotFoundException ( "Location not found." );

            // 2. Check Dependencies (Assets)
            var hasAssets = await UnitOfWork.readRepository<Asset> ( )
              .CountAsync ( a => a.LocationId == location.Id && ( a.IsDeleted == false || a.IsDeleted == null ) );

            if ( hasAssets > 0 )
                throw new InvalidOperationException ( "Cannot delete: There are Assets currently assigned to this location. Please move or delete them first." );

            // 3. Soft Delete
            location.DeletedDate = DateTime.Now;
            location.IsDeleted = true;

            await UnitOfWork.writeRepository<Location> ( ).UpdateAsync ( location.Id, location );
            await UnitOfWork.SaveChangeAsync ( );

            // Audit
            var auditTrail = new AuditTrail ( )
            {
                AddedOn = DateTime.Now,
                Action = "Delete",
                EntityType = "Location",
                EntityName = location.Barcode,
                UserId = UserId ?? "System"
            };
            await UnitOfWork.writeRepository<AuditTrail> ( ).AddAsync ( auditTrail );
            await UnitOfWork.SaveChangeAsync ( );
        }
        #endregion
    }
}