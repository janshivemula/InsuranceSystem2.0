using InsuranceSystem2._0.Repositories.Interfaces;
using InsuranceSystem2._0.Data;
using InsuranceSystem2._0.Models;
using Microsoft.EntityFrameworkCore;

namespace InsuranceSystem2._0.Repositories.Implementations
{
    public class ClaimStatusHistoryRepository : IClaimStatusHistoryRepository
    {
        private readonly AppDbContext _context;

        public ClaimStatusHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        // Select history details and related claim/user data.

        private IQueryable<ClaimStatusHistory> GetHistoryQuery()
        {
            return _context.ClaimStatusHistories
                .AsNoTracking()
                .Select(h => new ClaimStatusHistory
                {
                    HistoryId = h.HistoryId,
                    ClaimId = h.ClaimId,
                    PreviousStatus = h.PreviousStatus,
                    NewStatus = h.NewStatus,
                    Remarks = h.Remarks,
                    UpdatedBy = h.UpdatedBy,
                    UpdatedDate = h.UpdatedDate,

                    Claim = h.Claim == null ? null! : new Claim
                    {
                        ClaimId = h.Claim.ClaimId,
                        ClaimNumber = h.Claim.ClaimNumber,
                        ClaimStatus = h.Claim.ClaimStatus,
                        CustomerId = h.Claim.CustomerId,
                        PolicyId = h.Claim.PolicyId,
                        CreatedDate = h.Claim.CreatedDate
                    },

                    User = h.User == null ? null! : new User
                    {
                        UserId = h.User.UserId,
                        FullName=h.User.FullName,
                        Email = h.User.Email
                    }
                });
        }


        // Retrieves all claim status history records.
        public async Task<IEnumerable<ClaimStatusHistory>> GetAllAsync()
        {
            return await GetHistoryQuery()
                .OrderByDescending(h => h.UpdatedDate)
                .ToListAsync();
        }

        // Retrieves a history record by HistoryId.
        public async Task<ClaimStatusHistory?> GetByIdAsync(int historyId)
        {
            return await GetHistoryQuery()
                .FirstOrDefaultAsync(h => h.HistoryId == historyId);
        }

        // Retrieves history records for a specific claim.
        public async Task<IEnumerable<ClaimStatusHistory>> GetByClaimIdAsync(
            int claimId)
        {
            return await GetHistoryQuery()
                .Where(h => h.ClaimId == claimId)
                .OrderByDescending(h => h.UpdatedDate)
                .ToListAsync();
        }

        // Adds a new claim status history record.
        public async Task AddAsync(ClaimStatusHistory claimStatusHistory)
        {
            await _context.ClaimStatusHistories.AddAsync(claimStatusHistory);
        }

        // Saves all pending changes to the database.
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
