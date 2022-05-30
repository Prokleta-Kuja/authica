using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace authica.Services;

// Taken from https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Extensions.Core/src/Base32.cs
internal static class Base32
{
    private const string _base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    public static string ToBase32(byte[] input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var sb = new StringBuilder();
        for (int offset = 0; offset < input.Length;)
        {
            byte a, b, c, d, e, f, g, h;
            int numCharsToOutput = GetNextGroup(input, ref offset, out a, out b, out c, out d, out e, out f, out g, out h);

            sb.Append((numCharsToOutput >= 1) ? _base32Chars[a] : '=');
            sb.Append((numCharsToOutput >= 2) ? _base32Chars[b] : '=');
            sb.Append((numCharsToOutput >= 3) ? _base32Chars[c] : '=');
            sb.Append((numCharsToOutput >= 4) ? _base32Chars[d] : '=');
            sb.Append((numCharsToOutput >= 5) ? _base32Chars[e] : '=');
            sb.Append((numCharsToOutput >= 6) ? _base32Chars[f] : '=');
            sb.Append((numCharsToOutput >= 7) ? _base32Chars[g] : '=');
            sb.Append((numCharsToOutput >= 8) ? _base32Chars[h] : '=');
        }

        return sb.ToString();
    }
    public static byte[] FromBase32(string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var trimmedInput = input.AsSpan().TrimEnd('=');
        if (trimmedInput.Length == 0)
            return Array.Empty<byte>();

        var output = new byte[trimmedInput.Length * 5 / 8];
        var bitIndex = 0;
        var inputIndex = 0;
        var outputBits = 0;
        var outputIndex = 0;
        while (outputIndex < output.Length)
        {
            var byteIndex = _base32Chars.IndexOf(char.ToUpperInvariant(trimmedInput[inputIndex]));
            if (byteIndex < 0)
                throw new FormatException();

            var bits = Math.Min(5 - bitIndex, 8 - outputBits);
            output[outputIndex] <<= bits;
            output[outputIndex] |= (byte)(byteIndex >> (5 - (bitIndex + bits)));

            bitIndex += bits;
            if (bitIndex >= 5)
            {
                inputIndex++;
                bitIndex = 0;
            }

            outputBits += bits;
            if (outputBits >= 8)
            {
                outputIndex++;
                outputBits = 0;
            }
        }
        return output;
    }
    private static int GetNextGroup(byte[] input, ref int offset, out byte a, out byte b, out byte c, out byte d, out byte e, out byte f, out byte g, out byte h)
    {
        uint b1, b2, b3, b4, b5;

        int retVal;
        switch (input.Length - offset)
        {
            case 1: retVal = 2; break;
            case 2: retVal = 4; break;
            case 3: retVal = 5; break;
            case 4: retVal = 7; break;
            default: retVal = 8; break;
        }

        b1 = (offset < input.Length) ? input[offset++] : 0U;
        b2 = (offset < input.Length) ? input[offset++] : 0U;
        b3 = (offset < input.Length) ? input[offset++] : 0U;
        b4 = (offset < input.Length) ? input[offset++] : 0U;
        b5 = (offset < input.Length) ? input[offset++] : 0U;

        a = (byte)(b1 >> 3);
        b = (byte)(((b1 & 0x07) << 2) | (b2 >> 6));
        c = (byte)((b2 >> 1) & 0x1f);
        d = (byte)(((b2 & 0x01) << 4) | (b3 >> 4));
        e = (byte)(((b3 & 0x0f) << 1) | (b4 >> 7));
        f = (byte)((b4 >> 2) & 0x1f);
        g = (byte)(((b4 & 0x3) << 3) | (b5 >> 5));
        h = (byte)(b5 & 0x1f);

        return retVal;
    }
}

// Taken from https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Extensions.Core/src/Rfc6238AuthenticationService.cs
internal static class TotpService
{
    const int DIGITS = 6;
    const int STEP_PERIOD = 30; // in seconds
    const int VALID_PREVIOUS_STEPS = -1; // -N previous steps considered valid
    const int VALID_NEXT_STEPS = 1; // N next steps considered valid
    public static (byte[] key, string uri) CreateAuthToken(string label, string username, string issuer)
    {
        var key = GenerateRandomKey();
        var secret = Base32.ToBase32(key);

        var encodedLabel = HttpUtility.UrlPathEncode(label);
        var encodedUsername = HttpUtility.UrlPathEncode(username);
        var encodedIssuer = HttpUtility.UrlEncode(issuer);

        var uri = $"otpauth://totp/{encodedLabel}:{encodedUsername}?secret={secret}&issuer={encodedIssuer}&digits={DIGITS}&period={STEP_PERIOD}";
        return (key, uri);
    }
    public static byte[] GenerateRandomKey()
    {
        var bytes = new byte[20];
        RandomNumberGenerator.Fill(bytes);
        return bytes;
    }
    internal static int ComputeTotp(byte[] key, long timestepNumber)
    {
        var timestepAsBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(timestepNumber));
        var hash = HMACSHA1.HashData(key, timestepAsBytes);

        var offset = hash[hash.Length - 1] & 0xf;
        var binaryCode = (hash[offset] & 0x7f) << 24
                            | (hash[offset + 1] & 0xff) << 16
                            | (hash[offset + 2] & 0xff) << 8
                            | (hash[offset + 3] & 0xff);

        var mod = int.Parse("1".PadRight(DIGITS + 1, '0'));// # of 0's = length of pin
        return binaryCode % mod;
    }
    private static long GetCurrentTimeStepNumber() // More info: https://tools.ietf.org/html/rfc6238#section-4
    {
        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timestep = Convert.ToInt64(unixTimestamp / 30);
        return timestep;
    }
    public static int GenerateCode(byte[] securityToken)
    {
        if (securityToken == null)
            throw new ArgumentNullException(nameof(securityToken));

        var currentTimeStep = GetCurrentTimeStepNumber();
        return ComputeTotp(securityToken, currentTimeStep);
    }
    public static bool ValidateCode(byte[] securityToken, int code)
    {
        if (securityToken == null)
            throw new ArgumentNullException(nameof(securityToken));

        var currentTimeStep = GetCurrentTimeStepNumber();
        for (var i = VALID_PREVIOUS_STEPS; i <= VALID_NEXT_STEPS; i++)
        {
            var computedTotp = ComputeTotp(securityToken, (currentTimeStep + i));
            if (computedTotp == code)
                return true;
        }

        return false;
    }
}