using System;

namespace Open.Nat;

/// <summary>
/// Protocol that should be used for searching a NAT device.
/// </summary>
[Flags]
public enum PortMapper
{
	None = 0,
	/// <summary>
	/// Use only Port Mapping Protocol
	/// </summary>
	Pmp = 1,

	/// <summary>
	/// Use only Universal Plug and Play
	/// </summary>
	Upnp = 2
}
