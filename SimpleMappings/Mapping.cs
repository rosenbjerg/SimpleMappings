using System;
using System.Collections.Generic;

namespace SimpleMappings
{
    public class Mapping<TSource, TDestination>
    {
        private readonly Func<TDestination> _factory;
        private readonly ICollection<Action<TSource, TDestination>> _mappings;

        internal Mapping(Func<TDestination> factory, ICollection<Action<TSource, TDestination>> mappings)
        {
            _factory = factory;
            _mappings = mappings;
        }

        /// <summary>
        /// Maps a number of instances of the source type to a number of instances of the destination type
        /// </summary>
        public IEnumerable<TDestination> MapMany(IEnumerable<TSource> sourceInstances)
        {
            if (sourceInstances == null) yield break;
            foreach (var sourceInstance in sourceInstances)
            {
                var destinationInstance = _factory();
                foreach (var mapping in _mappings)
                    mapping(sourceInstance, destinationInstance);
                yield return destinationInstance;
            }
        }
        /// <summary>
        /// Maps an instance of the source type to an instance of the destination type
        /// </summary>
        public TDestination Map(TSource sourceInstance)
        {
            var destinationInstance = _factory();
            foreach (var mapping in _mappings)
                mapping(sourceInstance, destinationInstance);
            return destinationInstance;
        }
    }
}