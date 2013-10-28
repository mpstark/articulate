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
		public abstract void Execute();
		public abstract Task ExecuteAsync();

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			
		}
	}
	
	#region Helpers

	public static class OutputExtensions
	{
		/// <summary>
		/// Spaces out each operation by the given delay, unless the operation is a <see cref="Sleep"/> operation.
		/// </summary>
		/// <param name="operations">The <see cref="OutputBase"/> operations which should be spaced apart.</param>
		/// <param name="delay">The amount of time, in milliseconds, that each operation should be delayed after the previous one.</param>
		public static IEnumerable<OutputBase> SpaceOperations(this IEnumerable<OutputBase> operations, int delay = 50)
		{
			using (var e = operations.GetEnumerator())
			{
				bool first = true;
				while (e.MoveNext())
				{
					if (!first && !(e.Current is Sleep))
						yield return new Sleep(delay);
										
					first = false;

					// Recursively space out grouped operations
					if (e.Current is OutputGroup)
						yield return new OutputGroup((e.Current as OutputGroup).Operations.SpaceOperations(delay));
					else
						yield return e.Current;
				}
			}
		}
	}

	#endregion
}
