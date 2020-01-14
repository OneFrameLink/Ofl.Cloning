using System;
using System.Collections.Generic;
using System.Reflection;
using Ofl.Linq;
using Ofl.Reflection;

namespace Ofl.Cloning
{
    public static partial class CloningExtensions
    {
        public static TDestination CloneProperties<TSource, TDestination>(this TSource source)
            where TSource : notnull
            where TDestination : notnull, new() => source.CloneProperties(new TDestination());

        public static TDestination CloneProperties<TSource, TDestination>(this TSource source, TDestination destination)
            where TSource : notnull
            where TDestination : notnull
        {
            // Validate parameters.
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            // Map the destination properties.
            // TODO: Generate compiled code, cache.
            // TODO: Map by property type as well?
            IReadOnlyDictionary<string, PropertyInfo> destinationProperties = typeof(TDestination).
                GetPropertiesWithPublicInstanceSetters().
                ToReadOnlyDictionary(pi => pi.Name, pi => pi);

            // Cycle through the source properties.  Copy to the destination.
            foreach (PropertyInfo sourceProperty in typeof(TSource).GetPropertiesWithPublicInstanceGetters())
            {
                // If not found, then skip.
                if (!destinationProperties.TryGetValue(sourceProperty.Name, out PropertyInfo destinationProperty)) continue;

                // If the source type is not assignable to the destination type, continue.
                if (!destinationProperty.PropertyType.GetTypeInfo().IsAssignableFrom(sourceProperty.PropertyType.GetTypeInfo()))
                    continue;

                // Assign.
                destinationProperty.SetValue(destination, sourceProperty.GetValue(source));
            }

            // Return the destination.
            return destination;
        }

    }
}
