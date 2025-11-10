namespace SoftCep.Api.Core;

public static class Consts
{
    public const int MaxRequestPerIp = 20;
    public static readonly TimeSpan RequestWindow = TimeSpan.FromSeconds(1);
    
    public static readonly TimeSpan CepCacheTime = TimeSpan.FromDays(1);
    public const int HttpRetryCount = 5;
    public const string TestEnvironmentName = "Testing";
}