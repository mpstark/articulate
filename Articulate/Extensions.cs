using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
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
	}
}
