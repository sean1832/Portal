using System;
using System.Collections.Generic;

namespace Portal.Core.Utils
{
    public class PortChecker
    {
        private Dictionary<int, string> _wellKnownPorts;

        public PortChecker()
        {
            LoadWellKnownPorts();
        }

        private void LoadWellKnownPorts()
        {
            _wellKnownPorts = new Dictionary<int, string>
            {
                { 2096, "NBX DIR" },
                { 2221, "Rockwell CSP1" },
                { 2381, "Compaq HTTPS" },
                { 2478, "SecurSight Authentication Server (SLL)" },
                { 2479, "SecurSight Event Logging Server (SSL)" },
                { 2482, "Oracle GIOP SSL" },
                { 2484, "Oracle TTC SSL" },
                { 2679, "Sync Server SSL" },
                { 2762, "DICOM TLS" },
                { 3077, "Orbix 2000 Locator SSL" },
                { 3078, "Orbix 2000 Locator SSL" },
                { 3269, "Microsoft Global Catalog with LDAP/SSL" },
                { 3306, "MySQL" },
                { 3471, "jt400-ssl" },
                { 3535, "MS-LA" },
                { 5007, "wsm server ssl" },
                { 8080, "HTTP / HTTP Proxy" },
                { 3410, "Backdoor.OptixPro.13" },
                { 5432, "postgres database server" },
                { 5984, "CouchDB database server" },
                { 5986, "Windows PowerShell Default psSession Port" },
                { 6379, "Redis key-value data store" },
                { 6443, "Kubernetes API server" },
            };
        }

        public List<int> GetAvailablePorts(int startPort, int endPort)
        {
            if (startPort < 0 || endPort > 65535 || startPort > endPort)
                throw new ArgumentOutOfRangeException("Port numbers must be between 0 and 65535, and startPort must be less than or equal to endPort.");
            
            if (startPort < 1024)
            {
                throw new ArgumentOutOfRangeException(nameof(startPort), @"Ports under 1024 are reserved for system use.");
            }


            List<int> availablePorts = new List<int>();

            for (int port = startPort; port <= endPort; port++)
            {
                if (IsPortAvailable(port))
                {
                    availablePorts.Add(port);
                }
            }

            return availablePorts;
        }

        public bool IsPortAvailable(int port)
        {
            return !_wellKnownPorts.ContainsKey(port) && port>=1024;
        }

        public string GetPortDescription(int port)
        {
            if (_wellKnownPorts.TryGetValue(port, out string description))
            {
                return $"Reserved for {description}";
            }

            if (port < 1024)
            {
                return "Ports under 1024 are reserved for system use";
            }

            return "Available port";
        }
    }
}
