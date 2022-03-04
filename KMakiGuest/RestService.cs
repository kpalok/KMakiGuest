using System;
using System.Net.Http;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace KMakiGuest
{
    public class RestService
    {

        private readonly HttpClient client;
        
        public DateTime TimeOfPost { get; private set; }
        public AlertItem ActiveAlert { get; private set; }

        public RestService()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
                    return (cert?.GetPublicKeyString() ?? string.Empty).Equals(Constants.PUBLIC_KEY);
                }
            };

            client = new HttpClient(handler, false)
            {
                BaseAddress = new Uri("https://kmakiapi.hopto.org")
            };
        }

        public void CreateActiveAlertWithID(int ID)
        {
            ActiveAlert = new AlertItem() { ID = ID };
        }

        public async Task PostAlert(AlertItem alert)
        {
            Uri uri = new Uri(Constants.RestUrl);

            try
            {
                string json = JsonConvert.SerializeObject(alert);
                StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    ActiveAlert = JsonConvert.DeserializeObject<AlertItem>(responseContent);
                    TimeOfPost = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task RefreshActiveAlert()
        {
            if (ActiveAlert != null)
            {
                Uri uri = new Uri($"{Constants.RestUrl}/{ActiveAlert.ID}");
                try
                {
                    HttpResponseMessage response = await client.GetAsync(uri);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        ActiveAlert = JsonConvert.DeserializeObject<AlertItem>(content);
                    }
                    else
                    {
                        ActiveAlert = null;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public async Task DeleteActiveAlert()
        {
            if (ActiveAlert != null)
            {
                Uri uri = new Uri($"{Constants.RestUrl}/{ActiveAlert.ID}");
                try
                {
                    HttpResponseMessage response = await client.DeleteAsync(uri);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                // clear local copy regardless
                ActiveAlert = null;
            }
        }
    }
}
