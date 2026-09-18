namespace HRM.Application.Common;

/// <summary>
/// Owns SaveChanges for every service, and — for the 2 flows that write
/// across more than one repository (CreateReviewPeriod, ApplyDecision) —
/// transaction control, matching the `UOW: HrmDbContext` participant in
/// SequenceDiagrams.md. Implemented in HRM.Infrastructure wrapping
/// HrmDbContext, so Application never references Infrastructure directly.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
