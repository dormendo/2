
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.

// This WCF sample implements the List-based Publish-Subscribe Design Pattern.

using System;
using System.ServiceModel;
using System.Diagnostics;
using common;
using System.ServiceModel.Channels;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server
{

    public class PriceChangeEventArgs : EventArgs
    {
        public string Item;
        public double Price;
        public double Change;
    }

    // The Service implementation implements your service contract.
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.PerSession)]
    public class SampleService : IPollingSession
    {
        public static int clientCount = 0;
		private static Dictionary<int, PriceChangeEventHandler> _events = new Dictionary<int, PriceChangeEventHandler>();
		public static bool AsyncNotify = false;

		public static event PriceChangeEventHandler PriceChangeEvent
		{
			add
			{
				lock (_events)
				{
					_events.Add(_events.Count, value);
					Console.WriteLine("Subscribe #" + _events.Count.ToString());
				}
			}
			remove
			{
				lock (_events)
				{
					_events.Remove(_events.Count - 1);
					Console.WriteLine("Unsubscribe #" + _events.Count.ToString());
				}
			}
		}
        public delegate void PriceChangeEventHandler(object sender, PriceChangeEventArgs e);

        IPollingSessionCallback callback = null;

        PriceChangeEventHandler priceChangeHandler = null;
		
		#region CompletedAsyncResult
		private class CompletedAsyncResult : IAsyncResult
		{
			public CompletedAsyncResult()
			{ }

			public object AsyncState
			{
				get { return null; }
			}

			public System.Threading.WaitHandle AsyncWaitHandle
			{
				get { throw new NotImplementedException(); }
			}

			public bool CompletedSynchronously
			{
				get { return true; }
			}

			public bool IsCompleted
			{
				get { return true; }
			}
		}

		#endregion

		#region Subscribe

        public void Subscribe()
        {
            callback = OperationContext.Current.GetCallbackChannel<IPollingSessionCallback>();
			((IChannel)callback).Closed += SampleService_Closed;
			((IChannel)callback).Faulted += SampleService_Faulted;
            priceChangeHandler = new PriceChangeEventHandler(PriceChangeHandler);
            PriceChangeEvent += priceChangeHandler;
			//Console.WriteLine("Subscribe. Client count: " + cc.ToString());
        }

		void SampleService_Faulted(object sender, EventArgs e)
		{
		}

		public IAsyncResult BeginSubscribe(AsyncCallback cb, object state)
		{
			callback = OperationContext.Current.GetCallbackChannel<IPollingSessionCallback>();
			((IChannel)callback).Closed += SampleService_Closed;
			priceChangeHandler = new PriceChangeEventHandler(PriceChangeHandler);
			PriceChangeEvent += priceChangeHandler;
			return new CompletedAsyncResult();
		}

		public void EndSubscribe(IAsyncResult ar)
		{
		}

		public Task SubscribeAsync()
		{
			Task task = new Task(() =>
			{
				callback = OperationContext.Current.GetCallbackChannel<IPollingSessionCallback>();
				((IChannel)callback).Closed += SampleService_Closed;
				priceChangeHandler = new PriceChangeEventHandler(PriceChangeHandler);
				PriceChangeEvent += priceChangeHandler;
			});
			task.RunSynchronously();
			return task;
		}

		#endregion

		void SampleService_Closed(object sender, EventArgs e)
		{
			
		}

		#region Unsubscribe

        public void Unsubscribe()
        {
            PriceChangeEvent -= priceChangeHandler;
			//Console.WriteLine("Unbscribe. Client count: " + cc.ToString());
		}

		public IAsyncResult BeginUnsubscribe(AsyncCallback callback, object state)
		{
			PriceChangeEvent -= priceChangeHandler;
			return new CompletedAsyncResult();
		}

		public void EndUnsubscribe(IAsyncResult ar)
		{
		}

		public async Task UnsubscribeAsync()
		{
			await Task.Factory.StartNew(() =>
			{
				PriceChangeEvent -= priceChangeHandler;
			});
		}

		#endregion

		public static void RaisePriceChangeEvent(PriceChangeEventArgs e)
		{
			foreach (PriceChangeEventHandler h in _events.Values)
			{
				h(null, e);
			}
		}

        //This event handler runs when a PriceChange event is raised.
        //The client's PriceChange service operation is invoked to provide notification about the price change.
        public void PriceChangeHandler(object sender, PriceChangeEventArgs e)
        {
            if (SampleService.AsyncNotify)
			{
				callback.PriceChangeAsync(e.Item, e.Price, e.Change);
			}
			else
			{
				callback.PriceChange(e.Item, e.Price, e.Change);
			}
        }
	}

    // The Service implementation implements your service contract.
    [ServiceBehavior]
    public class PublishService : IPublishData
	{
		public void PublishPriceChange(string item, double price, double change)
		{
			Stopwatch sw = Stopwatch.StartNew();
			PriceChangeEventArgs e = new PriceChangeEventArgs();
			e.Item = item;
			e.Price = price;
			e.Change = change;
			SampleService.RaisePriceChangeEvent(e);
			sw.Stop();
			Console.WriteLine("Оповещения отправлены {0} за {1} мс", (SampleService.AsyncNotify ? "асинхронно" : "синхронно"), sw.ElapsedMilliseconds);
			SampleService.AsyncNotify = !SampleService.AsyncNotify;
		}
	}

}

