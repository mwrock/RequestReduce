using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using RequestReduce.IOC;
using RequestReduce.Configuration;
using System.Net;

namespace RequestReduce.Utilities
{
    public interface IIpFilter
    {
        bool IsAuthorizedIpAddress(HttpContextBase context);
        string UserIpAddress(HttpContextBase context);
    }

    public class IpFilter : IIpFilter
    {
        private readonly IRRConfiguration config;
        private static readonly RegexCache Regex = new RegexCache();
        private readonly string[] IpHttpHeaders = { "HTTP_X_FORWARDED_FOR", "HTTP_X_FORWARDED", "HTTP_X_CLUSTER_CLIENT_IP", "HTTP_FORWARDED_FOR", "HTTP_FORWARDED" };
        
        public IpFilter(IRRConfiguration config)
        {
            this.config = config;
        }

        public bool IsAuthorizedIpAddress(HttpContextBase context)
        {
            if (config.IpFilterList == null || config.IpFilterList.Count() == 0)
            {
                return true;
            }

            IEnumerable<string> validIpFilters = config.IpFilterList.Select(f => f.Trim()).Where(f => IsValidIpAddress(f));
            if (validIpFilters.Count() == 0)
            {
                return true;
            }

            string userIpAddress = UserIpAddress(context);
            if (!IsValidIpAddress(userIpAddress))
            {
                return false;
            }

            return validIpFilters.Select(f => IPAddress.Parse(f)).Any(f => f.Equals(IPAddress.Parse(userIpAddress)));
        }

        public string UserIpAddress(HttpContextBase context)
        {
            if (context == null || context.Request == null)
            {
                return null;
            }

            // Look at IP header first. Harder to fake than HTTP headers.
            string hostAddr;
            try
            {
                // Interop services
                hostAddr = context.Request.UserHostAddress;
            }
            catch (ArgumentException)
            {
                return null;
            }

            if (IsPublicIpAddress(hostAddr) && !IsTrustedProxy(hostAddr, config.ProxyList))
            {
                return hostAddr;
            }

            // Look at HTTP headers
            foreach (string header in IpHttpHeaders)
            {
                try
                {
                    // Interop services
                    if (context.Request.ServerVariables[header] != null)
                    {
                        IEnumerable<string> values = context.Request.ServerVariables[header].Split(new char[] { ',' }).Select(val => val.Trim());

                        // Find the last public (most reliable) IP address
                        // http://en.wikipedia.org/wiki/X-Forwarded-For#Format
                        string lastPublicIp = values.LastOrDefault(val => IsPublicIpAddress(val) && !IsTrustedProxy(val, config.ProxyList));
                        if (lastPublicIp != null)
                        {
                            return lastPublicIp;
                        }
                    }
                }
                catch (ArgumentException) { }
            }

            // No public IP found. Use IP header.
            try
            {
                // Interop services
                hostAddr = context.Request.UserHostAddress;
            }
            catch (ArgumentException)
            {
                return null;
            }
             
            return hostAddr;
        }

        private bool IsPublicIpAddress(string ip)
        {
            return IsValidIpAddress(ip) && !Regex.PrivateIpPattern.IsMatch(ip);
        }

        private bool IsTrustedProxy(string ip, IEnumerable<string> proxyList)
        {
            if (proxyList == null)
            {
                return false;
            }
            return IsValidIpAddress(ip) &&
                proxyList.Select(p => p.Trim()).Where(p => IsValidIpAddress(p))
                         .Select(p => IPAddress.Parse(p)).Any(p => p.Equals(IPAddress.Parse(ip)));
        }

        private bool IsValidIpAddress(string ip)
        {
            IPAddress res;
            return IPAddress.TryParse(ip ?? "", out res);
        }
    }
}
