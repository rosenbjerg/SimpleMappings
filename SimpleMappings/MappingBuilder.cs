using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleMappings
{
    public class MappingBuilder<TSource, TDestination>
    {
        private readonly Dictionary<string, Action<TSource, TDestination>> _mapped =
            new Dictionary<string, Action<TSource, TDestination>>();

        private MappingBuilder()
        {
        }

        /// <summary>
        /// Start building a new mapping from one type to another
        /// </summary>
        public static MappingBuilder<TSource, TDestination> New()
        {
            return new MappingBuilder<TSource, TDestination>();
        }

        /// <summary>
        /// Defines a mapping from one property on the source object to one property on the destination object
        /// </summary>
        /// <param name="getter">A function that returns the property value from the source object</param>
        /// <param name="propertySelector">A expression that select the target property on the destination object</param>
        /// <exception cref="MappingException">For properties that already has a mapping</exception>
        /// <exception cref="MappingException">If no destination property was found</exception>
        public MappingBuilder<TSource, TDestination> MapProperty<TProperty>(
            Expression<Func<TDestination, TProperty>> propertySelector,
            Func<TSource, TProperty> getter)
        {
            var propertyName = GetMemberName(propertySelector);
            if (_mapped.ContainsKey(propertyName)) throw new MappingException($"The property {propertyName} has already been mapped");

            var destinationProperty = typeof(TDestination).GetProperty(propertyName);
            if (destinationProperty == default) throw new MappingException($"A destination property with the name {propertyName} was not found on type {nameof(TDestination)}");

            void Setter(TDestination instance, TProperty value) =>
                destinationProperty.SetMethod.Invoke(instance, new object[] {value});

            _mapped[propertyName] = (source, destination) => Setter(destination, getter(source));

            return this;
        }

        /// <summary>
        /// Tries to automatically create mappings for all unmapped properties on TDestination.
        /// Use ThrowIfUnmapped() to ensure that all properties have mappings if that is needed.
        /// </summary>
        /// <param name="throwOnUnassignableNameMatch">Throw an exception if a destination property is unassignable from the source property</param>
        /// <returns></returns>
        public MappingBuilder<TSource, TDestination> AutomapRemaining(bool throwOnUnassignableNameMatch = false)
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            var destinationProperties =
                destinationType.GetProperties().Where(property => !_mapped.ContainsKey(property.Name));
            var normalizedMapping = destinationProperties.ToDictionary(property => Normalize(property.Name));

            foreach (var sourceProperty in sourceType.GetProperties())
            {
                if (!normalizedMapping.TryGetValue(Normalize(sourceProperty.Name), out var destinationProperty))
                    continue;
                
                if (!destinationProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                {
                    if (throwOnUnassignableNameMatch)
                        throw new MappingException(
                            $"{destinationProperty.Name} is not assignable from {sourceProperty.Name} ({destinationProperty.PropertyType.Name} <- {sourceProperty.PropertyType.Name})");
                    continue;
                }

                void Setter(TDestination instance, object value) => destinationProperty.SetMethod.Invoke(instance, new[] {value});
                object Getter(TSource instance) => sourceProperty.GetMethod.Invoke(instance, new object[0]);

                _mapped[destinationProperty.Name] = (source, destination) => Setter(destination, Getter(source));
            }

            return this;
        }


        /// <summary>
        /// Throws an exception if any property on the destination object is missing a mapping
        /// </summary>
        /// <returns></returns>
        /// <exception cref="MappingException">On unmapped properties</exception>
        public MappingBuilder<TSource, TDestination> ThrowIfUnmapped()
        {
            var unmapped = typeof(TDestination).GetProperties().Where(prop => !_mapped.ContainsKey(prop.Name));
            if (unmapped.Any())
                throw new MappingException($"Unmapped properties: {string.Join(", ", unmapped.Select(p => p.Name))}");
            return this;
        }

        /// <summary>
        /// Specifies that the default constructor of a given type should be used, and builds the Mapping
        /// </summary>
        public Mapping<TSource, TDestination> UsingFactory<TConstructor>()
            where TConstructor : TDestination, new()
        {
            return new Mapping<TSource, TDestination>(() => new TConstructor(), _mapped.Values);
        }

        /// <summary>
        /// Specifies the instance factory to use and builds the Mapping
        /// </summary>
        public Mapping<TSource, TDestination> UsingFactory(Func<TDestination> factory)
        {
            return new Mapping<TSource, TDestination>(factory, _mapped.Values);
        }
        
        private static string Normalize(string input)
        {
            return input.TrimStart('_').ToLowerInvariant();
        }

        private static string GetMemberName<TProperty>(Expression<TProperty> expression)
        {
            return expression.Body switch
            {
                MemberExpression m => m.Member.Name,
                UnaryExpression u when u.Operand is MemberExpression m => m.Member.Name,
                _ => throw new NotImplementedException($"The expression used is not supported: {expression.GetType()}")
            };
        }
    }
}