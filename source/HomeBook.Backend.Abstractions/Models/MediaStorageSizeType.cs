namespace HomeBook.Backend.Abstractions.Models;

/// <summary>
///
/// </summary>
/// <param name="ScopeKey">the scope-key</param>
/// <param name="ModuleKey">the module key like 'homeobok.kitchen'</param>
/// <param name="ModuleName">the module name</param>
/// <param name="StorageSizeBytes">the size of alle files in this storage</param>
public record MediaStorageSizeType(
    string ScopeKey,
    string ModuleKey,
    string ModuleName,
    long StorageSizeBytes);
