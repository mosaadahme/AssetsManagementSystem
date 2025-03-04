
using AssetsManagementSystem.DTOs.SupplierDTOs;

namespace AssetsManagementSystem.Services.Suppliers
{
    public class SupplierService : BaseClassForServices
    {
        public SupplierService(IUnitOfWork unitOfWork,
            Others.Interfaces.IAutoMapper.IMapper mapper, 
            IHttpContextAccessor httpContextAccessor) 
            : base(unitOfWork, mapper, httpContextAccessor)
        {
        }


        #region Adding a new supplier
        public async Task AddSupplierAsync(AddSupplierRequestDTO addSupplierRequest)
        {
            if (addSupplierRequest == null)
            {
                throw new ArgumentNullException(nameof(addSupplierRequest), "Supplier details cannot be null.");
            }

             var existingSupplier = await UnitOfWork.readRepository<Supplier>()
                                        .GetAsync(s => s.CompanyName == addSupplierRequest.CompanyName);

            if (existingSupplier != null)
            {
                throw new InvalidOperationException("A supplier with the same name already exists.");
            }

             var supplier = Mapper.Map<Supplier,AddSupplierRequestDTO>(addSupplierRequest);

             supplier.AddedOnDate = DateTime.Now;
             
            await UnitOfWork.writeRepository<Supplier>().AddAsync(supplier);

             await UnitOfWork.SaveChangeAsync();
        }
        #endregion

        #region Retrieve a supplier by ID
        public async Task<GetSupplierRequestDTO> GetSupplierByIdAsync(int supplierId)
        {
            if (supplierId <= 0)
            {
                throw new ArgumentException("Invalid supplier ID.");
            }

            var supplier = await UnitOfWork.readRepository<Supplier>().GetAsync(s => s.Id == supplierId 
            && (s.IsDeleted==false || s.IsDeleted==null));

            GetSupplierRequestDTO getSupplierRequestDTO = Mapper.Map<GetSupplierRequestDTO, Supplier>(supplier);

            if (supplier == null)
            {
                throw new KeyNotFoundException("Supplier not found.");
            }

            return getSupplierRequestDTO;
        }
        #endregion

        #region Retrieve all suppliers
        public async Task<IEnumerable<GetSupplierRequestDTO>> GetAllSuppliersAsync()
        {
           //var Suppliers=await UnitOfWork.readRepository<Supplier>()
           //     .GetAllAsync(predicate: s=> (s.IsDeleted == false || s.IsDeleted == null));
          var Suppliers=await UnitOfWork.readRepository<Supplier>()
                .GetAllAsync(predicate: s=> (  s.IsDeleted == null));
         
            var getSupplierRequestDTOs = 
                Mapper.Map<GetSupplierRequestDTO, Supplier>(Suppliers);

            return getSupplierRequestDTOs;

        }
        #endregion

        #region Retrieve all suppliers
        public async Task<IEnumerable<GetSupplierRequestDTO>> GetAllByPaginationSuppliersAsync(int currentPage = 1, int pageSize = 10)
        {
            var Suppliers = await UnitOfWork.readRepository<Supplier>()
                 .GetAllByPagningAsync(predicate: s => (s.IsDeleted == false || s.IsDeleted == null), pageSize: pageSize, currentPage: currentPage);

            var getSupplierRequestDTOs =
                Mapper.Map<GetSupplierRequestDTO, Supplier>(Suppliers);

            return getSupplierRequestDTOs;

        }
        #endregion

        #region Update a supplier
        public async Task UpdateSupplierAsync(int supplierId, UpdateSupplierRequestDTO updateSupplierRequest)
        {
            if (updateSupplierRequest == null)
            {
                throw new ArgumentNullException(nameof(updateSupplierRequest), "Supplier details cannot be null.");
            }

            var supplierDTO = await GetSupplierByIdAsync(supplierId);

            var supplier=Mapper.Map<Supplier,GetSupplierRequestDTO>(supplierDTO);

             var existingSupplier = await UnitOfWork.readRepository<Supplier>()
                                        .GetAsync(s => s.CompanyName == updateSupplierRequest.CompanyName && s.Id != supplierId);

            if (existingSupplier != null)
            {
                throw new InvalidOperationException("Another supplier with the same name already exists.");
            }

             supplier.CompanyName = updateSupplierRequest.CompanyName;

            supplier.email = updateSupplierRequest.email;
            supplier.Address = updateSupplierRequest.Address;
            supplier.PhoneNumber = updateSupplierRequest.PhoneNumber;
            supplier.Note = updateSupplierRequest.Note;

            supplier.UpdatedDate = DateTime.Now;

            supplier.AddedOnDate = supplier.AddedOnDate;

             await UnitOfWork.writeRepository<Supplier>().UpdateAsync(supplier.Id, supplier);

             await UnitOfWork.SaveChangeAsync();
        }
        #endregion

        #region Delete a supplier
        public async Task DeleteSupplierAsync(int supplierId)
        {
            var supplierDTO = await GetSupplierByIdAsync(supplierId);

            var asset = await UnitOfWork.readRepository<Asset>()
                .GetAsync(predicate: a => a.AssetsSuppliers.Any(As => As.SupplierId == supplierId));
            if (asset is not null)
                throw new InvalidOperationException("There are suppliers dependent on this supplier,Please Go and delete it first");

            var supplier = Mapper.Map<Supplier, GetSupplierRequestDTO>(supplierDTO);


            supplier.DeletedDate = DateTime.Now;
            supplier.IsDeleted = true;

            await UnitOfWork.writeRepository<Supplier>().UpdateAsync(supplier.Id, supplier);

            await UnitOfWork.SaveChangeAsync();
        }

        #endregion
    }
}
