using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DirectPackageInstaller.Others
{
    public class DiscoveredConsole
    {
        public string IP { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public int Port { get; set; }

        public override string ToString() => $"{Name} ({IP}:{Port})";
    }

    public static class ConsoleDiscoveryService
    {
        public static readonly int[] ProbedPorts = new int[] { 12800, 9090, 9898 };

        public static async Task<List<DiscoveredConsole>> DiscoverAsync(
            Action<int, int>? onProgress = null,
            CancellationToken cancellationToken = default)
        {
            var discovered = new ConcurrentBag<DiscoveredConsole>();
            var localIps = IPHelper.EnumLocalIPs();

            if (localIps == null || localIps.Length == 0)
                return new List<DiscoveredConsole>();

            // Find valid local IPv4 subnets
            var subnets = new HashSet<string>();
            foreach (var ipStr in localIps)
            {
                if (IPAddress.TryParse(ipStr, out var ip) && ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    var bytes = ip.GetAddressBytes();
                    // Exclude loopback or link-local (127.x, 169.254.x)
                    if (bytes[0] == 127 || (bytes[0] == 169 && bytes[1] == 254))
                        continue;

                    subnets.Add($"{bytes[0]}.{bytes[1]}.{bytes[2]}");
                }
            }

            if (subnets.Count == 0)
                return new List<DiscoveredConsole>();

            var targets = new List<string>();
            foreach (var subnet in subnets)
            {
                for (int i = 1; i <= 254; i++)
                {
                    targets.Add($"{subnet}.{i}");
                }
            }

            int total = targets.Count;
            int scanned = 0;

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 40,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(targets, parallelOptions, async (targetIp, ct) =>
            {
                if (ct.IsCancellationRequested) return;

                foreach (var port in ProbedPorts)
                {
                    if (ct.IsCancellationRequested) break;

                    bool portOpen = await CheckPortAsync(targetIp, port, 350, ct);
                    if (portOpen)
                    {
                        var device = await IdentifyDeviceAsync(targetIp, port);
                        if (device != null)
                        {
                            discovered.Add(device);
                            break; // Identified this IP, no need to probe other ports on same IP
                        }
                    }
                }

                int currentScanned = Interlocked.Increment(ref scanned);
                onProgress?.Invoke(currentScanned, total);
            });

            return discovered.OrderBy(x => x.IP).ToList();
        }

        private static async Task<bool> CheckPortAsync(string ip, int port, int timeoutMs, CancellationToken ct)
        {
            try
            {
                using var client = new TcpClient();
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(timeoutMs);

                var connectTask = client.ConnectAsync(ip, port);
                var completedTask = await Task.WhenAny(connectTask, Task.Delay(timeoutMs, cts.Token));

                if (completedTask == connectTask && client.Connected)
                    return true;
            }
            catch { }

            return false;
        }

        private static async Task<DiscoveredConsole?> IdentifyDeviceAsync(string ip, int port)
        {
            try
            {
                if (port == 9090 && await IPHelper.IsGoldHENOnline(ip))
                {
                    return new DiscoveredConsole
                    {
                        IP = ip,
                        Port = 9090,
                        Name = "PS4 (GoldHEN)",
                        ServiceType = "GoldHEN Payload Server"
                    };
                }

                if (port == 12800)
                {
                    if (await IPHelper.IsEtaHenOnline(ip))
                    {
                        return new DiscoveredConsole
                        {
                            IP = ip,
                            Port = 12800,
                            Name = "PS5 (etaHEN)",
                            ServiceType = "etaHEN API"
                        };
                    }

                    if (await IPHelper.IsRPIOnline(ip))
                    {
                        return new DiscoveredConsole
                        {
                            IP = ip,
                            Port = 12800,
                            Name = "PS4 (Remote Package Installer)",
                            ServiceType = "RPI Homebrew"
                        };
                    }
                }

                if (port == 9898)
                {
                    return new DiscoveredConsole
                    {
                        IP = ip,
                        Port = 9898,
                        Name = "PS4 / PS5 (DPI Listener)",
                        ServiceType = "DirectPackageInstaller Server"
                    };
                }

                // If port is open but unknown signature, return generic console device
                return new DiscoveredConsole
                {
                    IP = ip,
                    Port = port,
                    Name = $"PlayStation ({ip})",
                    ServiceType = $"Port {port}"
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
