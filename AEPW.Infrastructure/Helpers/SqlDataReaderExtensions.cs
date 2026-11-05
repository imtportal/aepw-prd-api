using System.Data;
using Microsoft.Data.SqlClient;

namespace AEPW.Infrastructure.Helpers;

/// <summary>
/// Safe reader helpers — returns default when column is DBNull.
/// </summary>
public static class SqlDataReaderExtensions
{
    public static string GetStringSafe(this SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    public static int GetInt32Safe(this SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
    }

    public static int? GetInt32Nullable(this SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    public static bool GetBooleanSafe(this SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
    }

    public static DateTime GetDateTimeSafe(this SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? DateTime.MinValue : reader.GetDateTime(ordinal);
    }

    public static DateTime? GetDateTimeNullable(this SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    public static T GetEnumSafe<T>(this SqlDataReader reader, string column) where T : struct, Enum
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? default : (T)Enum.ToObject(typeof(T), reader.GetInt32(ordinal));
    }
}
