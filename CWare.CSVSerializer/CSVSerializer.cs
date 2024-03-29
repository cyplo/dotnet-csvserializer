﻿using System;
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

    public class CSVSerializer<T> where T : CSVSerializable
    {

        private IEnumerable<T> _items;
        private Predicate<PropertyInfo> _predicate;


        public CSVSerializer(IEnumerable<T> items, Predicate<PropertyInfo> predicate = null)
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
            if (property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)) &&
                property.PropertyType != typeof(string))
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
                return result;
            }
            var value = property.GetValue(item, null);
            
            result += value!=null?value.ToString():"";

            return result;
        }

        private string SerializeItem(T item, IEnumerable<PropertyInfo> properties)
        {
            var result = "";

            

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

            var properties = from property in typeof(T).GetProperties()
                             where _predicate(property)
                             select property;

            if (!properties.Any()) { return result; }

            //headline
            result += properties.First().Name;
            for(int i =1;i<properties.Count();++i)
            {
                var property=properties.ElementAt(i);
                result += CELL_SEPARATOR + property.Name;
            }
            result += Environment.NewLine;

            foreach (var item in _items)
            {
                result += SerializeItem(item, properties) + Environment.NewLine;
            }
            return result;
        }

    }
}
