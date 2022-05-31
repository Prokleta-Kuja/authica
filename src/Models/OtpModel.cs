using System;
using System.Collections.Generic;
using System.Linq;
using authica.Entities;
using authica.Services;
using authica.Translations;
using Microsoft.AspNetCore.DataProtection;
using QRCoder;

namespace authica.Models;

public class OtpModel
{
    public byte[]? OldKey { get; set; }
    public decimal? OldCode { get; set; }
    public AuthToken NewToken { get; set; }
    public decimal? NewCode { get; set; }
    public string Qr { get; set; }
    public string ChunkedSecret { get; set; }
    public OtpModel(User u, IDataProtectionProvider dpProvider)
    {
        NewToken = TotpService.CreateAuthToken(C.Configuration.Current.Issuer, u.UserName, C.Configuration.Current.Issuer);
        if (u.OtpKey != null)
        {
            var protector = dpProvider.CreateProtector(nameof(User.OtpKey));
            OldKey = protector.Unprotect(u.OtpKey);
        }

        var chunks = NewToken.Secret.Chunk(4).Select(c => new string(c));
        ChunkedSecret = string.Join(' ', chunks);

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(NewToken.Uri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(5);
        Qr = $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";
    }
    public Dictionary<string, string>? Validate(IMyProfile t)
    {
        var errors = new Dictionary<string, string>();
        if (OldKey != null)
        {
            if (!OldCode.HasValue)
                errors.Add(nameof(OldCode), t.ValidationRequired);
            else if (!TotpService.ValidateCode(OldKey, (int)OldCode.Value))
                errors.Add(nameof(OldCode), t.ValidationInvalid);
        }

        if (!NewCode.HasValue)
            errors.Add(nameof(NewCode), t.ValidationRequired);
        else if (!TotpService.ValidateCode(NewToken.Key, (int)NewCode))
            errors.Add(nameof(NewCode), t.ValidationInvalid);

        return errors.Any() ? errors : null;
    }
}