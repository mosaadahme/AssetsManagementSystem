
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

            var supplier = new Supplier
            {
                CompanyName = addSupplierRequest.CompanyName,
                email = addSupplierRequest.email,
                PhoneNumber = addSupplierRequest.PhoneNumber,
                Address = addSupplierRequest.Address,
                Note = addSupplierRequest.Note,
                AddedOnDate = DateTime.Now,
                ContactPersons = new List<SupplierContactPerson> ( ) // تهيئة القائمة لتجنب NullReferenceException
            };

            if ( addSupplierRequest.ContactPersons != null && addSupplierRequest.ContactPersons.Any ( ) )
            {
                supplier.ContactPersons = addSupplierRequest.ContactPersons.Select ( c => new SupplierContactPerson
                {
                    Name = c.Name,
                    JobTitle = c.JobTitle,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    Note = c.Note,
                    IsActive = c.IsActive, // افتراضياً فعال عند الإضافة
                    AddedOnDate = DateTime.Now
                } ).ToList ( );
            }

            await UnitOfWork.writeRepository<Supplier>().AddAsync(supplier);

             await UnitOfWork.SaveChangeAsync();
        }
        #endregion

        #region Retrieve a supplier by ID
        public async Task<GetSupplierRequestDTO> GetSupplierByIdAsync ( int supplierId )
        {
            if ( supplierId <= 0 )
            {
                throw new ArgumentException ( "Invalid supplier ID.", nameof ( supplierId ) );
            }

            var supplier = await UnitOfWork.readRepository<Supplier> ( )
                                .GetAsync ( s => s.Id == supplierId && ( s.IsDeleted == false || s.IsDeleted == null ) );

            // 1. التحقق من وجود المورد (يجب أن يكون هنا قبل المابينج)
            if ( supplier == null )
            {
                throw new KeyNotFoundException ( "Supplier not found." );
            }

            // 2. المابينج اليدوي (Manual Mapping)
            var getSupplierRequestDTO = new GetSupplierRequestDTO
            {
                Id = supplier.Id,
                CompanyName = supplier.CompanyName,
                email = supplier.email,
                PhoneNumber = supplier.PhoneNumber,
                Address = supplier.Address,
                Note = supplier.Note,
                AddedOnDate = supplier.AddedOnDate,
                UpdatedDate = supplier.UpdatedDate,

                // مابينج جهات الاتصال بخطوة واحدة مع حماية من الـ Null
                ContactPersons = supplier.ContactPersons?.Select ( c => new SupplierContactPersonDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    JobTitle = c.JobTitle,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    Note = c.Note,
                    IsActive = c.IsActive
                } ).ToList ( ) ?? new List<SupplierContactPersonDTO> ( )
            };

            return getSupplierRequestDTO;
        }
        #endregion


        #region Retrieve all suppliers
        public async Task<IEnumerable<GetSupplierRequestDTO>> GetAllSuppliersAsync ( )
        {
            // 1. جلب البيانات من قاعدة البيانات
            // ملاحظة: يُفضل استخدام (s.IsDeleted != true) لتغطية الحالتين (null و false) بعبارة أقصر
            var suppliers = await UnitOfWork.readRepository<Supplier> ( )
                .GetAllAsync ( predicate: s => s.IsDeleted != true );

            // التحقق من أن القائمة ليست فارغة لتجنب أي أخطاء (اختياري لكن مفضل)
            if ( suppliers == null || !suppliers.Any ( ) )
            {
                return new List<GetSupplierRequestDTO> ( ); // إرجاع قائمة فارغة بدلاً من null
            }

            // 2. المابينج اليدوي للقائمة باستخدام Select
            var getSupplierRequestDTOs = suppliers.Select ( supplier => new GetSupplierRequestDTO
            {
                Id = supplier.Id,
                CompanyName = supplier.CompanyName,
                email = supplier.email,
                PhoneNumber = supplier.PhoneNumber,
                Address = supplier.Address,
                Note = supplier.Note,
                AddedOnDate = supplier.AddedOnDate,
                UpdatedDate = supplier.UpdatedDate,

                // مابينج جهات الاتصال لكل مورد في القائمة
                ContactPersons = supplier.ContactPersons?.Select ( c => new SupplierContactPersonDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    JobTitle = c.JobTitle,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    Note = c.Note,
                    IsActive = c.IsActive
                } ).ToList ( ) ?? new List<SupplierContactPersonDTO> ( )
            } ).ToList ( ); // استخدام ToList() لتنفيذ الاستعلام فوراً في الذاكرة

            return getSupplierRequestDTOs;
        }
        #endregion

        #region Retrieve all suppliers
        public async Task<IEnumerable<GetSupplierRequestDTO>> GetAllByPaginationSuppliersAsync(int currentPage = 1, int pageSize = 10)
        {
            var Suppliers = await UnitOfWork.readRepository<Supplier>()
                 .GetAllByPagningAsync(predicate: s => (s.IsDeleted == false || s.IsDeleted == null), pageSize: pageSize, currentPage: currentPage);

            var getSupplierRequestDTOs =
                Mapper.Map<GetSupplierRequestDTO, Supplier>(Suppliers, "ContactPersons" );

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

            var supplier = new Supplier
            {
                Id = supplierDTO.Id,
                CompanyName = supplierDTO.CompanyName,
                email = supplierDTO.email,
                PhoneNumber = supplierDTO.PhoneNumber,
                Address = supplierDTO.Address,
                Note = supplierDTO.Note,
                AddedOnDate = supplierDTO.AddedOnDate,
                UpdatedDate = supplierDTO.UpdatedDate,
                ContactPersons = supplierDTO.ContactPersons?.Select ( c => new SupplierContactPerson
                {
                    Id = c.Id ?? 0,
                    Name = c.Name,
                    JobTitle = c.JobTitle,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    Note = c.Note,
                    IsActive = c.IsActive
                } ).ToList ( ) ?? new List<SupplierContactPerson> ( )
            };
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

            if ( updateSupplierRequest.ContactPersons != null )
            {
                // مسح الأشخاص اللي الفرونت مبعتهمش (معناها إنهم اتمسحوا من الشاشة)
                var incomingIds = updateSupplierRequest.ContactPersons.Where ( c => c.Id.HasValue ).Select ( c => c.Id.Value ).ToList ( );
                var contactsToRemove = supplier.ContactPersons.Where ( c => !incomingIds.Contains ( c.Id ) ).ToList ( );

                foreach ( var contact in contactsToRemove )
                {
                    // ممكن تمسحه خالص أو تخليه IsActive = false حسب البيزنس
                    contact.IsActive = false;
                    contact.IsDeleted = true;
                    contact.DeletedDate = DateTime.Now;
                }

                // إضافة أو تعديل الأشخاص اللي جايين في الريكويست
                foreach ( var incomingContact in updateSupplierRequest.ContactPersons )
                {
                    if ( incomingContact.Id.HasValue && incomingContact.Id.Value > 0 )
                    {
                        // تعديل شخص موجود
                        var existingContact = supplier.ContactPersons.FirstOrDefault ( c => c.Id == incomingContact.Id.Value );
                        if ( existingContact != null )
                        {
                            existingContact.Name = incomingContact.Name;
                            existingContact.JobTitle = incomingContact.JobTitle;
                            existingContact.Email = incomingContact.Email;
                            existingContact.PhoneNumber = incomingContact.PhoneNumber;
                            existingContact.Note = incomingContact.Note;
                            existingContact.IsActive = incomingContact.IsActive;
                            existingContact.UpdatedDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        // إضافة شخص جديد متضاف في شاشة التعديل
                        supplier.ContactPersons.Add ( new SupplierContactPerson
                        {
                            Name = incomingContact.Name,
                            JobTitle = incomingContact.JobTitle,
                            Email = incomingContact.Email,
                            PhoneNumber = incomingContact.PhoneNumber,
                            Note = incomingContact.Note,
                            IsActive = incomingContact.IsActive,
                            AddedOnDate = DateTime.Now
                        } );
                    }
                }
            }


            await UnitOfWork.writeRepository<Supplier>().UpdateAsync(supplier.Id, supplier);

             await UnitOfWork.SaveChangeAsync();
        }
        #endregion

        #region Delete a supplier
        public async Task DeleteSupplierAsync ( int supplierId )
        {
            var supplierDTO = await GetSupplierByIdAsync ( supplierId );

            var asset = await UnitOfWork.readRepository<Asset> ( )
                .GetAsync ( predicate: a => a.AssetsSuppliers.Any ( As => As.SupplierId == supplierId ) );
            if ( asset is not null )
                throw new InvalidOperationException ( "There are suppliers dependent on this supplier,Please Go and delete it first" );

            // ---------------- بداية التعديل: المابينج اليدوي ----------------
            var supplier = new Supplier
            {
                Id = supplierDTO.Id,
                CompanyName = supplierDTO.CompanyName,
                email = supplierDTO.email,
                PhoneNumber = supplierDTO.PhoneNumber,
                Address = supplierDTO.Address,
                Note = supplierDTO.Note,
                AddedOnDate = supplierDTO.AddedOnDate,
                UpdatedDate = supplierDTO.UpdatedDate,
                ContactPersons = supplierDTO.ContactPersons?.Select ( c => new SupplierContactPerson
                {
                    Id = c.Id ?? 0,
                    Name = c.Name,
                    JobTitle = c.JobTitle,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    Note = c.Note,
                    IsActive = c.IsActive
                } ).ToList ( ) ?? new List<SupplierContactPerson> ( )
            };
            // ---------------- نهاية التعديل ----------------

            supplier.DeletedDate = DateTime.Now;
            supplier.IsDeleted = true;

            await UnitOfWork.writeRepository<Supplier> ( ).UpdateAsync ( supplier.Id, supplier );

            await UnitOfWork.SaveChangeAsync ( );
        }

        #endregion
    }
}
