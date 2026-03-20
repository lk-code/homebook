using HomeBook.Backend.Responses;
using HomeBook.Backend.Requests;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomeBook.Backend.Handler;

public static class SystemHandler
{
    public static async Task<IResult> HandleGetSystemStorageInfo(
        [FromServices] ISystemStorageService systemStorageService,
        CancellationToken cancellationToken)
    {
        try
        {
            StorageSize storageInformation = await systemStorageService
                .GetStorageSizeInformationsAsync(cancellationToken);
            long totalSizeBytes = storageInformation.TotalSizeBytes;
            long usedSizeBytes = storageInformation.UsedSizeBytes;
            long freeSizeBytes = storageInformation.FreeSizeBytes;

            List<MediaStorageSizeType> storageByType = await systemStorageService
                .GetStorageSizeTypeAsync(cancellationToken);
            MediaStorageSizeTypeResponse[]? mediaStorageSizes = (storageByType ?? [])
                .Select(x =>
                    new MediaStorageSizeTypeResponse(
                        x.ScopeKey,
                        x.ModuleKey,
                        x.ModuleName,
                        x.StorageSizeBytes))
                .ToArray();

            GetSystemStorageInfoResponse response = new(totalSizeBytes,
                usedSizeBytes,
                freeSizeBytes,
                mediaStorageSizes);
            return TypedResults.Ok(response);
        }
        catch (Exception)
        {
            return TypedResults.Problem("An error occurred while retrieving system storage informations.",
                statusCode: 500);
        }
    }

    public static IResult HandleGetSystemInfo([FromServices] IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        try
        {
            string dotnetVersion = Environment.Version.ToString();
            string appVersion = configuration["Version"] ?? "Unknown";
            string databaseProvider = configuration["Database:Provider"] ?? "Unknown";
            string deploymentType = "Docker";

            GetSystemInfoResponse response = new(dotnetVersion,
                appVersion,
                databaseProvider,
                deploymentType);
            return TypedResults.Ok(response);
        }
        catch (Exception)
        {
            return TypedResults.Problem("An error occurred while retrieving system information.", statusCode: 500);
        }
    }

    public static async Task<IResult> HandleGetUsers([FromServices] IUserRepository userRepository,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? username = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                page = 1;
            if (pageSize < 1 || pageSize > 100)
                pageSize = 10;

            // Get all users from repository and materialize to avoid multiple enumeration
            List<User> allUsers = (await userRepository.GetAllAsync(cancellationToken)).ToList();

            // Apply username filter if provided
            if (!string.IsNullOrWhiteSpace(username))
            {
                allUsers = allUsers.Where(u => u.Username.Contains(username, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            int totalCount = allUsers.Count;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Apply pagination
            IEnumerable<User> paginatedUsers = allUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            // Map to response models
            IEnumerable<UserResponse> userResponses = paginatedUsers.Select(user => new UserResponse(
                user.Id,
                user.Username,
                user.Created,
                user.Disabled,
                user.IsAdmin));

            UsersResponse response = new(userResponses, totalCount, page, pageSize, totalPages);
            return TypedResults.Ok(response);
        }
        catch (Exception)
        {
            return TypedResults.Problem("An error occurred while retrieving users.", statusCode: 500);
        }
    }

    public static async Task<IResult> HandleGetUserById(Guid userId,
        [FromServices] IUserRepository userRepository,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user exists
            User? user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return TypedResults.NotFound("User not found");
            }

            // Map to response model
            UserResponse userResponse = new(
                user.Id,
                user.Username,
                user.Created,
                user.Disabled,
                user.IsAdmin);

            return TypedResults.Ok(userResponse);
        }
        catch (Exception)
        {
            return TypedResults.Problem("An error occurred while retrieving the user.", statusCode: 500);
        }
    }

    public static async Task<IResult> HandleCreateUser([FromServices] IUserRepository userRepository,
        [FromServices] IHashProviderFactory hashProviderFactory,
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return TypedResults.BadRequest("Username is required");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return TypedResults.BadRequest("Password is required");
            }

            if (request.Username.Length < 5 || request.Username.Length > 20)
            {
                return TypedResults.BadRequest("Username must be between 5 and 20 characters");
            }

            if (request.Password.Length < 8)
            {
                return TypedResults.BadRequest("Password must be at least 8 characters long");
            }

