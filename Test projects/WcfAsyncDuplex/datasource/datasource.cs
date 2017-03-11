//  Copyright (c) Microsoft Corporation.  All Rights Reserved.

using common;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ServiceModel.Samples
{
    //The service contract is defined in generatedClient.cs, generated from the service by the svcutil tool.

    //Client implementation code.

	class Client
    {
        static void Main(string[] args)
        {
			InstanceContext site = new InstanceContext(null, new Client());
			ChannelFactory<IPublishData> factory = new ChannelFactory<IPublishData>("server");
			IPublishData client = factory.CreateChannel();
			Console.ReadLine();
			
			Console.WriteLine("Sending PublishPriceChange(Gold, 400.00D, -0.25D)");
            client.PublishPriceChange("Gold", 400.00D, -0.25D);
			Console.ReadLine();

            Console.WriteLine("Sending PublishPriceChange(Silver, 7.00D, -0.20D)");
            client.PublishPriceChange("Silver", 7.00D, -0.20D);
			Console.ReadLine();

            Console.WriteLine("Sending PublishPriceChange(Platinum, 850.00D, +0.50D)");
            client.PublishPriceChange("Platinum", 850.00D, +0.50D);
			Console.ReadLine();

            Console.WriteLine("Sending PublishPriceChange(Gold, 401.00D, 1.00D)");
            client.PublishPriceChange("Gold", 401.00D, 1.00D);
			Console.ReadLine();

            Console.WriteLine("Sending PublishPriceChange(Silver, 6.60D, -0.40D)");
            client.PublishPriceChange("Silver", 6.60D, -0.40D);
			Console.ReadLine();

            Console.WriteLine();
            Console.WriteLine("Press ENTER to shut down data source");
            Console.ReadLine();

            //Closing the client gracefully closes the connection and cleans up resources
			((IChannel)client).Close();
		}
	}
}

