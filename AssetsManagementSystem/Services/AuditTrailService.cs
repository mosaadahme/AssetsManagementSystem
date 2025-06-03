namespace AssetsManagementSystem.Services
{
    public class AuditTrailService : BaseClassForServices
    {
        public AuditTrailService(IUnitOfWork unitOfWork,
            Others.Interfaces.IAutoMapper.IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
            : base(unitOfWork, mapper, httpContextAccessor)
        {
        }
        public async Task<IList<AuditTrail>> GetAuditTrails() 
        {
            var result= await UnitOfWork.readRepository<AuditTrail>().GetAllAsync(); 
            return result.ToList();
                

        }
    }
}
