using AssetsManagementSystem.DTOs.CategoryDTOs;
using AssetsManagementSystem.Models.DbSets;

namespace AssetsManagementSystem.Services.Categories
{
    public class CategoryService:BaseClassForServices
    {
        public CategoryService(IUnitOfWork unitOfWork,
            Others.Interfaces.IAutoMapper.IMapper mapper,
            IHttpContextAccessor httpContextAccessor) 
            : base(unitOfWork, mapper, httpContextAccessor)
        {
        }

        //#region Adding a new category
        //public async Task AddCategoryAsync(AddCategoryRequestDTO addCategoryRequest)
        //{
        //    if (addCategoryRequest == null)
        //    {
        //        throw new ArgumentNullException(nameof(addCategoryRequest), "Category details cannot be null.");
        //    }

        //     var existingCategory = await UnitOfWork.readRepository<Category>()
        //                                .GetAsync(c => c.Name == addCategoryRequest.Name);

        //    if (existingCategory != null)
        //    {
        //        throw new InvalidOperationException("A category with the same name already exists.");
        //    }

        //      var category = Mapper.Map<Category,AddCategoryRequestDTO>(addCategoryRequest);

        //      category.AddedOnDate=DateTime.Now;

        //    var categoryforId = addCategoryRequest.ParentCategoryId == 0 ? null :
        //        await UnitOfWork.readRepository<Category>().GetAsync(c => c.SerialCode == (addCategoryRequest.ParentCategoryId).ToString());

        //     category.ParentCategoryId=categoryforId?.Id;


        //     await UnitOfWork.writeRepository<Category>().AddAsync(category);

        //     await UnitOfWork.SaveChangeAsync();
        //}
        //#endregion


        #region Add Category
        public async Task AddCategoryAsync ( AddCategoryRequestDTO dto )
        {
            if ( dto == null ) throw new ArgumentNullException ( nameof ( dto ) );

            // 1. Check Validations (Name & Code uniqueness)
            var existingCategory = await UnitOfWork.readRepository<Category> ( )
                .GetAsync ( c => ( c.Name == dto.Name || c.SerialCode == dto.SerialCode )
                               && ( c.IsDeleted == false || c.IsDeleted == null ) );

            if ( existingCategory != null )
            {
                if ( existingCategory.Name == dto.Name )
                    throw new InvalidOperationException ( $"Category with name '{dto.Name}' already exists." );
                if ( existingCategory.SerialCode == dto.SerialCode )
                    throw new InvalidOperationException ( $"Category Code '{dto.SerialCode}' already exists." );
            }

            // 2. Validate Parent Category (if exists)
            if ( dto.ParentCategoryId.HasValue && dto.ParentCategoryId > 0 )
            {
                var parent = await UnitOfWork.readRepository<Category> ( )
                    .GetAsync ( c => c.Id == dto.ParentCategoryId && ( c.IsDeleted == false || c.IsDeleted == null ) );

                if ( parent == null ) throw new KeyNotFoundException ( "Parent Category not found." );
            }

            var category = Mapper.Map<Category> ( dto ); // Correct AutoMapper syntax

            category.AddedOnDate = DateTime.Now;
            category.ParentCategoryId = ( dto.ParentCategoryId == 0 ) ? null : dto.ParentCategoryId;

            await UnitOfWork.writeRepository<Category> ( ).AddAsync ( category );
            await UnitOfWork.SaveChangeAsync ( );
        }
        #endregion


        #region Get Main Categories
        public async Task<IList<GetCategoryRequestDTO>> GetMainCategory ( )
        {
            var categories = await UnitOfWork.readRepository<Category> ( )
                .GetAllAsync ( predicate: c => ( c.ParentCategoryId == null || c.ParentCategoryId == 0 )
                                          && ( c.IsDeleted == false || c.IsDeleted == null ) );

            // يفضل الـ Manual Mapping للتحكم الكامل، أو AutoMapper
            // هنا مثال Manual عشان تبقى شبه الدوال التانية
            return categories.Select ( c => new GetCategoryRequestDTO
            {
                Id = c.Id,
                Name = c.Name,
                SerialCode = c.SerialCode,
                Description = c.Description,
                AddedOnDate = c.AddedOnDate,
                UpdatedDate = c.UpdatedDate
            } ).ToList ( );
        }
        #endregion

        #region Get SubCategories
        public async Task<IList<GetCategoryRequestDTO>> GetSubCategory ( int parentId )
        {
            var categories = await UnitOfWork.readRepository<Category> ( )
                .GetAllAsync (
                    predicate: c => c.ParentCategoryId == parentId && ( c.IsDeleted == false || c.IsDeleted == null ),
                    include: src => src.Include ( c => c.ParentCategory ) // Include Parent Name
                );

            return categories.Select ( c => new GetCategoryRequestDTO
            {
                Id = c.Id,
                Name = c.Name,
                SerialCode = c.SerialCode,
                Description = c.Description,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = c.ParentCategory?.Name, // Safe Navigation
                AddedOnDate = c.AddedOnDate,
                UpdatedDate = c.UpdatedDate
            } ).ToList ( );
        }
        #endregion


