using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.ServiceBus;
using System.ServiceModel.Description;

namespace EchoWcfRelayServiceConsoleApp
{
    [ServiceContract]
    public interface IEchoServiceContract
    {
        [OperationContract]
        string Echo(string txt);
    }

    [ServiceBehavior(Name ="MyEchoService")]
    public class EchoService: IEchoServiceContract
    {
        public string Echo(string txt)
        {
            Console.WriteLine("Echoing {0}",txt);
            return txt;
        }
    }

    public interface IEchoChannel: IEchoServiceContract, IClientChannel
    {

    }

    class Program
    {
        static void Main(string[] args)
        {
            //Create service credential
            Console.WriteLine("Enter your Service Bus Namespace: ");
            string serviceName = Console.ReadLine();
            Console.WriteLine("Enter Your SAS key: ");
            string sasKey = Console.ReadLine();

            TransportClientEndpointBehavior sasCredential = new TransportClientEndpointBehavior();
            sasCredential.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("RootManageSharedAccessKey", sasKey);

            //Create base address
            Uri address = ServiceBusEnvironment.CreateServiceUri("sb", serviceName, "EchoService");

            //Create and configure the host
            ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.AutoDetect;
            ServiceHost host = new ServiceHost(typeof(EchoService), address);
            IEndpointBehavior serviceRegistrySettings = new ServiceRegistrySettings(DiscoveryType.Public);

            //Add Relay credential at all endpoints specified in the configuration
            foreach(ServiceEndpoint endpoint in host.Description.Endpoints)
            {
                endpoint.EndpointBehaviors.Add(serviceRegistrySettings);
                endpoint.EndpointBehaviors.Add(sasCredential);
            }

            //Open the host
            host.Open();
            Console.WriteLine("Service at Address: " + address+ " running.");
            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();

            //Close the host
            host.Close();

        }
    }
}
