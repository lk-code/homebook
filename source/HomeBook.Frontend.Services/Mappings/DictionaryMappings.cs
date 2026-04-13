using Microsoft.Kiota.Abstractions.Serialization;

namespace HomeBook.Frontend.Services.Mappings;

public static class DictionaryMappings
{
    public static Dictionary<string, List<string>>? MapToDictionary(this IDictionary<string, object> untypedDictionary)
    {
        if (untypedDictionary is null)
            return null;

        return untypedDictionary.ToDictionary(kvPair => kvPair.Key,
            kvPair => (kvPair.Value as UntypedArray)?.GetValue()
                .Select(x => (x as UntypedString)?.GetValue())
                .ToList());
    }
}
