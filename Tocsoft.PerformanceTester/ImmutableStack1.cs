namespace Tocsoft.PerformanceTester
{
    internal static class ImmutableStack
    {
        /// <summary>
        /// Returns an empty collection.
        /// </summary>
        /// <typeparam name="T">The type of items stored by the collection.</typeparam>
        /// <returns>The immutable collection.</returns>
        public static ImmutableStack<T> Create<T>()
        {
            return ImmutableStack<T>.Empty;
        }

        /// <summary>
        /// Creates a new immutable collection prefilled with the specified item.
        /// </summary>
        /// <typeparam name="T">The type of items stored by the collection.</typeparam>
        /// <param name="item">The item to prepopulate.</param>
        /// <returns>The new immutable collection.</returns>
        public static ImmutableStack<T> Create<T>(T item)
        {
            return ImmutableStack<T>.Empty.Push(item);
        }
    }
}
