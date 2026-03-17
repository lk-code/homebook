namespace HomeBook.Backend.Modules.Abstractions;

public interface IStorageBuilder
{
    IStorageBuilder RegisterStorage(string storageScopeName);
}
