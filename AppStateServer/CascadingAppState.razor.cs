using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.ComponentModel;
using System.Text.Json;

namespace AppStateServer;

public partial class CascadingAppState : ComponentBase, IAppState
{
	private readonly string StorageKey = "MyAppStateKey";

	private readonly int StorageTimeoutInSeconds = 30;

	bool loaded = false;

	public DateTime LastStorageSaveTime { get; set; }

	[Inject]
	ProtectedLocalStorage localStorage { get; set; }

	[Parameter]
	public RenderFragment ChildContent { get; set; }

	/// <summary>
	/// Implement property handlers like so
	/// </summary>
	private string message = "";
	public string Message
	{
		get => message;
		set
		{
			message = value;
			// Force a re-render
			StateHasChanged();
			// Save to local storage
			new Task(async () =>
			{
				await Save();
			}).Start();
		}
	}

	private int count = 0;
	public int Count
	{
		get => count;
		set
		{
			count = value;
			StateHasChanged();
			new Task(async () =>
			{
				await Save();
			}).Start();
		}
	}


	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await Load();
			loaded = true;
			StateHasChanged();
		}
	}

	protected override void OnInitialized()
	{
		Message = "Initial Message";
	}

	// Used for tracking changes
	public IAppState GetCopy()
	{
		var state = (IAppState)this;
		var json = JsonSerializer.Serialize(state);
		var copy = JsonSerializer.Deserialize<AppState>(json);
		return copy;
	}

	public async Task Save()
	{
		if (!loaded) return;

		// set LastSaveTime
		LastStorageSaveTime = DateTime.Now;
		// serialize 
		var state = (IAppState)this;
		var json = JsonSerializer.Serialize(state);
		// save
		await localStorage.SetAsync(StorageKey, json);
	}

	public async Task Load()
	{
		try
		{
			var data = await localStorage.GetAsync<string>(StorageKey);
			var state = JsonSerializer.Deserialize<AppState>(data.Value);
			if (state != null)
			{
				if (DateTime.Now.Subtract(state.LastStorageSaveTime).TotalSeconds <= StorageTimeoutInSeconds)
				{
					// decide whether to set properties manually or with reflection

					// comment to set properties manually
					//this.Message = state.Message;
					//this.Count = state.Count;

					// set properties using Reflaction
					var t = typeof(IAppState);
					var props = t.GetProperties();
					foreach (var prop in props)
					{
						if (prop.Name != "LastStorageSaveTime")
						{
							object value = prop.GetValue(state);
							prop.SetValue(this, value, null);
						}
					}

				}
			}
		}
		catch (Exception ex)
		{

		}
	}
}
