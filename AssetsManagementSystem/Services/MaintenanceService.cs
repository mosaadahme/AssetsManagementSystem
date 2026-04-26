using AssetsManagementSystem.DTOs.AssetMaintenanceDTOs.AssetMaintanceTempDTOs;
using AssetsManagementSystem.Models.DbSets;
using AssetsManagementSystem.Models.DbSets.Temp;
using AssetsManagementSystem.Models.Enums;
using AssetsManagementSystem.Models.Enums.AssetsManagementSystem.Models.Enums;
using AssetsManagementSystem.Services.Assets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssetsManagementSystem.Services.Maintenance
{
    public class MaintenanceService : BaseClassForServices
    {
        private readonly ILogger<MaintenanceService> _logger;

        public MaintenanceService (
            IUnitOfWork unitOfWork,
            Others.Interfaces.IAutoMapper.IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<MaintenanceService> logger )
            : base ( unitOfWork, mapper, httpContextAccessor )
        {
            _logger = logger;
        }

        #region 1. Preventive Maintenance (Time-Based)

        public async Task<int> CreateMaintenancePlanAsync ( CreateMaintenancePlanRequestDTO dto )
        {
            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                var plan = new MaintenancePlan
                {
                    TargetLevel = dto.TargetLevel,
                    TargetId = dto.TargetId,
                    Periodicity = dto.Periodicity,
                    ActionTypeId = dto.ActionTypeId,
                    PreferredVendorId = dto.PreferredVendorId,
                    StartDate = dto.StartDate,
                    IsActive = true
                };

                await UnitOfWork.writeRepository<MaintenancePlan> ( ).AddAsync ( plan );
                await UnitOfWork.SaveChangeAsync ( );

                // توليد أول موعد صيانة
                await GenerateNextSchedule ( plan.Id, plan.StartDate );

                await UnitOfWork.CommitTransactionAsync ( );
                return plan.Id;
            }
            catch ( Exception ex )
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                _logger.LogError ( ex, "Error creating maintenance plan" );
                throw;
            }
        }

        public async Task ExecutePreventiveAsync ( ExecuteMaintenanceDTO dto )
        {
            var schedule = await UnitOfWork.readRepository<MaintenanceSchedule> ( ).GetAsync ( s => s.Id == dto.ScheduleId );
            if ( schedule == null ) throw new KeyNotFoundException ( "Schedule record not found." );

            schedule.Status = dto.Status.ToString ( );
            schedule.ActualExecutionDate = DateTime.Now;
            schedule.Cost = dto.Cost;
            schedule.Notes = dto.Notes;

            await UnitOfWork.writeRepository<MaintenanceSchedule> ( ).UpdateAsync ( schedule.Id, schedule );

            if ( dto.Status == MaintenanceStatus.Done )
            {
                var plan = await UnitOfWork.readRepository<MaintenancePlan> ( ).GetAsync ( p => p.Id == schedule.PlanId );
                DateTime nextDate = CalculateNextDate ( schedule.DueDate, plan.Periodicity );
                await GenerateNextSchedule ( plan.Id, nextDate );
            }
            else if ( dto.Status == MaintenanceStatus.Rescheduled && dto.RescheduledDate.HasValue )
            {
                await GenerateNextSchedule ( schedule.PlanId, dto.RescheduledDate.Value );
            }

            await UnitOfWork.SaveChangeAsync ( );
        }

        #endregion

        #region 2. Corrective Maintenance (Breakdown)

        public async Task<int> SubmitRepairRequestAsync ( SubmitRepairRequestDTO dto )
        {
            var request = new MaintenanceRequest
            {
                AssetId = dto.AssetId,
                RequesterId = UserId ?? "System",
                ProblemDescription = dto.ProblemDescription,
                RequestDate = DateTime.Now,
                Status = RequestStatus.PendingReview,
                PhotoUrl = " ",//dto.PhotoUrl,
                NeedReplacement = dto.NeedReplacement
            };

            await UnitOfWork.writeRepository<MaintenanceRequest> ( ).AddAsync ( request );
            await UnitOfWork.SaveChangeAsync ( );
            return request.Id;
        }

        public async Task ReviewRequestAsync ( ReviewRequestDTO dto )
        {
            var request = await UnitOfWork.readRepository<MaintenanceRequest> ( ).GetAsync ( r => r.Id == dto.RequestId );
            if ( request == null ) throw new KeyNotFoundException ( "Request not found." );

            request.Status = dto.Decision;
            request.MaintenanceNotes = dto.Notes;
            if ( dto.Decision == RequestStatus.Approved )
            {
                request.TechnicianId = dto.TechnicianId;
            }

            await UnitOfWork.writeRepository<MaintenanceRequest> ( ).UpdateAsync ( request.Id, request );
            await UnitOfWork.SaveChangeAsync ( );
        }

        public async Task CloseRepairRequestAsync ( CloseRepairDTO dto )
        {
            var request = await UnitOfWork.readRepository<MaintenanceRequest> ( ).GetAsync ( r => r.Id == dto.RequestId );
            if ( request == null ) throw new KeyNotFoundException ( "Request not found." );

            request.Status = RequestStatus.Completed;
            request.FinalCost = dto.Cost;
            request.MaintenanceNotes = dto.Details;
            request.VendorId = dto.VendorId;
            request.OutDate = dto.OutDate;
            request.ReturnDate = dto.ReturnDate;

            await UnitOfWork.writeRepository<MaintenanceRequest> ( ).UpdateAsync ( request.Id, request );
            await UnitOfWork.SaveChangeAsync ( );
        }

        #endregion

        #region Private Helpers

        private async Task GenerateNextSchedule ( int planId, DateTime dueDate )
        {
            var schedule = new MaintenanceSchedule
            {
                PlanId = planId,
                DueDate = dueDate,
                Status = MaintenanceStatus.Pending.ToString ( )
            };
            await UnitOfWork.writeRepository<MaintenanceSchedule> ( ).AddAsync ( schedule );
            await UnitOfWork.SaveChangeAsync ( );
        }

        private DateTime CalculateNextDate ( DateTime current, Periodicity periodicity )
        {
            return periodicity switch
            {
                Periodicity.Weekly => current.AddDays ( 7 ),
                Periodicity.Monthly => current.AddMonths ( 1 ),
                Periodicity.Quarterly => current.AddMonths ( 3 ),
                Periodicity.SemiAnnually => current.AddMonths ( 6 ),
                Periodicity.Annually => current.AddYears ( 1 ),
                _ => current.AddMonths ( 1 )
            };
        }

        #endregion
    }
}