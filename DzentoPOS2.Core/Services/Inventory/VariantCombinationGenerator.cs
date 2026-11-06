using System;
using System.Collections.Generic;

namespace DzentoPOS2.Core.Services.Inventory
{
    /// <summary>
    /// Builds every value combination (cartesian product) of a set of attribute
    /// axes. Used by the variant editor to materialise variants from the
    /// product's attributes without any UI dependency, so it stays unit-testable.
    /// </summary>
    public static class VariantCombinationGenerator
    {
        /// <summary>
        /// Returns the cartesian product of <paramref name="axes"/>. Each axis is
        /// the ordered value list of one attribute; every returned list holds one
        /// value per axis, in axis order. An empty axis yields no combinations.
        /// </summary>
        public static List<List<T>> Generate<T>(IReadOnlyList<IReadOnlyList<T>> axes)
        {
            if (axes is null) throw new ArgumentNullException(nameof(axes));

            var result = new List<List<T>> { new List<T>() };

            foreach (var axis in axes)
            {
                if (axis is null || axis.Count == 0)
                    return new List<List<T>>();

                var next = new List<List<T>>(result.Count * axis.Count);
                foreach (var partial in result)
                {
                    foreach (var item in axis)
                    {
                        var clone = new List<T>(partial) { item };
                        next.Add(clone);
                    }
                }
                result = next;
            }

            return result;
        }
    }
}
