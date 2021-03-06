﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Gorilla.Wistia.Authentication;
using Gorilla.Wistia.Modules.Stats;
using Gorilla.Wistia.Modules.Upload;
using Newtonsoft.Json;
using Account = Gorilla.Wistia.Modules.Data.Account;
using Media = Gorilla.Wistia.Modules.Data.Media;
using Project = Gorilla.Wistia.Modules.Data.Project;

namespace Gorilla.Wistia
{
    public class Client
    {
        public const string UploadUrl = "https://upload.wistia.com";
        public const string ApiEndpoint = "https://api.wistia.com/v1";

        public Client(IAuthentication authentication)
        {
            Authentication = authentication;
        }
        public IAuthentication Authentication { get; }

        public Project Project => new Project(this);

        public Media Media => new Media(this);

        public Account Account => new Account(this);

        public Upload Upload => new Upload(this);

        public Stats Stats => new Stats(this);

        public async Task<string> Get(string uri, Dictionary<string, string> parameters = null)
        {
            using (var client = new HttpClient())
            {
                var pars = ParametersToQueryString(parameters);
                var response = await client.GetAsync($"{ApiEndpoint}{uri}?{pars}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Unable to GET the data");
                }

                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> Post(string uri, Dictionary<string, string> parameters = null)
        {
            using (var client = new HttpClient())
            {
                var postData = new FormUrlEncodedContent(this.FixParameters(parameters));
                var response = await client.PostAsync($"{ApiEndpoint}{uri}", postData);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Unable to POST the data");
                }

                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> Put(string uri, Dictionary<string, string> parameters = null)
        {
            using (var client = new HttpClient())
            {
                var postData = new FormUrlEncodedContent(this.FixParameters(parameters));
                var response = await client.PutAsync($"{ApiEndpoint}{uri}", postData);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Unable to PUT the data");
                }

                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> Delete(string uri, Dictionary<string, string> parameters = null)
        {
            using (var client = new HttpClient())
            {
                var pars = ParametersToQueryString(parameters);
                var response = await client.DeleteAsync($"{ApiEndpoint}{uri}?{pars}");
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Unable to DELETE the data");
                }

                return await response.Content.ReadAsStringAsync();
            }
        }

        private Dictionary<string, string> FixParameters(Dictionary<string, string> parameters = null)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<string, string>();
            }
            parameters.Add(Authentication.FieldName, Authentication.Value);
            return parameters.Where(x => !string.IsNullOrWhiteSpace(x .Value)).ToDictionary(x => x.Key, x => x.Value);
        }

        private string ParametersToQueryString(Dictionary<string, string> parameters = null)
        {
            return string.Join("&", FixParameters(parameters)
                         .Select(p => $"{p.Key}=" + HttpUtility.UrlEncode(p.Value)));
        }

        public T Hydrate<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
