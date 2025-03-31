namespace MemberAPIs.Configurations;

public static class AppSetting
{
    private static readonly IConfigurationRoot configurationRoot = InitConfiguration();

    private static IConfigurationRoot InitConfiguration()
    {
        const string configFileName = "appsettings.json";

        if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), configFileName)))
        {
            return null;
        }

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(configFileName, optional: false, reloadOnChange: true);

        return builder.Build();
    }

    public static string APIToken => GetValue<string>("APIToken");

    public static T GetValue<T>(string key)
    {
        try
        {
            return configurationRoot.GetValue<T>(key);
        }
        catch (Exception ex)
        {
            throw new InvalidCastException($"Key: {key}", ex);
        }
    }
}