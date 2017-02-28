using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace WeatherBot_Done.Dialogs
{
    [Serializable]
    [LuisModel("<App Id>", "<Subscription Key>")]
    public class WeatherLuisDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Sorry, I did not understand '{result.Query}'.");

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Today")]
        public async Task TodaysWeather(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            await context.PostAsync("Looking for today's weather.");
            PromptDialog.Text(context, AfterCityEntered, "Which city are you interested in?");
        }

        private async Task AfterCityEntered(IDialogContext context, IAwaitable<string> result)
        {
            var city = await result;

            HttpClient hc = new HttpClient();
            var weatherResponse = await hc.GetAsync($"https://pocasi.azurewebsites.net/api/Current?code=HD4sWFPXI67rlSWH7far1lPKTT48hGCzRdtaN2WC7K/qKU6tds7HSg==&city={city}");
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
    }
}