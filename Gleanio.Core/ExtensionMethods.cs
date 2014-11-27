namespace Gleanio.Core
{
    using System;
    using System.Collections.Generic;

    public static class ExtensionMethods
    {
        #region Methods

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");

            foreach (var element in source)
            {
                action(element);
            }
        }


        public static void ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");

            int index = 0;
            foreach (var element in source)
            {
                action(index, element);

                index++;
            }
        }

        #endregion Methods
    }
}