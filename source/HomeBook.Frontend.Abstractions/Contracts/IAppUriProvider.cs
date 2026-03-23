namespace HomeBook.Frontend.Abstractions.Contracts;

/// <summary>
///
/// </summary>
public interface IAppUriProvider
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="relativeUri"></param>
    /// <returns></returns>
    Uri GetAbsoluteUri(string relativeUri);
}
