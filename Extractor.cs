using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace shookayNET
{
    public static class Extractor
    {
        public static HashSet<string> GetWords(string wyrazenie)
        {
            Console.WriteLine($"dodaje {wyrazenie}");
            if (string.IsNullOrWhiteSpace(wyrazenie))
            {
                return new HashSet<string>();
            }
            var przecinki = new List<char> { ' ', '-', '.', '/', '\n', '\r' }.ToArray();
            var wszystkieslowa = wyrazenie.Split(przecinki);
            return new HashSet<string>(wszystkieslowa.Select(w => w.ToLower().Trim()));
        }

        public static KeyValuePair<int, string> WordExtractor<T>(T item, string? idname = null) where T : class
        {

            ArgumentNullException.ThrowIfNull(item);
            HashSet<string> resultStrings = new();
            int id = 0;
            bool idFound = false;
            bool multipleIdProperties = false;

            ExtractProperties(item, ref resultStrings, ref id, ref idFound, ref multipleIdProperties, idname, true);

            if (multipleIdProperties)
            {
                throw new InvalidOperationException("More than one potential ID property found.");
            }

            if (!idFound)
            {
                throw new InvalidOperationException("ID property not found.");
            }

            return new KeyValuePair<int, string>(id, string.Join(" ", resultStrings));
        }

        private static void ExtractProperties<T>(T item, ref HashSet<string> resultStrings, ref int id, ref bool idFound, ref bool multipleIdProperties, string? idname, bool isRootObject) where T : class
        {
            Console.WriteLine($"Analyzing properties of object: {item.GetType().Name}");

            foreach (PropertyInfo prop in item.GetType().GetProperties())
            {
                Console.WriteLine($"Checking property: {prop.Name}, Type: {prop.PropertyType.Name}");

                // Ignoring virtual properties (except strings)
                if (prop.GetGetMethod().IsVirtual && prop.PropertyType != typeof(string))
                {
                    Console.WriteLine($"Ignoring virtual property: {prop.Name}");
                    continue;
                }

                if (prop.PropertyType == typeof(string))
                {
                    string value = (string)prop.GetValue(item);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        Console.WriteLine($"Adding string property: {prop.Name}, Value: {value}");
                        resultStrings.UnionWith(GetWords(value));
                    }
                }
                else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                {
                    Console.WriteLine($"Processing collection property: {prop.Name}");
                    var enumerable = (System.Collections.IEnumerable)prop.GetValue(item);
                    foreach (var element in enumerable)
                    {
                        if (element.GetType().IsClass)
                        {
                            Console.WriteLine($"Processing element of type: {element.GetType().Name} in collection {prop.Name}");
                            ExtractProperties(element, ref resultStrings, ref id, ref idFound, ref multipleIdProperties, idname, false);
                        }
                    }
                }
                else if (!prop.PropertyType.IsPrimitive && prop.PropertyType != typeof(DateTime) && prop.PropertyType != typeof(string) && prop.GetValue(item) != null)
                {
                    Console.WriteLine($"Processing nested object property: {prop.Name}");
                    ExtractProperties(prop.GetValue(item), ref resultStrings, ref id, ref idFound, ref multipleIdProperties, idname, false);
                }

                if (!idFound && isRootObject && (prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)
                                || prop.Name.Equals(item.GetType().Name + "Id", StringComparison.OrdinalIgnoreCase)
                                || prop.Name.Equals(idname, StringComparison.OrdinalIgnoreCase)))
                {
                    if (prop.PropertyType != typeof(int))
                    {
                        throw new InvalidOperationException("ID property is not an integer.");
                    }

                    if (idFound && isRootObject)
                    {
                        multipleIdProperties = true;
                        Console.WriteLine($"Multiple ID properties found, which is invalid.");
                        return; // No need to continue
                    }

                    id = (int)prop.GetValue(item);
                    idFound = true;
                    Console.WriteLine($"ID property found: {prop.Name}, Value: {id}");
                }
            }

            if (isRootObject && idname != null && !idFound)
            {
                throw new InvalidOperationException($"ID property with name '{idname}' not found.");
            }
        }





    }
}
