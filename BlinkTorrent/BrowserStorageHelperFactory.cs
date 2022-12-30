using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
namespace BlinkTorrent;

public class BrowserStorageHelperFactory
{
    private readonly ProtectedLocalStorage _protectedLocalStorage;
    public BrowserStorageHelperFactory(ProtectedLocalStorage protectedLocalStorage)
    {
        _protectedLocalStorage = protectedLocalStorage;
    }

    public BrowserStorageHelper<T> Create<T>(string key) where T : notnull
    {
		return BrowserStorageHelper<T>.Create(_protectedLocalStorage, key);
    }
}
