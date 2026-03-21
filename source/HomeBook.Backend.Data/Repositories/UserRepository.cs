using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Data.Repositories;

/// <inheritdoc />
public class UserRepository(
    ILogger<UserRepository> logger,
    IDbContextFactory<AppDbContext> factory) : IUserRepository
{
    /// <inheritdoc />
    public async Task<User> CreateUserAsync(User user,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating user");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);
        dbContext.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    /// <inheritdoc />
    public async Task<bool> ContainsUserAsync(string username,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Checking whether user exists");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);
        return (await GetUserByUsernameAsync(username, cancellationToken)) is not null;
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByIdAsync(Guid id,
        CancellationToken cancellationToken = default,
        AppDbContext? appDbContext = null)
    {
        logger.LogDebug("Retrieving user");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<User>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByUsernameAsync(string username,
        CancellationToken cancellationToken = default,
        AppDbContext? appDbContext = null)
    {
        logger.LogDebug("Retrieving user by username");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<User>().FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all users from repository");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Users.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User> UpdateUserAsync(User user,
        CancellationToken cancellationToken)
    {
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);
        dbContext.Update(user);

        logger.LogInformation("Persisting user update");

        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    /// <inheritdoc />
    public async Task<User?> UpdateAsync(Guid userId,
        Action<User> updateHandler,
        CancellationToken cancellationToken)
    {
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        User? user = await GetUserByIdAsync(userId, cancellationToken, dbContext);
        if (user is null)
            throw new KeyNotFoundException($"User with id '{userId}' not found.");

        updateHandler(user);

        dbContext.Update(user);

        logger.LogInformation("Persisting user update");

        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        User? user = await GetUserByIdAsync(userId, cancellationToken, dbContext);
        if (user is null)
            return false;

        dbContext.Remove(user);

        logger.LogInformation("Deleting user");

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
