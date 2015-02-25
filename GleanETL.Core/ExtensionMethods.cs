using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GleanETL.Core
{
    public static class ExtensionMethods
    {
        #region Methods

        public static void AppendFormattedLine(this StringBuilder builder, string line, params object[] parameters)
        {
            builder.AppendLine(string.Format(line, parameters) + Environment.NewLine);
        }


        public static bool ContainsCaseInsensitive(this string text, string search)
        {
            return text.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }

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

            var index = 0;
            foreach (var element in source)
            {
                action(index, element);

                index++;
            }
        }

        public static T[] ForEachAssign<T>(this T[] source, Func<int, T, T> func)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (func == null) throw new ArgumentNullException("func");

            for (var i = 0; i < source.Length; i++)
            {
                source[i] = func(i, source[i]);
            }

            return source;
        }

        public static T[] ForEachAssign<T>(this T[] source, Func<T, T> func)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (func == null) throw new ArgumentNullException("func");

            for (var i = 0; i < source.Length; i++)
            {
                source[i] = func(source[i]);
            }

            return source;
        }

        public static bool IsLastElement<T>(this IEnumerable<T> items, T element)
            where T : class
        {
            var last = items.LastOrDefault();

            return last != null && last.Equals(element);
        }

        public static bool IsNullOrEmpty(this Array array)
        {
            return array == null || array.Length == 0;
        }

        public static bool IsNumber(this string text)
        {
            return IsNumber(text, 0, text.Length);
        }

        public static bool IsNumber(this string text, int startIndex)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            var length = text.Length - startIndex;

            return IsNumber(text, startIndex, length);
        }

        public static bool IsNumber(this string text, int startIndex, int length)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            if (length <= 0) throw new ArgumentException("length");
            if (startIndex < 0) throw new ArgumentException("startIndex");

            var testLength = Math.Min(text.Length, length);

            var testText = text.Substring(startIndex, testLength);

            double x;
            return !string.IsNullOrWhiteSpace(testText) &&
                   double.TryParse(testText, NumberStyles.Number, CultureInfo.InvariantCulture, out x);
        }

        public static string RemoveAllWhitespace(this string text)
        {
            if (text == null) throw new ArgumentNullException("text");

            text = text.Replace("\t", string.Empty);

            while (text.Contains(Constants.SingleSpace))
            {
                text = text.Replace(Constants.SingleSpace, string.Empty);
            }

            return text;
        }

        public static string TrimAndRemoveConsecutiveWhitespace(this string text)
        {
            if (text == null) throw new ArgumentNullException("text");

            text = text.Replace("\t", Constants.SingleSpace).Trim();

            while (text.Contains(Constants.DoubleSpace))
            {
                text = text.Replace(Constants.DoubleSpace, Constants.SingleSpace);
            }

            return text;
        }

        #endregion Methods
    }
}