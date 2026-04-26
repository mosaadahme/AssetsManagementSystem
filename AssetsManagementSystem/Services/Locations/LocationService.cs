using AssetsManagementSystem.DTOs.LocationDTOs;
using AssetsManagementSystem.Models.DbSets;
using AssetsManagementSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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

            // 1. Check Duplicate Barcode OR Name
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

            // 2. Validate Parent Location if exists
            if ( dto.ParentLocationId.HasValue )
            {
                var parentExists = await UnitOfWork.readRepository<Location> ( )
                    .GetAsync ( l => l.Id == dto.ParentLocationId.Value && ( l.IsDeleted == false || l.IsDeleted == null ) );
                if ( parentExists == null )
                    throw new KeyNotFoundException ( "The specified Parent Location does not exist." );
            }

            // 3. Generate Full Address dynamically
            string fullAddress = await GenerateFullAddressAsync ( dto.ParentLocationId, dto.Name );

            // 4. Manual Mapping (DTO to Entity)
            var location = new Location
            {
                Barcode = dto.Barcode,
                Name = dto.Name,
                Level = dto.Level,
                ParentLocationId = dto.ParentLocationId,
                Address = fullAddress, // العنوان المجمع أوتوماتيك
                AddedOnDate = DateTime.Now
            };

            await UnitOfWork.writeRepository<Location> ( ).AddAsync ( location );
            await UnitOfWork.SaveChangeAsync ( );

            // 5. Audit
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

        #region Retrieve a location by Barcode
        public async Task<GetLocationRequestDTO> GetLocationByBarcodeAsync ( string locationBarcode )
        {
            if ( string.IsNullOrEmpty ( locationBarcode ) ) throw new ArgumentException ( "Invalid location Barcode." );

            // لو الـ Generic Repository بتاعك بيدعم الـ Includes، يفضل تعمل Include للـ ParentLocation هنا
            var location = await UnitOfWork.readRepository<Location> ( )
                .GetAsync ( l => l.Barcode == locationBarcode && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( location == null ) throw new KeyNotFoundException ( "Location not found." );

            // Fetch parent name if exists (fallback if Include isn't available)
            string parentName = null;
            if ( location.ParentLocationId.HasValue )
            {
                var parent = await UnitOfWork.readRepository<Location> ( ).GetAsync ( l => l.Id == location.ParentLocationId );
                parentName = parent?.Name;
            }

            // --- Manual Mapping ---
            return new GetLocationRequestDTO
            {
                Id = location.Id,
                Barcode = location.Barcode,
                Name = location.Name,
                Address = location.Address,
                Level = location.Level,
                ParentLocationId = location.ParentLocationId,
                ParentLocationName = location.ParentLocation?.Name ?? parentName,
                AddedOnDate = location.AddedOnDate,
                UpdatedDate = location.UpdatedDate
            };
        }
        #endregion

        #region Retrieve all locations
        public async Task<IEnumerable<GetLocationRequestDTO>> GetAllLocationsAsync ( )
        {
            var locations = await UnitOfWork.readRepository<Location> ( )
               .GetAllAsync ( predicate: l => ( l.IsDeleted == false || l.IsDeleted == null ) );

            // --- Manual Mapping ---
            var dtos = locations.Select ( l => new GetLocationRequestDTO
            {
                Id = l.Id,
                Barcode = l.Barcode,
                Name = l.Name,
                Address = l.Address,
                Level = l.Level,
                ParentLocationId = l.ParentLocationId,
                ParentLocationName = l.ParentLocation?.Name, // Assuming EF tracks it or it's included
                AddedOnDate = l.AddedOnDate,
                UpdatedDate = l.UpdatedDate
            } ).ToList ( );

            return dtos;
        }
        #endregion

        #region Retrieve all locations Pagination
        public async Task<IEnumerable<GetLocationRequestDTO>> GetAllByPaginationLocationsAsync ( int currentPage = 1, int pageSize = 10 )
        {
            var locations = await UnitOfWork.readRepository<Location> ( )
                .GetAllByPagningAsync ( predicate: l => ( l.IsDeleted == false || l.IsDeleted == null ), pageSize: pageSize, currentPage: currentPage );

            // --- Manual Mapping ---
            var dtos = locations.Select ( l => new GetLocationRequestDTO
            {
                Id = l.Id,
                Barcode = l.Barcode,
                Name = l.Name,
                Address = l.Address,
                Level = l.Level,
                ParentLocationId = l.ParentLocationId,
                ParentLocationName = l.ParentLocation?.Name, // Assuming EF tracks it or it's included
                AddedOnDate = l.AddedOnDate,
                UpdatedDate = l.UpdatedDate
            } ).ToList ( );

            return dtos;
        }
        #endregion

        #region Update a location
        public async Task UpdateLocationAsync ( string barcode, UpdateLocationRequestDTO dto )
        {
            if ( dto == null ) throw new ArgumentNullException ( nameof ( dto ) );

            // 1. Get the Entity directly
            var location = await UnitOfWork.readRepository<Location> ( )
                .GetAsync ( l => l.Barcode == barcode && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( location == null ) throw new KeyNotFoundException ( "Location not found." );

            // 2. Check duplicate name (excluding current location)
            var duplicateName = await UnitOfWork.readRepository<Location> ( )
                .GetAsync ( l => l.Name == dto.Name && l.Id != location.Id
                              && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( duplicateName != null )
                throw new InvalidOperationException ( "Another location with the same name already exists." );

            bool nameOrParentChanged = location.Name != dto.Name || location.ParentLocationId != dto.ParentLocationId;

            // 3. Update Fields
            location.Name = dto.Name;
            location.Level = dto.Level;
            location.ParentLocationId = dto.ParentLocationId;
            location.UpdatedDate = DateTime.Now;

            // 4. Update Address if Hierarchy changed
            if ( nameOrParentChanged )
            {
                location.Address = await GenerateFullAddressAsync ( dto.ParentLocationId, dto.Name );
            }

            await UnitOfWork.writeRepository<Location> ( ).UpdateAsync ( location.Id, location );
            await UnitOfWork.SaveChangeAsync ( );

            // 5. Cascade Update Children Addresses (If Name or Parent Changed)
            if ( nameOrParentChanged )
            {
                await UpdateChildrenAddressesAsync ( location.Id, location.Address );
            }

            // 6. Audit
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

        #region Delete a location
        public async Task DeleteLocationAsync ( string barcode )
        {
            var location = await UnitOfWork.readRepository<Location> ( )
                .GetAsync ( l => l.Barcode == barcode && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( location == null ) throw new KeyNotFoundException ( "Location not found." );

            // 1. Check if it has Child Locations
            var hasChildren = await UnitOfWork.readRepository<Location> ( )
                .CountAsync ( l => l.ParentLocationId == location.Id && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( hasChildren > 0 )
                throw new InvalidOperationException ( "Cannot delete: This location has sub-locations (Children). Please delete or move them first." );

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

            // 4. Audit
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

        // =========================================================================
        // Helper Methods (Private)
        // =========================================================================

        #region Helper: Generate Full Address
        private async Task<string> GenerateFullAddressAsync ( int? parentId, string currentName )
        {
            if ( !parentId.HasValue ) return currentName;

            var pathNames = new List<string> ( );
            int? currentId = parentId;

            // Loop to climb up the hierarchy tree
            while ( currentId.HasValue )
            {
                var loc = await UnitOfWork.readRepository<Location> ( )
                    .GetAsync ( l => l.Id == currentId.Value && ( l.IsDeleted == false || l.IsDeleted == null ) );

                if ( loc == null ) break;

                pathNames.Add ( loc.Name );
                currentId = loc.ParentLocationId;
            }

            pathNames.Reverse ( ); // Egypt -> Alex -> Smouha
            pathNames.Add ( currentName ); // Add the new location name at the end

            return string.Join ( " - ", pathNames );
        }
        #endregion

        #region Helper: Cascade Update Children Addresses
        private async Task UpdateChildrenAddressesAsync ( int parentId, string parentAddress )
        {
            // نجيب كل الأبناء المباشرين للمكان ده
            var children = await UnitOfWork.readRepository<Location> ( )
                .GetAllAsync ( l => l.ParentLocationId == parentId && ( l.IsDeleted == false || l.IsDeleted == null ) );

            foreach ( var child in children )
            {
                // نحدث عنوان الابن بناءً على عنوان الأب الجديد
                child.Address = $"{parentAddress} - {child.Name}";
                child.UpdatedDate = DateTime.Now;

                await UnitOfWork.writeRepository<Location> ( ).UpdateAsync ( child.Id, child );

                // ننده نفس الدالة تاني (Recursion) عشان لو الابن ده جواه أبناء (أحفاد)
                await UpdateChildrenAddressesAsync ( child.Id, child.Address );
            }

            if ( children.Any ( ) )
            {
                await UnitOfWork.SaveChangeAsync ( );
            }
        }

        #region 1. Get Location Levels (الليفيلز)
        public IEnumerable<object> GetLocationLevels ( )
        {
            // دي مش محتاجة داتابيز، بتقرأ من الـ Enum مباشرة
            var levels = Enum.GetValues ( typeof ( LocationLevel ) )
                             .Cast<LocationLevel> ( )
                             .Select ( e => new
                             {
                                 Id = (int) e,
                                 Name = e.ToString ( )
                             } ).ToList ( );

            return levels;
        }
        #endregion

        #region 2. Get Locations By Parent (التتابع / Cascading)
        public async Task<IEnumerable<GetLocationRequestDTO>> GetLocationsByParentAsync ( int? parentId )
        {
            // لو parentId بـ null، هيجيب أعلى مستوى (الدول)
            var locations = await UnitOfWork.readRepository<Location> ( )
                .GetAllAsync ( l => l.ParentLocationId == parentId && ( l.IsDeleted == false || l.IsDeleted == null ) );

            // Manual Mapping
            var dtos = locations.Select ( l => new GetLocationRequestDTO
            {
                Id = l.Id,
                Barcode = l.Barcode,
                Name = l.Name,
                Address = l.Address,
                Level = l.Level,
                ParentLocationId = l.ParentLocationId,
                AddedOnDate = l.AddedOnDate,
                UpdatedDate = l.UpdatedDate
            } ).ToList ( );

            return dtos;
        }
        #endregion

        #region 3. Get Location Breadcrumbs (المسار العكسي)
        public async Task<IEnumerable<LocationBreadcrumbDTO>> GetLocationBreadcrumbsAsync ( int locationId )
        {
            var breadcrumbs = new List<LocationBreadcrumbDTO> ( );
            int? currentId = locationId;

            // هنفضل نطلع لفوق لحد ما نوصل للدولة (اللي ملهاش Parent)
            while ( currentId.HasValue )
            {
                var loc = await UnitOfWork.readRepository<Location> ( )
                    .GetAsync ( l => l.Id == currentId.Value && ( l.IsDeleted == false || l.IsDeleted == null ) );

                if ( loc == null ) break;

                breadcrumbs.Add ( new LocationBreadcrumbDTO
                {
                    Id = loc.Id,
                    Name = loc.Name,
                    Level = (int) loc.Level,
                    LevelName = loc.Level.ToString ( )
                } );

                currentId = loc.ParentLocationId;
            }

            // الليستة دلوقتي (غرفة -> دور -> مبنى)، هنعكسها عشان الفرونت يعرضها صح (مبنى -> دور -> غرفة)
            breadcrumbs.Reverse ( );
            return breadcrumbs;
        }
        #endregion

        #region 4. Search Locations (البحث السريع)
        public async Task<IEnumerable<GetLocationRequestDTO>> SearchLocationsAsync ( string query )
        {
            if ( string.IsNullOrWhiteSpace ( query ) )
                return new List<GetLocationRequestDTO> ( );

            query = query.ToLower ( );

            var locations = await UnitOfWork.readRepository<Location> ( )
                .GetAllAsync ( l => ( l.IsDeleted == false || l.IsDeleted == null ) &&
                                  ( l.Name.ToLower ( ).Contains ( query ) || l.Barcode.ToLower ( ).Contains ( query ) ) );

            // Manual Mapping
            var dtos = locations.Select ( l => new GetLocationRequestDTO
            {
                Id = l.Id,
                Barcode = l.Barcode,
                Name = l.Name,
                Address = l.Address, // العنوان التفصيلي هيفيد جداً في نتيجة البحث
                Level = l.Level,
                ParentLocationId = l.ParentLocationId,
                AddedOnDate = l.AddedOnDate,
                UpdatedDate = l.UpdatedDate
            } ).ToList ( );

            return dtos;
        }
        #endregion
        #endregion
    }
}