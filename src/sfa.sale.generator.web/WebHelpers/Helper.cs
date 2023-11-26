namespace sfa.sale.generator.web.WebHelpers;

public static class Helper
{
    public static (string domain, string user) ResolveWindowsUserAndDomain(string windowsUser)
         => windowsUser?.Contains('\\') ?? false
                 ? (windowsUser.Split('\\')[0], windowsUser.Split('\\')[1])
                 : (string.Empty, windowsUser);

    public static string ResolveWindowsUserName(string windowsUser)
        => ResolveWindowsUserAndDomain(windowsUser).user;
}