using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Articulate
{
	static class Extensions
	{
		// http://social.msdn.microsoft.com/Forums/en-US/36bf6ecb-70ea-4be3-aa35-b9a9cbc9a078/observable-from-any-property-in-a-inotifypropertychanged-class?forum=rx

		public static IObservable<T> ToObservable<T>(this DependencyObject dependencyObject, DependencyProperty property)
		{
			return Observable.Create<T>(o =>
			{
				var des = DependencyPropertyDescriptor.FromProperty(property, dependencyObject.GetType());
				var eh = new EventHandler((s, e) => o.OnNext((T)des.GetValue(dependencyObject)));
				des.AddValueChanged(dependencyObject, eh);
				return () => des.RemoveValueChanged(dependencyObject, eh);
			});
		}

		// http://stackoverflow.com/questions/9367119/replacing-a-char-at-a-given-index-in-string
		public static string ReplaceAt(this string input, int index, char newChar)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			char[] chars = input.ToCharArray();
			chars[index] = newChar;
			return new string(chars);
		}
		public static IEnumerable<T> Combine<T>(this IEnumerable<T> collection,T second)
		{
			using (var e = collection.GetEnumerator())
				while (e.MoveNext()) yield return e.Current;
			yield return second;
		}

		public static IEnumerable<T> Combine<T>(this IEnumerable<T> collection, IEnumerable<T> second)
		{
			using (var e = collection.GetEnumerator())
				while (e.MoveNext()) yield return e.Current;
			using (var e = second.GetEnumerator())
				while (e.MoveNext()) yield return e.Current;
		}

        public static void AddToSet<T>(this List<T> list, IEnumerable<T> items)
        {
            var set = new HashSet<T>(list);
            list.AddRange(items.Where(i => !set.Contains(i)));
        }

        public static Version GetVersion(this Assembly assembly)
        {
            return assembly?.GetName()?.Version;
        }

    }
}