            // Check if username already exists
            bool userExists = await userRepository.ContainsUserAsync(request.Username, cancellationToken);
            if (userExists)
            {
                return TypedResults.BadRequest("Username already exists");
            }

            // Hash the password
            IHashProvider hashProvider = hashProviderFactory.CreateDefault();
            string passwordHash = hashProvider.Hash(request.Password);

            // Create new user entity
            User newUser = new()
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                PasswordHashType = hashProvider.AlgorithmName,
                IsAdmin = request.IsAdmin,
                Created = DateTime.UtcNow
            };

            // Save user to database
            User createdUser = await userRepository.CreateUserAsync(newUser, cancellationToken);

            // Return the new user ID
            CreateUserResponse response = new(createdUser.Id);
            return TypedResults.Ok(response);
        }
        catch (Exception)
        {
            return TypedResults.Problem("An error occurred while creating the user.", statusCode: 500);
        }
    }

    public static async Task<IResult> HandleDeleteUser([FromServices] IUserRepository userRepository,
        Guid userId,
        [FromServices] IJwtService jwtService,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current user ID from JWT token
            string? authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return TypedResults.Unauthorized();
            }

            string token = authHeader.Substring("Bearer ".Length).Trim();
            Guid? currentUserId = jwtService.GetUserIdFromToken(token);

            if (currentUserId == null)
            {
                return TypedResults.Unauthorized();
            }

            // Check if user is trying to delete themselves
            if (currentUserId == userId)
            {
                return TypedResults.BadRequest("You cannot delete your own account");
            }

            // Check if user to delete exists
            User? userToDelete = await userRepository.GetUserByIdAsync(userId, cancellationToken);
            if (userToDelete == null)
            {
                return TypedResults.NotFound("User not found");
            }

            // Actually delete the user from the database
            bool deleteResult = await userRepository.DeleteAsync(userId, cancellationToken);

            if (!deleteResult)
            {
                return TypedResults.Problem("Failed to delete user", statusCode: 500);
            }

            return TypedResults.Ok("User deleted successfully");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An error occurred while deleting the user.", statusCode: 500);
        }
    }

    public static async Task<IResult> HandleUpdatePassword([FromServices] IUserRepository userRepository,
        [FromServices] IHashProviderFactory hashProviderFactory,
        Guid userId,
        [FromBody] UpdatePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return TypedResults.BadRequest("Password is required");
            }

            if (request.NewPassword.Length < 8)
            {
                return TypedResults.BadRequest("Password must be at least 8 characters long");
            }

            // Check if user exists
            User? user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return TypedResults.NotFound("User not found");
            }

            // Hash the new password
            IHashProvider hashProvider = hashProviderFactory.CreateDefault();
            string passwordHash = hashProvider.Hash(request.NewPassword);

            // Update user password
            user.PasswordHash = passwordHash;
            user.PasswordHashType = hashProvider.AlgorithmName;

            await userRepository.UpdateUserAsync(user, cancellationToken);

            return TypedResults.Ok("Password updated successfully");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An error occurred while updating the password.", statusCode: 500);
        }
    }

    public static async Task<IResult> HandleUpdateUserAdmin([FromServices] IUserRepository userRepository,
        [FromServices] IJwtService jwtService,
        Guid userId,
        [FromBody] UpdateUserAdminRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current user ID from JWT token
            string? authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return TypedResults.Unauthorized();
            }

            string token = authHeader.Substring("Bearer ".Length).Trim();
            Guid? currentUserId = jwtService.GetUserIdFromToken(token);

            if (currentUserId == null)
            {
                return TypedResults.Unauthorized();
            }

            // Check if user is trying to change their own admin status
            if (currentUserId == userId)
            {
                return TypedResults.BadRequest("You cannot change your own admin status");
            }

            // Check if target user exists
            User? user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return TypedResults.NotFound("User not found");
            }

            // Update user admin status
            user.IsAdmin = request.IsAdmin;

            await userRepository.UpdateUserAsync(user, cancellationToken);

            return TypedResults.Ok($"User admin status updated successfully to {request.IsAdmin}");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An error occurred while updating admin status.", statusCode: 500);
        }
    }

    public static async Task<IResult> HandleEnableUser([FromServices] IUserRepository userRepository,
        [FromServices] IJwtService jwtService,
        [FromRoute] Guid userId,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current user ID from JWT token
            string? authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return TypedResults.Unauthorized();
            }

            string token = authHeader.Substring("Bearer ".Length).Trim();
            Guid? currentUserId = jwtService.GetUserIdFromToken(token);

            if (currentUserId == null)
            {
                return TypedResults.Unauthorized();
            }

            // Check if user is trying to enable themselves
            if (currentUserId == userId)
            {
                return TypedResults.BadRequest("You cannot enable your own account");
            }

            // Check if target user exists
            User? user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return TypedResults.NotFound("User not found");
            }

            // Check if user is already enabled
            if (!user.Disabled.HasValue)
            {
                return TypedResults.BadRequest("User is already enabled");
            }

            // Enable user by clearing the Disabled timestamp
            await userRepository.UpdateAsync(userId,
                u => u.Disabled = null,
                cancellationToken);

            return TypedResults.Ok("User enabled successfully");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An error occurred while enabling the user.", statusCode: 500);
        }
    }

    public static async Task<IResult> HandleDisableUser([FromServices] IUserRepository userRepository,
        [FromServices] IJwtService jwtService,
        [FromRoute] Guid userId,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current user ID from JWT token
            string? authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
                return TypedResults.Unauthorized();

            string token = authHeader.Substring("Bearer ".Length).Trim();
            Guid? currentUserId = jwtService.GetUserIdFromToken(token);

            if (currentUserId == null)
                return TypedResults.Unauthorized();

            // Check if user is trying to disable themselves
            if (currentUserId == userId)
                return TypedResults.BadRequest("You cannot disable your own account");

            // Check if target user exists
            User? user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
                return TypedResults.NotFound("User not found");

            // Check if user is already disabled
            if (user.Disabled.HasValue)
                return TypedResults.BadRequest("User is already disabled");

            // Disable user by setting the Disabled timestamp
            await userRepository.UpdateAsync(userId,
                u => u.Disabled = DateTime.UtcNow,
                cancellationToken);

            return TypedResults.Ok("User disabled successfully");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An error occurred while disabling the user.", statusCode: 500);
        }
    }

    public static async Task<IResult> HandleUpdateUsername([FromServices] IUserRepository userRepository,
        Guid userId,
        [FromBody] UpdateUsernameRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.NewUsername))
                return TypedResults.BadRequest("New username is required");

            if (request.NewUsername.Length is < 5 or > 20)
                return TypedResults.BadRequest("Username must be between 5 and 20 characters");

            // Check if username already exists (case-insensitive)
            bool usernameExists = await userRepository.ContainsUserAsync(request.NewUsername, cancellationToken);
            if (usernameExists)
                return TypedResults.Conflict("Username already exists");

            // Retrieve the user to update
            User? user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
                return TypedResults.NotFound("User not found");

            // Update the username
            user.Username = request.NewUsername;
            await userRepository.UpdateUserAsync(user, cancellationToken);

            return TypedResults.Ok("Username updated successfully");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An error occurred while updating the username", statusCode: 500);
        }
    }

    public static async Task<IResult> HandleUpdateInstanceName(
        [FromServices] IInstanceConfigurationProvider instanceConfigurationProvider,
        [FromBody] UpdateInstanceNameRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Name))
                return TypedResults.BadRequest("Instance name is required and cannot be empty");

            // Update the instance name
            await instanceConfigurationProvider.SetHomeBookInstanceNameAsync(request.Name,
                cancellationToken);

            return TypedResults.Ok("Instance name updated successfully");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An error occurred while updating the instance name", statusCode: 500);
        }
    }

    public static async Task<IResult> HandleUpdateInstanceDefaultLocale(
        [FromServices] IInstanceConfigurationProvider instanceConfigurationProvider,
        [FromBody] UpdateInstanceDefaultLocaleRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Locale))
                return TypedResults.BadRequest("Locale is required and cannot be empty");

            // Update the instance name
            await instanceConfigurationProvider.SetHomeBookInstanceDefaultLocaleAsync(request.Locale,
                cancellationToken);

            return TypedResults.Ok("Default locale updated successfully");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An error occurred while updating the default locale", statusCode: 500);
        }
    }
}
