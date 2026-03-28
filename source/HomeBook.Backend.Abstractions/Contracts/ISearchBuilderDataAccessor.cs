namespace HomeBook.Backend.Abstractions.Contracts;

public interface ISearchBuilderDataAccessor
{
        /// <summary>
        /// get the registered search handlers
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetRegisteredSearchHandlers();
}
