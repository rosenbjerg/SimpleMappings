using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleMappings
{
    public abstract class MapperBase
    {
        private readonly Dictionary<(Type, Type), object> _mappings = new Dictionary<(Type, Type), object>();

        protected MapperBase(bool reflectProperties = true)
        {
            if (!reflectProperties) return;
            
            var mappingProperties = GetType().GetProperties()
                .Where(property => IsInstanceOfGeneric(property.PropertyType, typeof(Mapping<,>)));

            foreach (var mappingProperty in mappingProperties)
            {
                var mappingTypes = mappingProperty.PropertyType.GetGenericArguments();
                _mappings[(mappingTypes[0], mappingTypes[1])] = mappingProperty.GetMethod.Invoke(this, new object[0]);
            }
        }
        /// <summary>
        /// Maps a number of instances of the source type to a number of instances of the destination type
        /// </summary>
        public IEnumerable<TDestination> MapMany<TSource, TDestination>(IEnumerable<TSource> sourceInstances)
        {
            if (!_mappings.TryGetValue((typeof(TSource), typeof(TDestination)), out var mappings)) return default;
            var mappingFunc = (Mapping<TSource, TDestination>) mappings;
            return mappingFunc.MapMany(sourceInstances);
        }

        /// <summary>
        /// Maps an instance of the source type to an instance of the destination type
        /// </summary>
        public TDestination Map<TSource, TDestination>(TSource sourceInstance)
        {
            if (!_mappings.TryGetValue((typeof(TSource), typeof(TDestination)), out var mappings)) return default;
            var mappingFunc = (Mapping<TSource, TDestination>) mappings;
            return mappingFunc.Map(sourceInstance);
        }
        
        private static bool IsInstanceOfGeneric(Type instance, Type generic)
        {
            while (instance != null && instance != typeof(object))
            {
                var cur = instance.IsGenericType ? instance.GetGenericTypeDefinition() : instance;
                if (generic == cur)
                {
                    return true;
                }

                instance = instance.BaseType;
            }

            return false;
        }
    }
}