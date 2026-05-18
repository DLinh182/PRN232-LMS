using System.Collections;
using System.Reflection;

namespace PRN232.LMS.API.Common;

public static class FieldSelector
{
    public static object ApplyFields(object source, string[] fields)
    {
        if (fields.Length == 0)
            return source;

        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var type = source.GetType();

        foreach (var field in fields)
        {
            var prop = type.GetProperty(
              field,
              BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (prop == null) continue;

            var value = prop.GetValue(source);

            dict[prop.Name] = value;
        }

        return dict;
    }

    public static List<object> ApplyFieldsList<T>(IEnumerable<T> items, string[] fields)
    {
        return items.Select(x => ApplyFields(x!, fields)).ToList();
    }
}