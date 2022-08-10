using System;
using System.Net;
using Xunit;

namespace Open.Nat.UnitTests;

public class UpnpNatDeviceInfoTests
{
	[Fact]
	public void TestSomething()
	{
		var info = new UpnpNatDeviceInfo(IPAddress.Loopback, new Uri("http://127.0.0.1:3221"), "/control?WANIPConnection", null);
		Assert.Equal("http://127.0.0.1:3221/control?WANIPConnection", info.ServiceControlUri.ToString());
	}
}
