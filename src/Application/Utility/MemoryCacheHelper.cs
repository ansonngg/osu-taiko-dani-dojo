﻿using Microsoft.Extensions.Caching.Memory;

namespace OsuTaikoDaniDojo.Application.Utility;

public static class MemoryCacheHelper
{
    public static void SetTyped<T>(this IMemoryCache memoryCache, object key, T value)
        => memoryCache.Set(GetTypedKey<T>(key), value);

    public static void SetTyped<T>(this IMemoryCache memoryCache, object key, T value, TimeSpan lifetime)
        => memoryCache.Set(GetTypedKey<T>(key), value, lifetime);

    public static bool TryGetTyped<T>(this IMemoryCache memoryCache, object key, out T? value)
        => memoryCache.TryGetValue(GetTypedKey<T>(key), out value);

    private static string GetTypedKey<T>(object key) => $"{typeof(T).FullName}:{key}";
}
