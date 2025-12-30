using Microsoft.Extensions.Caching.Memory;

namespace OsuTaikoDaniDojo.Application.Utility;

public static class MemoryCacheExtension
{
    extension(IMemoryCache memoryCache)
    {
        public void SetTyped<T>(object key, T value) => memoryCache.Set(GetTypedKey<T>(key), value);

        public void SetTyped<T>(object key, T value, TimeSpan lifetime)
            => memoryCache.Set(GetTypedKey<T>(key), value, lifetime);

        public bool TryGetTyped<T>(object key, out T? value) => memoryCache.TryGetValue(GetTypedKey<T>(key), out value);
        public void RemoveTyped<T>(object key) => memoryCache.Remove(GetTypedKey<T>(key));
    }

    private static string GetTypedKey<T>(object key) => $"{typeof(T).FullName}:{key}";
}