        #region Get By ID  
        public async Task<GetCategoryRequestDTO> GetCategoryByIdAsync ( int categoryId )
        {
            if ( categoryId <= 0 ) throw new ArgumentException ( "Invalid category ID." );

            var category = await UnitOfWork.readRepository<Category> ( )
                .GetAsync (
                    predicate: c => c.Id == categoryId && ( c.IsDeleted == false || c.IsDeleted == null ),
                    include: src => src.Include ( c => c.ParentCategory ) // Fix Null Reference
                );

            if ( category == null ) throw new KeyNotFoundException ( "Category not found." );

            // Manual Mapping is safer here
            return new GetCategoryRequestDTO
            {
                Id = category.Id,
                Name = category.Name,
                SerialCode = category.SerialCode,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name ?? "Main Category",
                AddedOnDate = category.AddedOnDate,
                UpdatedDate = category.UpdatedDate
            };
        }
        #endregion

        #region Retrieve all categories
        public async Task<IEnumerable<GetCategoryRequestDTO>> GetAllCategoriesAsync()
        {
            var categories= await UnitOfWork.readRepository<Category>()
                .GetAllAsync(predicate: c=> (c.IsDeleted == false || c.IsDeleted == null));

            var getCategoryRequestDTOs = categories.Select
                (
                c=>new GetCategoryRequestDTO()
                {
                    Id= c.Id,   
                    Name= c.Name,
                    SerialCode= c.SerialCode,
                    ParentCategoryId= c.ParentCategoryId==null?null:c.ParentCategoryId,
                    ParentCategoryName=c.ParentCategoryId==null?null:c.ParentCategory.Name,
                    AddedOnDate=c.AddedOnDate,
                    UpdatedDate=c.UpdatedDate
                }
                );
            return getCategoryRequestDTOs;
            
        }
        #endregion

        #region Retrieve all categories
        public async Task<IEnumerable<GetCategoryRequestDTO>> GetAllByPaginationCategoriesAsync(int currentPage = 1, int pageSize = 10)
        {
            var categories = await UnitOfWork.readRepository<Category>()
                .GetAllByPagningAsync(predicate: c => (c.IsDeleted == false || c.IsDeleted == null), pageSize: pageSize, currentPage: currentPage);

            var getCategoryRequestDTOs = Mapper.Map<GetCategoryRequestDTO, Category>(categories);

            return getCategoryRequestDTOs;

        }
        #endregion 

        #region Update Category
        public async Task UpdateCategoryAsync ( int categoryId, UpdateCategoryRequestDTO dto )
        {
            if ( dto == null ) throw new ArgumentNullException ( nameof ( dto ) );

            var category = await UnitOfWork.readRepository<Category> ( )
                .GetAsync ( c => c.Id == categoryId && ( c.IsDeleted == false || c.IsDeleted == null ) );

            if ( category == null ) throw new KeyNotFoundException ( "Category not found." );

            // Check Duplicate Name (excluding self)
            var duplicateCheck = await UnitOfWork.readRepository<Category> ( )
                .GetAsync ( c => c.Name == dto.Name && c.Id != categoryId && ( c.IsDeleted == false || c.IsDeleted == null ) );

            if ( duplicateCheck != null )
                throw new InvalidOperationException ( $"Category name '{dto.Name}' is already taken." );

            // Validate New Parent (Prevent Circular Dependency is complex, but check existence at least)
            if ( dto.ParentCategoryId.HasValue && dto.ParentCategoryId != category.ParentCategoryId )
            {
                if ( dto.ParentCategoryId == category.Id )
                    throw new InvalidOperationException ( "Category cannot be its own parent." );

                var parent = await UnitOfWork.readRepository<Category> ( )
                    .GetAsync ( c => c.Id == dto.ParentCategoryId );
                if ( parent == null ) throw new KeyNotFoundException ( "New Parent Category not found." );
            }

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.ParentCategoryId = dto.ParentCategoryId; // Allow updating parent
            category.UpdatedDate = DateTime.Now;

            await UnitOfWork.writeRepository<Category> ( ).UpdateAsync ( category.Id, category );
            await UnitOfWork.SaveChangeAsync ( );
        }
        #endregion

        #region Delete Category (Improved Logic)
        public async Task DeleteCategoryAsync ( int categoryId )
        {
            var category = await UnitOfWork.readRepository<Category> ( )
                .GetAsync ( c => c.Id == categoryId && ( c.IsDeleted == false || c.IsDeleted == null ) );

            if ( category == null ) throw new KeyNotFoundException ( "Category not found." );

            // 1. Check Assets Dependency
            var hasAssets = await UnitOfWork.readRepository<Asset> ( )
                .CountAsync ( a => a.CategoryId == categoryId && ( a.IsDeleted == false || a.IsDeleted == null ) );

            if ( hasAssets > 0 )
                throw new InvalidOperationException ( "Cannot delete: There are Assets assigned to this category." );

            // 2. Check SubCategories Dependency (Prevent Orphan Records)
            var hasSubCategories = await UnitOfWork.readRepository<Category> ( )
                .CountAsync ( c => c.ParentCategoryId == categoryId && ( c.IsDeleted == false || c.IsDeleted == null ) );

            if ( hasSubCategories > 0 )
                throw new InvalidOperationException ( "Cannot delete: This category contains Sub-Categories. Please delete or move them first." );

            // Soft Delete
            category.IsDeleted = true;
            category.DeletedDate = DateTime.Now;

            await UnitOfWork.writeRepository<Category> ( ).UpdateAsync ( categoryId, category );
            await UnitOfWork.SaveChangeAsync ( );
        }
        #endregion
    }
}

