namespace Common
{
    #region Using directives

    using System.Text.Json;

    #endregion

    /// <summary>
    ///     The extension methods for converting objects to/from JSON.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     Converts <paramref name="obj" /> to <see cref="string" /> containing JSON.
        /// </summary>
        public static string ToJson(this object obj) =>
            obj != null
                ? JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true })
                : string.Empty;

        /// <summary>
        ///     Converts <paramref name="obj" /> to <see cref="byte[]" /> containing JSON.
        /// </summary>
        public static byte[] ToJsonBytes(this object obj) =>
            obj != null
                ? JsonSerializer.SerializeToUtf8Bytes(obj, new JsonSerializerOptions { WriteIndented = true })
                : null;

        /// <summary>
        ///     Converts <paramref name="json" /> to strongly typed object specified by <typeparamref name="T" />.
        /// </summary>
        public static T FromJson<T>(this string json) => JsonSerializer.Deserialize<T>(json);

        /// <summary>
        ///     Converts <paramref name="json" /> to strongly typed object specified by <typeparamref name="T" />.
        /// </summary>
        public static T FromJsonBytes<T>(this byte[] json) => JsonSerializer.Deserialize<T>(json);
    }
}