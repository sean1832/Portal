using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NetworkGh.Core.Utils
{
    internal class NetworkInfoManager
    {
        public List<NetworkInfo> Networks { get; private set; } = new List<NetworkInfo>();

        public void Load()
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

            // Get the default gateway addresses
            var defaultGatewayAddresses = NetworkInterface.GetAllNetworkInterfaces()
                .SelectMany(ni => ni.GetIPProperties().GatewayAddresses)
                .Select(ga => ga.Address.ToString())
                .Where(a => !string.IsNullOrEmpty(a)).ToList();

            foreach (NetworkInterface adapter in adapters)
            {
                if ((adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                     adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                    adapter.OperationalStatus == OperationalStatus.Up)
                {
                    var ipAddressInfo = adapter.GetIPProperties().UnicastAddresses
                        .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    if (ipAddressInfo == null) return;
                    bool isPrimary = adapter.GetIPProperties().GatewayAddresses
                        .Any(ga => defaultGatewayAddresses.Contains(ga.Address.ToString()));

                    string type = adapter.NetworkInterfaceType.ToString();
                    string adapterName = adapter.Name;
                    string macAddress = adapter.GetPhysicalAddress().ToString();
                    
                    string ipAddress = ipAddressInfo?.Address.ToString();
                    Networks.Add(new NetworkInfo(type, adapterName, ipAddress, macAddress, isPrimary));
                    PortChecker portChecker = new PortChecker();
                }
            }
        }
    }
}
