# DX Tech Tasting workshop

## Set up the language understanding

1. Go to [Luis.ai](http://Luis.ai)
2. "**Sign in or create account**"
3. "**New App**"
4. "**New application**"
5. Enter application name, can be anything
6. Scenario will be "**Bot**"
7. Select domain "**Weather**"
8. Intents > **+**
   1. Intent name: Today
   2. Example: "*what is the weather today?*"
   3. Save
9. Add new utterance
   1. "*how is the weather?*"
   2. "*how about today?*"
   3. "*is it raining?*"
   4. "*what's the weather outside?*"
10. Click "**Train**"
11. Go to **Publish** and publish your model as a web service
12. Go to **App Settings**
    1. Copy **App Id**
13. Go to Account Settings
    1. Copy **Subscription Key**

## Build the bot

First download the [emulator](https://emulator.botframework.com/) and [Visual Studio Template](http://aka.ms/bf-bc-vstemplate).

1. Start Visual Studio
2. Click *File* > *New Project* > *Visual C#* > Bot...
3. Create new folder *Dialogs*
4. Right-click the folder and select Add > Class
   1. Call it *WeatherLuisDialog.cs*
5. Change the signature and add your keys:

	```c#
	[Serializable]
	[LuisModel("<App Id>", "<Subscription Key>")]
	public class WeatherLuisDialog : LuisDialog<object>
	{

	}
	```

6. Add method to handle the None intent:

	```c#
	[LuisIntent("")]
	[LuisIntent("None")]
	public async Task None(IDialogContext context, LuisResult result)
	{
		await context.PostAsync($"Sorry, I did not understand '{result.Query}'.");

		context.Wait(this.MessageReceived);
	}
	```

7. Add method to handle the Today intent:

	```c#
	[LuisIntent("Today")]
	public async Task TodaysWeather(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
	{
		var message = await activity;
		await context.PostAsync("Looking for today's weather.");
		PromptDialog.Text(context, AfterCityEntered, "Which city are you interested in?");
	}
	```

8. Handle the city input:

	```c#
	private async Task AfterCityEntered(IDialogContext context, IAwaitable<string> result)
	{
		var city = await result;

		HttpClient hc = new HttpClient();
		var weatherResponse = await hc.GetAsync($"https://pocasi.azurewebsites.net/api/Current?		code=HD4sWFPXI67rlSWH7far1lPKTT48hGCzRdtaN2WC7K/qKU6tds7HSg==&city={city}");
		if (weatherResponse.IsSuccessStatusCode)
		{
			JObject weatherObj = JObject.Parse(await weatherResponse.Content.ReadAsStringAsync());
			var description = weatherObj["weather"]?.FirstOrDefault()["description"]?.ToString();
			var temp = weatherObj["main"]["temp"]?.ToString();

			await context.PostAsync($"Current weather: {description}, temperature: {temp} °C");
			await context.PostAsync("What do we do next?");
		}
		else
		{
			await context.PostAsync("Unable to fetch weather from the service. Try again later.");
		}

		context.Done(true);
	}
	```

9. Open **MessagesController.cs**
10. Change the body of the Post method:

	```c#
	public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
	{
		if (activity.Type == ActivityTypes.Message)
		{
			await Conversation.SendAsync(activity, () => new WeatherLuisDialog());
		}
		else
		{
			HandleSystemMessage(activity);
		}
		var response = Request.CreateResponse(HttpStatusCode.OK);
		return response;
	}
	```

9. Press F5 and wait for the browser to show up
10. Run the **Bot Framework Emulator**
11. Configure messaging endpoint: `http://localhost:3979/api/messages` and click **Connect**
12. Test your bot
