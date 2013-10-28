using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Articulate
{
	public abstract class OutputBase : ISerializable
	{
		/// <summary>
		/// Creates a new instance of an <see cref="OutputBase"/> object with the given display and serialization names
		/// </summary>
		/// <param name="displayName">The name that will be displayed on the user interface. May be a localizable key.</param>
		/// <param name="serializationName">The name that will be used when serializing this particular output type.</param>
		protected OutputBase(string displayName, string serializationName)
		{
			DisplayName = displayName;
			SerializationName = serializationName;
		}

		protected OutputBase(SerializationInfo info, StreamingContext context)
		{
			if (info == null) throw new ArgumentNullException("info");

			DisplayName = info.GetString("DisplayName");
			SerializationName = info.GetString("SerializationName");
		}

		public string DisplayName { get; protected set; }
		public string SerializationName { get; protected set; }

		public abstract void Execute();
		public abstract Task ExecuteAsync();

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null) throw new ArgumentNullException("info");

			info.AddValue("DisplayName", DisplayName);
			info.AddValue("SerializationName", SerializationName);
		}
	}
	
	#region Helpers

	/// <summary>
	/// Provides some helper extensions for <see cref="OutputBase"/> collections which make
	/// common tasks easier.
	/// </summary>
	public static class OutputExtensions
	{
		/// <summary>
		/// Spaces out each operation by the given delay, unless the operation is a <see cref="Sleep"/> operation.
		/// </summary>
		/// <param name="operations">The <see cref="OutputBase"/> operations which should be spaced apart.</param>
		/// <param name="delay">The amount of time, in milliseconds, that each operation should be delayed after the previous one.</param>
		/// <returns>Enumerates the original operations, with the given <paramref name="delay"/> inserted between each operation.</returns>
		/// <remarks>
		/// Some special treatement is given to <see cref="Space"/> and <see cref="OutputGroup"/> operation types, with <see cref="Space"/>
		/// operations not receiving the prefixing or postfixing <paramref name="delay"/> and <see cref="OutputGroup"/>s being recursively
		/// spaced such that their internal operations receive the same <paramref name="delay"/>.
		/// </remarks>
		public static IEnumerable<OutputBase> SpaceOperations(this IEnumerable<OutputBase> operations, int delay = 50)
		{
			using (var e = operations.GetEnumerator())
			{
				bool first = true;
				while (e.MoveNext())
				{
					if (!first && !(e.Current is Sleep))
					{
						first = true;
						yield return new Sleep(delay);
					}
										
					first = false;

					// Recursively space out grouped operations
					if (e.Current is OutputGroup)
						yield return new OutputGroup((e.Current as OutputGroup).Operations.SpaceOperations(delay));
					else
						yield return e.Current;
				}
			}
		}

		/// <summary>
		/// Flattens the given <see cref="OutputBase"/> enumerable into a single depth collection of ordered operations
		/// </summary>
		/// <param name="operations">The <see cref="OutputBase"/> operations which should have higher depth versions flattened.</param>
		/// <returns>Returns an <see cref="IEnumerable"/> collection of operations without any recursive depth elements</returns>
		/// <remarks>
		/// This method should be used prior to serialization to improve performance, reduce complexity and avoid the need for recursion
		/// when serializing and deserializing <see cref="OutputBase"/> objects.
		/// </remarks>
		public static IEnumerable<OutputBase> Flatten(this IEnumerable<OutputBase> operations)
		{
			using (var e = operations.GetEnumerator())
			{
				while (e.MoveNext())
				{
					if (e.Current is OutputGroup)
					{
						using (var e2 = (e.Current as OutputGroup).Operations.Flatten().GetEnumerator())
						{
							while (e2.MoveNext())
								yield return e2.Current;
						}
					}
					else
						yield return e.Current;
				}
			}
		}
	}

	#endregion
}
