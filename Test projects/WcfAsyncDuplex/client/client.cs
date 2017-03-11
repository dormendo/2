//  Copyright (c) Microsoft Corporation.  All Rights Reserved.

using common;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace SubscribeClient
{
	//The service contract is defined in generatedClient.cs, generated from the service by the svcutil tool.
	//Client implementation code.

	class Client
	{
		private const int clientCount = 100;
		static void Main(string[] args)
		{
			//RunSync();
			//RunIar().Wait();
			//RunAsync().Wait();
			//RunForManyInstances();
			RunOneInstance();
		}

		private static void RunOneInstance()
		{
			ClientBase<IPollingSession>.CacheSetting = CacheSetting.AlwaysOn;
			DuplexChannelFactory<IPollingSession> factory = new DuplexChannelFactory<IPollingSession>(typeof(ClientContract), "server");
			Console.WriteLine("Press ENTER to subscribe");
			Console.ReadLine();
			Console.WriteLine("Subscribing");

			IPollingSession client = factory.CreateChannel(new InstanceContext(new ClientContract()));
			IChannel channel = (IChannel)client;
			channel.Open();
			channel.Closed += channel_Closed;
			channel.Faulted += channel_Faulted;
			client.Subscribe();
			Console.WriteLine("Subscribed");
			Console.WriteLine("Press ENTER to unsubscribe");
			Console.ReadLine();

			client.Unsubscribe();
			Console.WriteLine("Unsubscribed");
			Console.WriteLine("Press ENTER to close channel");
			Console.ReadLine();

			channel.Close();
			Console.WriteLine("Channel closed");
			Console.WriteLine("Press ENTER to exit");
			Console.ReadLine();
		}

		static void channel_Faulted(object sender, EventArgs e)
		{
		}

		static void channel_Closed(object sender, EventArgs e)
		{
		}

		private static void RunSync()
		{
			ClientBase<IPollingSession>.CacheSetting = CacheSetting.AlwaysOn;
			DuplexChannelFactory<IPollingSession> factory = new DuplexChannelFactory<IPollingSession>(typeof(ClientContract), "server");
			List<IPollingSession> clients = new List<IPollingSession>();

			//Subscribe.
			Console.WriteLine("Press ENTER to subscribe and shut down client");
			Console.ReadLine();
			Console.WriteLine("Subscribing");
			for (int i = 0; i < clientCount; i++)
			{
				//if ((i % 100) == 0)
				//{
				//	factory = new DuplexChannelFactory<ISampleContract>(typeof(ClientContract), "server");
				//}
				IPollingSession client = factory.CreateChannel(new InstanceContext(new ClientContract()));
				client.Subscribe();
				clients.Add(client);
				//Console.WriteLine(i+1);
			}

			Console.WriteLine();
			Console.WriteLine("Press ENTER to unsubscribe and shut down client");
			Console.ReadLine();

			Console.WriteLine("Unsubscribing");
			for (int i = 0; i < clientCount; i++)
			{
				IPollingSession client = clients[i];
				client.Unsubscribe();
				//Closing the client gracefully closes the connection and cleans up resources
				((IChannel)client).Close();
			}
		}

		private static void RunForManyInstances()
		{
			ClientBase<IPollingSession>.CacheSetting = CacheSetting.AlwaysOn;
			DuplexChannelFactory<IPollingSession> factory = new DuplexChannelFactory<IPollingSession>(typeof(ClientContract), "server");
			List<IPollingSession> clients = new List<IPollingSession>();

			//Subscribe.
			Console.WriteLine("Subscribing");
			for (int i = 0; i < 100; i++)
			{
				IPollingSession client = factory.CreateChannel(new InstanceContext(new ClientContract()));
				client.Subscribe();
				clients.Add(client);
			}

			//Console.WriteLine();
			//Console.WriteLine("Press ENTER to unsubscribe and shut down client");
			//Console.ReadLine();
			Thread.Sleep(600000);

			Console.WriteLine("Unsubscribing");
			for (int i = 0; i < clientCount; i++)
			{
				IPollingSession client = clients[i];
				client.Unsubscribe();
				//Closing the client gracefully closes the connection and cleans up resources
				((IChannel)client).Close();
			}
		}

		private static async Task RunIar()
		{
			ClientBase<IPollingSession>.CacheSetting = CacheSetting.AlwaysOn;
			DuplexChannelFactory<IPollingSession> factory = new DuplexChannelFactory<IPollingSession>(typeof(ClientContract), "server");
			List<IPollingSession> clients = new List<IPollingSession>();

			Console.WriteLine("Press ENTER to subscribe and shut down client");
			Console.ReadLine();
			Console.WriteLine("Subscribing");
			List<Task> taskList = new List<Task>(100);
			for (int i = 0; i < clientCount; i++)
			{
				IPollingSession client = factory.CreateChannel(new InstanceContext(new ClientContract()));
				taskList.Add(Task.Factory.FromAsync(client.BeginSubscribe, client.EndSubscribe, null).
					ContinueWith((t)=>
					{
						if (((IChannel)client).State != CommunicationState.Opened)
						{
							CommunicationState status = ((IChannel)client).State;
						}
						lock (clients)
						{
							clients.Add(client);
						} 
					}));
				if (taskList.Count == 100 || i == clientCount - 1)
				{
					await Task.WhenAny(taskList);
					taskList.Clear();
				}
			}

			Console.WriteLine();
			Console.WriteLine("Press ENTER to unsubscribe and shut down client");
			Console.ReadLine();

			Console.WriteLine("Unsubscribing");
			for (int i = 0; i < clientCount; i++)
			{
				IPollingSession client = clients[i];
				taskList.Add(Task.Factory.FromAsync(client.BeginUnsubscribe, client.EndUnsubscribe, null).
					ContinueWith((t) => { ((IChannel)client).Close(); }));
				if (taskList.Count == 200 || i == clientCount - 1)
				{
					await Task.WhenAll(taskList);
					taskList.Clear();
				}
				//Closing the client gracefully closes the connection and cleans up resources
			}
		}

		private static async Task RunAsync()
		{
			ClientBase<IPollingSession>.CacheSetting = CacheSetting.AlwaysOn;
			DuplexChannelFactory<IPollingSession> factory = new DuplexChannelFactory<IPollingSession>(typeof(ClientContract), "server");
			List<IPollingSession> clients = new List<IPollingSession>();

			Console.WriteLine("Press ENTER to subscribe and shut down client");
			Console.ReadLine();
			Console.WriteLine("Subscribing");
			List<Task> taskList = new List<Task>(100);
			for (int i = 0; i < clientCount; i++)
			{
				IPollingSession client = factory.CreateChannel(new InstanceContext(new ClientContract()));
				taskList.Add(client.SubscribeAsync().ContinueWith((t) => { lock (clients) { clients.Add(client); } }));
				if (taskList.Count == 100 || i == clientCount - 1)
				{
				}
			}
			await Task.WhenAll(taskList);
			taskList.Clear();

			Console.WriteLine();
			Console.WriteLine("Press ENTER to unsubscribe and shut down client");
			Console.ReadLine();

			Console.WriteLine("Unsubscribing");
			for (int i = 0; i < clientCount; i++)
			{
				IPollingSession client = clients[i];
				taskList.Add(client.UnsubscribeAsync().ContinueWith((t) => { ((IChannel)client).Close(); }));
				if (taskList.Count == 100 || i == clientCount - 1)
				{
				}
				//Closing the client gracefully closes the connection and cleans up resources

			}
			await Task.WhenAll(taskList);
			taskList.Clear();
		}
	}

	public class ClientContract : IPollingSessionCallback
	{
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

		public void PriceChange(string item, double price, double change)
		{
			Console.WriteLine("PriceChange(item {0}, price {1}, change {2})", item, price.ToString("C"), change);
		}

		public IAsyncResult BeginPriceChange(string item, double price, double change, AsyncCallback callback, object state)
		{
			Console.WriteLine("PriceChange(item {0}, price {1}, change {2})", item, price.ToString("C"), change);
			return new CompletedAsyncResult();
		}

		public void EndPriceChange(IAsyncResult ar)
		{
		}

		public async Task PriceChangeAsync(string item, double price, double change)
		{
			Console.WriteLine("PriceChange(item {0}, price {1}, change {2})", item, price.ToString("C"), change);
		}
	}
}

