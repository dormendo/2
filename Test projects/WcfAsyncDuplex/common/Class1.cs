using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace common
{
	// Create a service contract and define the service operations.
	// NOTE: The service operations must be declared explicitly.
	[ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPollingSessionCallback))]
	public interface IPollingSession
	{
		[OperationContract(IsOneWay = false, IsInitiating = true)]
		void Subscribe();
		[OperationContract(IsOneWay = false, IsTerminating = true)]
		void Unsubscribe();


		[OperationContract(IsOneWay = false, IsInitiating = true, Name="ASubscribe")]
		Task SubscribeAsync();
		[OperationContract(IsOneWay = false, IsTerminating = true, Name="AUnsubscribe")]
		Task UnsubscribeAsync();


		[OperationContract(AsyncPattern = true, IsOneWay = false, IsInitiating = true)]
		IAsyncResult BeginSubscribe(AsyncCallback callback, object state);
		void EndSubscribe(IAsyncResult ar);

		[OperationContract(AsyncPattern = true, IsOneWay = false, IsTerminating = true)]
		IAsyncResult BeginUnsubscribe(AsyncCallback callback, object state);
		void EndUnsubscribe(IAsyncResult ar);
	}

	public interface IPollingSessionCallback
	{
		[OperationContract(IsOneWay = false)]
		void PriceChange(string item, double price, double change);
		

		[OperationContract(IsOneWay = false, Name="APriceChange")]
		Task PriceChangeAsync(string item, double price, double change);



		[OperationContract(AsyncPattern = true, IsOneWay = false)]
		IAsyncResult BeginPriceChange(string item, double price, double change, AsyncCallback callback, object state);
		void EndPriceChange(IAsyncResult ar);
	}

	[ServiceContract]
	public interface IPublishData
	{
		[OperationContract(IsOneWay=true)]
		void PublishPriceChange(string item, double price, double change);
	}
}
