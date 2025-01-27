//
// Authors:
//   Ben Motmans <ben.motmans@gmail.com>
//
// Copyright (C) 2007 Ben Motmans
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Nat.ConsoleTest;

internal class NatTest
{
	public static async Task Main(string[] args)
	{
		var nat = new NatDiscoverer();
		NatDiscoverer.TraceSource.Switch.Level = SourceLevels.Verbose;
		NatDiscoverer.TraceSource.Listeners.Add(new ColorConsoleTraceListener());

		using var cts = new CancellationTokenSource(5_000);
		var devices = await nat.DiscoverDevicesAsync(PortMapper.Upnp, cts);
		var device = devices.First();

		var sb = new StringBuilder();
		var ip = await device.GetExternalIPAsync();

		sb.AppendFormat("\nYour IP: {0}", ip);
		await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 1600, 1700, "Open.Nat (temporary)"));
		await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 1601, 1701, "Open.Nat (Session lifetime)"));
		await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 1602, 1702, 0, "Open.Nat (Permanent lifetime)"));
		await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 1603, 1703, 20, "Open.Nat (Manual lifetime)"));
		sb.AppendFormat("\nAdded mapping: {0}:1700 -> 127.0.0.1:1600\n", ip);
		sb.AppendFormat(
			"\n+------+-------------------------------+--------------------------------+------------------------------------+-------------------------+");
		sb.AppendFormat("\n| PROT | PUBLIC (Reacheable)		   | PRIVATE (Your computer)		| Descriptopn						|						 |");
		sb.AppendFormat(
			"\n+------+----------------------+--------+-----------------------+--------+------------------------------------+-------------------------+");
		sb.AppendFormat("\n|	  | IP Address		   | Port   | IP Address			| Port   |									| Expires				 |");
		sb.AppendFormat(
			"\n+------+----------------------+--------+-----------------------+--------+------------------------------------+-------------------------+");
		foreach (var mapping in await device.GetAllMappingsAsync())
		{
			sb.AppendFormat("\n|  {5} | {0,-20} | {1,6} | {2,-21} | {3,6} | {4,-35}|{6,25}|",
				ip, mapping.PublicPort, mapping.PrivateIP, mapping.PrivatePort, mapping.Description,
				mapping.Protocol == Protocol.Tcp ? "TCP" : "UDP", mapping.Expiration.ToLocalTime());
		}
		sb.AppendFormat(
			"\n+------+----------------------+--------+-----------------------+--------+------------------------------------+-------------------------+");

		sb.AppendFormat("\n[Removing TCP mapping] {0}:1700 -> 127.0.0.1:1600", ip);
		await device.DeletePortMapAsync(new Mapping(Protocol.Tcp, 1600, 1700));
		sb.AppendFormat("\n[Done]");

		Console.WriteLine(sb.ToString());
		/*
					var mappings = await device.GetAllMappingsAsync();
					var deleted = mappings.All(x => x.Description != "Open.Nat Testing");
					Console.WriteLine(deleted
						? "[SUCCESS]: Test mapping effectively removed ;)"
						: "[FAILURE]: Test mapping wan not removed!");
		*/
		Console.ReadKey();
	}
}
