using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;

namespace PetSounds
{
    class DataCollection
    {
        public async Task<JsonObject> getData(string requestUrl)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(requestUrl);

            //will throw an exception if not successful
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return await Task.Run(() => JsonObject.Parse(content));

        }
    }
}
