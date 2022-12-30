using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
namespace BlinkTorrent;

public class BrowserStorageHelper<T> where T : notnull
{
	private readonly ProtectedLocalStorage _protectedLocalStorage;
	private readonly string _key;

	private BrowserStorageHelper(ProtectedLocalStorage localStorage, string key)
	{ 
		_protectedLocalStorage = localStorage; 
		_key = key;
	}

	public static BrowserStorageHelper<T> Create(ProtectedLocalStorage localStorage, string key)
	{
		return new BrowserStorageHelper<T>(localStorage, key);
	}

    public async ValueTask<T> GetOrSetDefaultAsync(Func<T> getDefaultValue)
    {
		var result = await _protectedLocalStorage.GetAsync<T>(_key);
		if(result.Success && result.Value != null) return result.Value;
		var defaultValue = getDefaultValue();
		await SetAsync(defaultValue);
        return defaultValue;
    }

    public async ValueTask<T> GetOrSetDefaultAsync(Func<Task<T>> getDefaultValue)
	{
		var result = await _protectedLocalStorage.GetAsync<T>(_key);
		if(result.Success && result.Value!=null) return result.Value;
		var defaultValue = await getDefaultValue();
		await SetAsync(defaultValue);
		return defaultValue;
	}

	public async Task SetAsync(T val)
	{
		try
		{
			await _protectedLocalStorage.SetAsync(_key, val);
		}
		catch 
		{
			// TODO: log
		}
	}
}
