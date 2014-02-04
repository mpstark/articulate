using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Runtime.Serialization;

namespace Articulate.IO
{
	public class PipedOutput : OutputBase
	{
		public PipedOutput(string command) : base("NamedPipe", "NamedPipe")
		{
			Command = command;
		}
		protected PipedOutput(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			Command = info.GetString("Command");
		}
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("Command", Command);
		}

		/// <summary>
		/// The command to be sent to the native game extension
		/// </summary>
		public string Command
		{
			get;
			private set;
		}

		[DllImport("kernel32.dll")]
		static extern bool WaitNamedPipe(string pipeName, int timeout);

		[DllImport("kernel32.dll")]
		static extern IntPtr CreateFile(string fileName, int desiredAccess, int shareMode, IntPtr securityAttributes, int creationDisposition, int flags, IntPtr templateFile);

		async static Task Write(string pipeName, string command)
		{
			using(var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.WriteThrough, System.Security.Principal.TokenImpersonationLevel.None, System.IO.HandleInheritability.None))
			{
				pipe.Connect(10);
				if (!pipe.IsConnected) return;
				var buffer = Encoding.UTF8.GetBytes(command);
				await pipe.WriteAsync(buffer, 0, buffer.Length);
			}
		}

		public override void Execute()
		{
			var task = Write(@"\\.\pipe\Articulate", Command);
			task.Wait();
		}

		public async override Task ExecuteAsync()
		{
			await Write(@"\\.\pipe\Articulate", Command);
		}
	}
}
