using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace DynamicDnsClient.Validation;

public static class Ipv4Address
{
    /// <summary>The length of the longest canonical IPv4 address: "255.255.255.255".</summary>
    public const int MaxLength = 15;

    /// <summary>
    /// Returns true only for an IPv4 address in canonical dotted-decimal form, e.g. "62.59.90.127".
    /// Rejects anything else, including surrounding whitespace, leading zeros ("062.59.90.127")
    /// and the shorthand forms that <see cref="IPAddress.TryParse(string?, out IPAddress?)"/> accepts ("62.59.90").
    /// </summary>
    public static bool IsCanonical([NotNullWhen(true)] string? value) =>
        value is { Length: > 0 and <= MaxLength } &&
        IPAddress.TryParse(value, out var address) &&
        address.AddressFamily == AddressFamily.InterNetwork &&
        address.ToString() == value;
}
