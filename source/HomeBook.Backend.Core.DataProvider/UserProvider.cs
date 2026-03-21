using FluentValidation;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Exceptions;
using HomeBook.Backend.Abstractions.Models.UserManagement;
using HomeBook.Backend.Core.DataProvider.Mappings;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Core.DataProvider;

/// <inheritdoc />
public class UserProvider(
    IUserRepository userRepository,
    IHashProviderFactory hashProviderFactory,
    IValidator<User> userValidator,
    ILogger<UserProvider> logger) : IUserProvider
{
    /// <inheritdoc />
    public async Task<Guid> CreateUserAsync(string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating user");

        bool userExists = await userRepository.ContainsUserAsync(username, cancellationToken);
        if (userExists)
            throw new UserAlreadyExistsException($"User with username '{username}' already exists.");

        IHashProvider hashProvider = hashProviderFactory.CreateDefault();
        string hashAlgorithm = hashProvider.AlgorithmName;
        string hashedPassword = hashProvider.Hash(password);
        User user = new()
        {
            Username = username,
            PasswordHashType = hashAlgorithm,
            PasswordHash = hashedPassword,
        };

        await userValidator.ValidateAndThrowAsync(user, cancellationToken);

        User createdUser = await userRepository.CreateUserAsync(user, cancellationToken);
        return createdUser.Id;
    }

    /// <inheritdoc />
    public async Task<bool> ContainsUserAsync(string username, CancellationToken cancellationToken = default) =>
        await userRepository.ContainsUserAsync(username, cancellationToken);

    /// <inheritdoc />
    public async Task<IEnumerable<UserInfo>> GetAllAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all users");

        IEnumerable<User> userEntities = await userRepository.GetAllAsync(cancellationToken);
        IEnumerable<UserInfo> userInfos = userEntities.Select(x => x.ToUserInfo());
        return userInfos;
    }

    /// <inheritdoc />
    public async Task UpdateUserAsync(UserInfo userInfo,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating user");

        User user = await userRepository.GetUserByIdAsync(userInfo.Id, cancellationToken)
                    ?? throw new KeyNotFoundException($"User with id '{userInfo.Id}' not found.");
        user = user.Update(userInfo);

        await userValidator.ValidateAndThrowAsync(user, cancellationToken);

        await userRepository.UpdateUserAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAdminFlag(Guid userId,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating user admin flag");

        User? user = userRepository.GetUserByIdAsync(userId, cancellationToken)
            .GetAwaiter()
            .GetResult();
        if (user is null)
            throw new KeyNotFoundException($"User with id '{userId}' not found.");

        return userRepository.UpdateAsync(userId,
            x =>
            {
                x.IsAdmin = true;
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserInfo?> GetUserByIdAsync(Guid userId,
        CancellationToken cancellationToken) =>
        (await userRepository.GetUserByIdAsync(userId,
            cancellationToken))?.ToUserInfo();
}
