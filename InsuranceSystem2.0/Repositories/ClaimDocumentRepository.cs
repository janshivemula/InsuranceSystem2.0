using InsuranceSystem2._0.DTOs.Claim;
using InsuranceSystem2._0.Repositories.Interfaces;
using InsuranceSystem2._0.Data;
using InsuranceSystem2._0.Models;
using Microsoft.EntityFrameworkCore;

namespace InsuranceSystem2._0.Repositories.Implementations
{
    public class ClaimDocumentRepository : IClaimDocumentRepository
    {
        private readonly AppDbContext _context;

        // Constructor for Dependency Injection
        public ClaimDocumentRepository(AppDbContext context)
        {
            _context = context;
        }

        // Select claim document details and related claim data.
        private IQueryable<ClaimDocument> GetClaimDocumentQuery()
        {
            return _context.ClaimDocuments.Select(cd => new ClaimDocument
            {
                DocumentId = cd.DocumentId,
                ClaimId = cd.ClaimId,

                // Include the document's own properties.
                DocumentName = cd.DocumentName,
                DocumentType = cd.DocumentType,
                DocumentReference = cd.DocumentReference,
                UploadedDate = cd.UploadedDate,

                // Select required claim properties.
                Claim = cd.Claim == null ? null : new Claim
                {
                    ClaimId = cd.Claim.ClaimId,
                    ClaimNumber = cd.Claim.ClaimNumber,
                    ClaimStatus = cd.Claim.ClaimStatus,
                    CustomerId = cd.Claim.CustomerId,
                    PolicyId = cd.Claim.PolicyId,
                    CreatedDate = cd.Claim.CreatedDate
                }
            });
        }

        // Retrieves all claim documents.
        public async Task<IEnumerable<ClaimDocument>> GetAllAsync()
        {
            return await GetClaimDocumentQuery()
                .ToListAsync();
        }

        // Retrieves all documents for a specific claim.
        public async Task<IEnumerable<ClaimDocument>> GetByClaimIdAsync(
            int claimId)
        {
            return await GetClaimDocumentQuery()
                .Where(cd => cd.ClaimId == claimId)
                .ToListAsync();
        }

        // Retrieves a claim document by its unique identifier.
        public async Task<ClaimDocument?> GetByIdAsync(int id)
        {
            return await GetClaimDocumentQuery()
                .FirstOrDefaultAsync(cd => cd.DocumentId == id);
        }

        // Adds a new claim document.
        public async Task AddAsync(ClaimDocument claimDocument)
        {
            await _context.ClaimDocuments.AddAsync(claimDocument);
        }

        // Saves all pending changes to the database.
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
