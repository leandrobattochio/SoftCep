namespace SoftCep.Api.Core;

public static class Consts
{
    public static readonly TimeSpan CepCacheTime = TimeSpan.FromDays(1);
    public const int HttpRetryCount = 5;
    public const string TestEnvironmentName = "Testing";
    public const string ProductionEnvironmentName = "Production";
}