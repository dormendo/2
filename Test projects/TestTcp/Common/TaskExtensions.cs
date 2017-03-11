using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestTcp
{
	public static class TaskExtensions
	{
		private struct Void { }

		public static async Task<TResult> WithCancellation<TResult>(this Task<TResult> originalTask, CancellationToken ct)
		{
			TaskCompletionSource<Void> cancelTask = new TaskCompletionSource<Void>();
			using (CancellationTokenRegistration ctr = ct.Register(t => ((TaskCompletionSource<Void>)t).TrySetResult(new Void()), cancelTask))
			{
				Task any = await Task.WhenAny(originalTask, cancelTask.Task);
				if (any == cancelTask.Task)
				{
					ct.ThrowIfCancellationRequested();
				}
			}

			return await originalTask;
		}
	}
}
