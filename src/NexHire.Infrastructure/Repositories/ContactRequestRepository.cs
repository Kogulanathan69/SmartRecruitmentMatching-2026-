using Microsoft.EntityFrameworkCore;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexHire.Infrastructure.Repositories;

public class ContactRequestRepository : IContactRequestRepository
{
    private readonly AppDbContext _context;

    public ContactRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    private IQueryable<ContactRequest> WithDetails()
    {
        return _context.ContactRequests
            .Include(x => x.JobApplication);
    }

    public Task<ContactRequest?> GetByIdWithDetailsAsync(Guid id)
    {
        return WithDetails()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<ContactRequest?> GetByApplicationIdAsync(Guid applicationId)
    {
        return WithDetails()
            .FirstOrDefaultAsync(x =>
                x.JobApplicationId == applicationId);
    }

    public async Task<IReadOnlyList<ContactRequest>>
        GetByEmployerUserIdAsync(Guid employerUserId)
    {
        return await WithDetails()
            .Where(x => x.EmployerUserId == employerUserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ContactRequest>>
        GetByCandidateUserIdAsync(Guid candidateUserId)
    {
        return await WithDetails()
            .Where(x => x.CandidateUserId == candidateUserId)
            .OrderBy(x =>
                x.Status == NexHire.Domain.Enums.ContactRequestStatus.Pending
                    ? 0
                    : 1)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task AddAsync(ContactRequest request)
    {
        await _context.ContactRequests.AddAsync(request);
    }

    public void Update(ContactRequest request)
    {
        _context.ContactRequests.Update(request);
    }
}