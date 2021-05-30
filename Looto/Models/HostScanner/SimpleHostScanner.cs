using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SaltwaterTaffy;
using SaltwaterTaffy.Container;

namespace Looto.Models.HostScanner
{
    /// <summary>
    /// <see cref="IHostScanner"/> interface implementation.<br/>
    /// Checks an array of hosts for Exists/NotExists state.
    /// </summary>
    public class SimpleHostScanner : IHostScanner
    {
        private readonly ParallelOptions _parallelOptions;
        private readonly object _lockObject;
        private IHostScannerConfig _config;
        private int _scannedHosts;
        private bool _isAborted = false;

        public HostData[] Hosts { get; set; }

        public int HostsCount => Hosts.Length;

        public event Action<int, int, HostData> OnOneHostWasScanned;
        public event Action<HostData[]> OnScanEnding;

        /// <summary>Create new Simple host scanner.</summary>
        public SimpleHostScanner()
        {
            _lockObject = new object();
            _parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
            };
        }

        public void Configure(IHostScannerConfig config)
        {
            _config = config;
            _parallelOptions.MaxDegreeOfParallelism = _config.MaximumCoresInLANScanning;
        }

        public void Abort()
        {
            _isAborted = true;
        }

        public async Task ScanAllAsync()
        {
            _scannedHosts = 0;
            List<HostData> result = new List<HostData>();
            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    Parallel.ForEach(Hosts, _parallelOptions, host =>
                    {
                        if (!_isAborted)
                            host.Exists = HostChecker.CheckHost(host.Host, _config);
                        else host.Exists = false;

                        // try scanning os of host
                        if (host.Exists)
                        {
                            var target = new Target(host.Host);
                            var scanner = new Scanner(target);
                            scanner.PersistentOptions = new NmapOptions {
                                NmapFlag.OsDetection
                            };
                            var hostOsDiscovery = scanner.HostDiscovery();
                            Os resultOsDiscovery = hostOsDiscovery.First().OsMatches.First();
                            host.OS =
                                $"{resultOsDiscovery.Name} ({resultOsDiscovery.Family} {resultOsDiscovery.Generation}) [{resultOsDiscovery.Certainty}]";
                            //Console.WriteLine($"{resultOsDiscovery.Name} ({resultOsDiscovery.Family} {resultOsDiscovery.Generation}) [{resultOsDiscovery.Certainty}]");
                        }
                        result.Add(host);
                        _scannedHosts++;
                        OnOneHostWasScanned?.Invoke(HostsCount, _scannedHosts, host);
                    });
                }
            });

            OnScanEnding?.Invoke(result.ToArray());
            _isAborted = false;
        }
    }
}
