using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp
{
	internal abstract class TestBase : IDisposable
	{
		protected string _name;

		protected int _iterationCount;

		protected int _currentIteration;

		protected int _singleOperations;

		protected Stopwatch _warmUpTime;

		protected Stopwatch _operationTime;

		protected TestBase(string name, int iterationCount)
		{
			this._name = name;
			this._iterationCount = iterationCount;
		}

		public virtual void Run()
		{
			this._currentIteration = -1;
			this._warmUpTime = new Stopwatch();
			this.RunIteration(this._warmUpTime);

			this._operationTime = new Stopwatch();
			for (this._currentIteration = 0; this._currentIteration < this._iterationCount; this._currentIteration++)
			{
				this.RunIteration(this._operationTime);
			}
		}

		protected abstract void RunIteration(Stopwatch sw);

		public virtual void PrintReport()
		{
			Console.WriteLine($"{this._name}. Iterations: {this._iterationCount}/{this._singleOperations}, warm up: {this._warmUpTime.ElapsedMilliseconds}, time: {this._operationTime.ElapsedMilliseconds}, total: {this._warmUpTime.ElapsedMilliseconds + this._operationTime.ElapsedMilliseconds}");
		}

		public virtual void Dispose()
		{
		}
	}
}
