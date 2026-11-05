using System.Data;
using Microsoft.Data.SqlClient;

namespace AEPW.Infrastructure.Helpers;

/// <summary>
/// Extension methods that reduce ADO.NET boilerplate when working with SqlCommand.
/// </summary>
public static class SqlCommandExtensions
{
    /// <summary>Adds a non-null input parameter.</summary>
    public static SqlParameter AddInputParam(this SqlCommand cmd, string name, SqlDbType type, object value)
    {
        var p = cmd.Parameters.Add(name, type);
        p.Direction = ParameterDirection.Input;
        p.Value     = value;
        return p;
    }

    /// <summary>Adds an input parameter that maps <c>null</c> to DBNull.</summary>
    public static SqlParameter AddInputParamNullable(this SqlCommand cmd, string name, SqlDbType type, object? value)
    {
        var p = cmd.Parameters.Add(name, type);
        p.Direction = ParameterDirection.Input;
        p.Value     = value ?? (object)DBNull.Value;
        return p;
    }

    /// <summary>Adds an OUTPUT parameter and returns it so the caller can read the value.</summary>
    public static SqlParameter AddOutputParam(this SqlCommand cmd, string name, SqlDbType type)
    {
        var p = cmd.Parameters.Add(name, type);
        p.Direction = ParameterDirection.Output;
        return p;
    }
}
