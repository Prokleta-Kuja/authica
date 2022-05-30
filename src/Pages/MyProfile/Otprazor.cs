using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using authica.Entities;
using authica.Services;
using authica.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace authica.Pages.MyProfile
{
    public partial class Otp
    {
        [Parameter] public EventCallback OnSaved { get; set; }
        [Inject] IDataProtectionProvider DpProvider { get; set; } = null!;
        [Inject] IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;

        IDataProtector _protector = null!;
        AuthToken _newToken = null!;
        User? _originalUser;
        int? _oldCode;
        int? _newCode;
        readonly Dictionary<string, string> _errors = new();
        protected override void OnInitialized()
        {
            _protector = DpProvider.CreateProtector(nameof(User.OtpKey));
        }
        public async Task Show(User user)
        {
            _originalUser = user;
            _newToken = TotpService.CreateAuthToken(C.Configuration.Current.Issuer, user.UserName, nameof(authica));
        }
        public async Task Hide()
        {
            _originalUser = null;
            StateHasChanged();
        }
        public void Validate()
        {
            _errors.Clear();

            if (_originalUser!.CanOtp)
            {
                var otpKey = _protector.Unprotect(_originalUser!.OtpKey!);
                if (!_oldCode.HasValue || !TotpService.ValidateCode(otpKey, _oldCode.Value))
                    _errors.Add(nameof(_oldCode), "Invalid code"); // TODO: translate
            }

            if (!_newCode.HasValue)
                _errors.Add(nameof(_newCode), "Required"); // TODO: translate
        }
        public async Task SaveClicked()
        {
            Validate();
            if (_errors.Any())
                return;


            using var db = await DbFactory.CreateDbContextAsync();
            db.Attach(_originalUser!);
            _originalUser!.OtpKey = _protector.Protect(_newToken.Key);

            await db.SaveChangesAsync();

            await Hide();

            if (OnSaved.HasDelegate)
                await OnSaved.InvokeAsync();
        }
    }
}