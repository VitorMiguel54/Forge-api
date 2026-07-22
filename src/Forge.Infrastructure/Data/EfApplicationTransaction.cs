using Forge.Application.Interfaces;

namespace Forge.Infrastructure.Data;

public class EfApplicationTransaction(ForgeDbContext dbContext) : IApplicationTransaction
{
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
