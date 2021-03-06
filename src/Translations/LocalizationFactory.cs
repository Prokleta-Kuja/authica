namespace authica.Translations;

public static class LocalizationFactory
{
    public static Formats Formats() => Formats(C.Env.Locale, C.Env.TimeZone);
    public static Formats Formats(string locale, string timeZone) => new(locale, timeZone);
    public static IStandard Standard() => Standard(C.Env.Locale);
    public static IStandard Standard(string locale)
    {
        if (locale.StartsWith("hr"))
            return new Standard_hr();

        return new Standard_en();
    }
    public static INavigation Navigation() => Navigation(C.Env.Locale);
    public static INavigation Navigation(string locale)
    {
        if (locale.StartsWith("hr"))
            return new Navigation_hr();

        return new Navigation_en();
    }

    // Pages
    public static IResetPassword ResetPassword() => ResetPassword(C.Env.Locale);
    public static IResetPassword ResetPassword(string locale)
    {
        if (locale.StartsWith("hr"))
            return new ResetPassword_hr();

        return new ResetPassword_en();
    }
    public static ISignIn SignIn() => SignIn(C.Env.Locale);
    public static ISignIn SignIn(string locale)
    {
        if (locale.StartsWith("hr"))
            return new SignIn_hr();

        return new SignIn_en();
    }
    public static ISignOut SignOut() => SignOut(C.Env.Locale);
    public static ISignOut SignOut(string locale)
    {
        if (locale.StartsWith("hr"))
            return new SignOut_hr();

        return new SignOut_en();
    }
    public static IVerify Verify() => Verify(C.Env.Locale);
    public static IVerify Verify(string locale)
    {
        if (locale.StartsWith("hr"))
            return new Verify_hr();

        return new Verify_en();
    }
    public static IApps Apps() => Apps(C.Env.Locale);
    public static IApps Apps(string locale)
    {
        if (locale.StartsWith("hr"))
            return new Apps_hr();

        return new Apps_en();
    }
    public static IUsers Users() => Users(C.Env.Locale);
    public static IUsers Users(string locale)
    {
        if (locale.StartsWith("hr"))
            return new Users_hr();

        return new Users_en();
    }
    public static IRoles Roles() => Roles(C.Env.Locale);
    public static IRoles Roles(string locale)
    {
        if (locale.StartsWith("hr"))
            return new Roles_hr();

        return new Roles_en();
    }
    public static IConfiguration Configuration() => Configuration(C.Env.Locale);
    public static IConfiguration Configuration(string locale)
    {
        if (locale.StartsWith("hr"))
            return new Configuration_hr();

        return new Configuration_en();
    }
    public static IMyProfile MyProfile() => MyProfile(C.Env.Locale);
    public static IMyProfile MyProfile(string locale)
    {
        if (locale.StartsWith("hr"))
            return new MyProfile_hr();

        return new MyProfile_en();
    }
    public static IShutdown Shutdown() => Shutdown(C.Env.Locale);
    public static IShutdown Shutdown(string locale)
    {
        if (locale.StartsWith("hr"))
            return new Shutdown_hr();

        return new Shutdown_en();
    }

    // Services
    public static IIpSecurity IpSecurity() => IpSecurity(C.Env.Locale);
    public static IIpSecurity IpSecurity(string locale)
    {
        if (locale.StartsWith("hr"))
            return new IpSecurity_hr();

        return new IpSecurity_en();
    }
    public static IMailService MailService() => MailService(C.Env.Locale);
    public static IMailService MailService(string locale)
    {
        if (locale.StartsWith("hr"))
            return new MailService_hr();

        return new MailService_en();
    }
}