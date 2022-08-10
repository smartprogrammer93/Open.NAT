using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Open.Nat.Tests;

public class InternetProtocolV6Tests
{
	[Fact]
	public async Task Connect()
	{
		var cfg = new ServerConfiguration();
		cfg.Prefix = "http://*:5431/";
		cfg.ServiceUrl = "http://[::1]:5431/dyndev/uuid:0000e068-20a0-00e0-20a0-48a8000808e0";
		cfg.ControlUrl = "http://[::1]:5431/uuid:0000e068-20a0-00e0-20a0-48a802086048/WANIPConnection:1";

		using var server = new UpnpMockServer(cfg);
		server.Start();

		server.WhenDiscoveryRequest = () =>
				  "HTTP/1.1 200 OK\r\n"
				+ "Server: Custom/1.0 UPnP/1.0 Proc/Ver\r\n"
				+ "EXT:\r\n"
				+ "Location: http://[::1]:5431/dyndev/uuid:0000e068-20a0-00e0-20a0-48a8000808e0\r\n"
				+ "Cache-Control:max-age=1800\r\n"
				+ "ST:urn:schemas-upnp-org:service:WANIPConnection:1\r\n"
				+ "USN:uuid:0000e068-20a0-00e0-20a0-48a802086048::urn:schemas-upnp-org:service:WANIPConnection:1";

		server.WhenGetExternalIpAddress = (ctx) =>
		{
			const string ResponseXml = "<?xml version=\"1.0\"?>" +
				"<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
				"s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
				"<s:Body>" +
				"<m:GetExternalIPAddressResponse xmlns:m=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
				"<NewExternalIPAddress>FE80::0202:B3FF:FE1E:8329</NewExternalIPAddress>" +
				"</m:GetExternalIPAddressResponse>" +
				"</s:Body>" +
				"</s:Envelope>";
			var bytes = Encoding.UTF8.GetBytes(ResponseXml);
			var response = ctx.Response;
			response.OutputStream.Write(bytes, 0, bytes.Length);
			response.OutputStream.Flush();
			response.StatusCode = 200;
			response.StatusDescription = "OK";
			response.Close();
		};

		var nat = new NatDiscoverer();
		using var cts = new CancellationTokenSource(5000);
		var device = await nat.DiscoverDeviceAsync(PortMapper.Upnp, cts);
		Assert.NotNull(device);

		var ip = await device.GetExternalIPAsync();
		Assert.Equal(IPAddress.Parse("FE80::0202:B3FF:FE1E:8329"), ip);
	}
}
