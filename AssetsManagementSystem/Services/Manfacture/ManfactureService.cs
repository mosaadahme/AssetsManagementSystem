
using AssetsManagementSystem.DTOs.ManufacturerDTOs;
using AssetsManagementSystem.Models.DbSets;

namespace AssetsManagementSystem.Services.Manfacture
{
    public class ManfactureService : BaseClassForServices
    {
        public ManfactureService(IUnitOfWork unitOfWork, 
            Others.Interfaces.IAutoMapper.IMapper mapper, 
            IHttpContextAccessor httpContextAccessor) 
            : base(unitOfWork, mapper, httpContextAccessor)
        {
        }
        
        public async Task<GetManufacturerResponseDTO> AddManfacture(AddManufacturerRequestDTO addManufacturerRequest) 
        {

            if (await UnitOfWork.readRepository<Manufacturer>().GetAsync(m => m.Name == addManufacturerRequest.Name) is not null)
            {
                throw new InvalidOperationException("This Name Is Already Exist");    
            }

           var Manfacture= Mapper.Map<Manufacturer,AddManufacturerRequestDTO>(addManufacturerRequest);
            Manfacture.IsDeleted=false;
            Manfacture.Info = addManufacturerRequest.Info;
            Manfacture.Name = addManufacturerRequest.Name;
      
            await UnitOfWork.writeRepository<Manufacturer>().AddAsync(Manfacture);
           await UnitOfWork.SaveChangeAsync();

            return await GetManfactureBtId(Manfacture.Id);   
        }

        public async Task<GetManufacturerResponseDTO> GetManfactureBtId(int id)
        {
            var Manfacure = await UnitOfWork.readRepository<Manufacturer>().GetAsync(m => m.Id == id&&m.IsDeleted==false);
            if (Manfacure is null)
            {
                throw new InvalidOperationException("This Manfacure Is not Exist ,may be deleted");
            }
            return Mapper.Map<GetManufacturerResponseDTO, Manufacturer>(Manfacure); ;
        }  
        
        public async Task<IList<GetManufacturerResponseDTO>> GetAll( )
        {
            var Manfacure = await UnitOfWork.readRepository<Manufacturer>().GetAllAsync(m => m.IsDeleted==false);
            
            return Mapper.Map<GetManufacturerResponseDTO, Manufacturer>(Manfacure); ;
        }

        public async Task<IList<GetManufacturerResponseDTO>> GetAllByPaginationManfactureBtId(int currentPage = 1, int pageSize = 10)
        {
            var Manfacure = await UnitOfWork.readRepository<Manufacturer>().GetAllByPagningAsync(m =>m.IsDeleted == false , 
                                                                pageSize: pageSize, currentPage: currentPage);
            if (Manfacure is null)
            {
                throw new InvalidOperationException("This Manfacure Is not Exist ,may be deleted");
            }
            return Mapper.Map<GetManufacturerResponseDTO, Manufacturer>(Manfacure); ;
        }

        public async Task UpdateManfacuture(int id,UpdateManufacturerRequestDTO updateManufacturerRequest)
        {
            var Manfacure = await UnitOfWork.readRepository<Manufacturer>().GetAsync(m => m.Id == id && m.IsDeleted == false);
            if (Manfacure is null)
            {
                throw new InvalidOperationException("This Manfacure Is not Exist ,may be deleted");
            }


            var existingManufacturer = await UnitOfWork.readRepository<Manufacturer>()
                                    .GetAsync(m => m.Name == updateManufacturerRequest.Name && m.Id != id
                                    && (m.IsDeleted == false));

            if (existingManufacturer != null)
            {
                throw new InvalidOperationException("Another location with the same name already exists.");
            }

             Manfacure.Name = updateManufacturerRequest.Name;
            Manfacure.Info = updateManufacturerRequest.Info;
            await UnitOfWork.writeRepository<Manufacturer>().UpdateAsync(id, Manfacure);
            await UnitOfWork.SaveChangeAsync(); 
        }


        public async Task DeleteManfacute(int id)
        {
            var GetManfactureB = await GetManfactureBtId(id);

            var asset = await UnitOfWork.readRepository<Asset>()
              .GetAsync(predicate: a => a.ManufacturerId == GetManfactureB.Id && (a.IsDeleted == false || a.IsDeleted == null));

            if (asset is not null)
                throw new InvalidOperationException("There are assets dependent on this Manufacturer,Please Go and delete it first");

            var Manfacture = Mapper.Map<Manufacturer, GetManufacturerResponseDTO>(GetManfactureB);

             Manfacture.IsDeleted = true;

            await UnitOfWork.writeRepository<Manufacturer>().UpdateAsync(Manfacture.Id, Manfacture);

            await UnitOfWork.SaveChangeAsync();
        }

    }
}
