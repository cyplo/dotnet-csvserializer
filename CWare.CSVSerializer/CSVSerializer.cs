using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CWare
{
    public interface CSVSerializable { }

    [AttributeUsage(AttributeTargets.Property)]
    public class CSVSerializablePropertyAttribute : Attribute { }

    public class CSVAttributeBasedSerializer<T> where T : CSVSerializable
    {

        private IEnumerable<T> _items;
        private Predicate<PropertyInfo> _predicate;


        public CSVAttributeBasedSerializer(IEnumerable<T> items, Predicate<PropertyInfo> predicate = null)
        {
            _items = items;
            if (predicate == null)
            {
                predicate = DefaultSerializationDecisionPredicate;
            }
            _predicate = predicate;
        }

        private bool DefaultSerializationDecisionPredicate(PropertyInfo property)
        {
            return property.GetCustomAttributes(typeof(CSVSerializablePropertyAttribute), true).Any();
        }

        private readonly static char CELL_SEPARATOR = ';';
        private readonly static char COLLECTION_ITEMS_SEPARATOR = ',';

        

        private string SerializeProperty( T item, PropertyInfo property)
        {
            var result = "";
            //detect collection
            if (property.GetType() is IEnumerable)
            {
        
                var collection = property.GetValue(item, null) as IEnumerable;

                var enumerator = collection.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    result += enumerator.Current.ToString();
                }

                while (enumerator.MoveNext())
                {
                    result += COLLECTION_ITEMS_SEPARATOR+enumerator.Current.ToString();
                }
            }
            return result;
        }

        private string SerializeItem(T item)
        {
            var properties = from property in typeof(T).GetProperties()
                             where _predicate(property)
                             select property;

            var result = "";

            if (!properties.Any()) { return result; }

            result += SerializeProperty(item, properties.First());

            for (int i = 1; i < properties.Count(); ++i )
            {
                var property = properties.ElementAt(i);
                result += CELL_SEPARATOR+SerializeProperty(item, property);
            }
            return result;
        }

        public string Serialize()
        {
            var result = "";

            foreach (var item in _items)
            {
                result += SerializeItem(item) + Environment.NewLine;
            }
            return result;
        }

    }
}
