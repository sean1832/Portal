﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NetworkGh.Core.Utils
{
    internal class NetworkInfo
    {
        public string Type { get; private set; }
        public string AdapterName { get; private set; }
        public string IpAddress { get; private set; }
        public string MacAddress { get; private set; }
        public bool IsPrimary { get; private set; }

        public NetworkInfo(string type, string adapterName, string ipAddress, string macAddress, bool isPrimary)
        {
            Type = type;
            AdapterName = adapterName;
            IpAddress = ipAddress;
            MacAddress = macAddress;
            IsPrimary = isPrimary;
        }

        public override string ToString()
        {
            return $"Type: {Type}, Adapter: {AdapterName}, IP: {IpAddress}, MAC: {MacAddress}, IsPrimary: {IsPrimary}";
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
