namespace HomeBook.Backend.Modules.Abstractions;

public interface ISearchBuilder
{
    void RegisterHandler<ISearchHandler>();
}
