using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace TelegramBot.Importer.Extensions;

[ExcludeFromCodeCoverage]
public static class DataReaderExtension
{
    [DebuggerStepThrough]
    public static string ToString(this IDataReader reader, string name)
    {
        if (reader[name] != DBNull.Value)
        {
            return reader[name].ToString();
        }

        return string.Empty;
    }

    [DebuggerStepThrough]
    public static bool ToBool(this IDataReader reader, string name)
    {
        return ToBool(reader, name, false);
    }

    [DebuggerStepThrough]
    public static bool ToBool(this IDataReader reader, string name, bool defaultValue)
    {
        if (reader[name] != DBNull.Value)
        {
            bool check = bool.TryParse(reader[name].ToString(), out bool result);
            if (check)
            {
                return result;
            }
        }

        return defaultValue;
    }

    [DebuggerStepThrough]
    public static int ToInt(this IDataReader reader, string name)
    {
        if (reader[name] != DBNull.Value)
        {
            bool check = int.TryParse(reader[name].ToString(), out int result);
            if (check)
            {
                return result;
            }
        }

        return 0;
    }

    [DebuggerStepThrough]
    public static DateTime ToDateTime(this IDataReader reader, string name)
    {
        if (reader[name] != DBNull.Value)
        {
            bool check = DateTime.TryParse(reader[name].ToString(), out DateTime result);
            if (check)
            {
                return result;
            }
        }

        return DateTime.MinValue;
    }

}