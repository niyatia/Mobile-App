using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Popups;
using System.Text;
using Windows.Data.Xml.Dom;
using System.Net;
using Windows.Data.Json;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641


namespace PointOfInterest
{
    public class MetaData
    {
        [DataMember(Name = "uri", EmitDefaultValue = false)]
        public string uri { get; set; }
        [DataMember(Name = "entityID", EmitDefaultValue = false)]
        public string EntityID { get; set; }
        [DataMember(Name = "displayName", EmitDefaultValue = false)]
        public string DisplayName { get; set; }
        [DataMember(Name = "addressLine", EmitDefaultValue = false)]
        public string AddressLine { get; set; }
        [DataMember(Name = "postalCode", EmitDefaultValue = false)]
        public string PostalCode { get; set; }
        [DataMember(Name = "phone", EmitDefaultValue = false)]
        public string Phone { get; set; }
        [DataMember(Name = "__distance", EmitDefaultValue = false)]
        public string __Distance { get; set; }
    }

    [DataContract]
    public class Results
    {
        [DataMember(Name = "metadata", EmitDefaultValue = false)]
        public MetaData[] metadata { get; set; }
    }

    [DataContract]
    public class Response
    {
        [DataMember(Name = "copyright", EmitDefaultValue = false)]
        public string Copyright { get; set; }

        [DataMember(Name = "results", EmitDefaultValue = false)]
        public Results[] results { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on it own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            MessageDialog messageDialog =
                new MessageDialog("This app access your phone's location. Is that ok?",
                "Location");
            // Add commands and set their command ids 
            messageDialog.Commands.Add(new UICommand("Allow", null, 1));
            messageDialog.Commands.Add(new UICommand("Cancel", null, 0));

            // Set the command that will be invoked by default 
            messageDialog.DefaultCommandIndex = 1;
            // Show the message dialog and get the event that was invoked via the async operator 
            var commandChosen = await messageDialog.ShowAsync();


            if ((int)commandChosen.Id == 1)
            {
                localSettings.Values["LocationConsent"] = true;
            }
            else
            {
                localSettings.Values["LocationConsent"] = false;
            }
        }

        private async void OneShotLocation_Click(object sender, RoutedEventArgs e)
        {
            string latitude, longitude;
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if ((bool)localSettings.Values["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                return;
            }

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                string bingMapsKey = "AqzuDl7U8coCVlvDrG30M2lwryW2N-tPnuQaIVjH4p9DFCnxD32Xgkt6ll9Selr7";
                latitude = geoposition.Coordinate.Latitude.ToString("0.00");
                longitude = geoposition.Coordinate.Longitude.ToString("0.00");
                double Radius = 3; // km

                string accessID = "f22876ec257b474b82fe2ffcb8393150";
                string dataEntityName = "NavteqNA";
                string dataSource = "NavteqPOIs";


                string requestUrl = string.Format("http://spatial.virtualearth.net/REST/v1/data/{0}/{1}/{2}?spatialFilter=nearby({3},{4},{5})&$filter=EntityTypeID eq '5800'&$select=EntityID,DisplayName,AddressLine,PostalCode,Phone,__Distance&$format=json&$top=10&key={6}", accessID, dataEntityName, dataSource, latitude, longitude, Radius, bingMapsKey);

                string json = await getData(requestUrl);
                Response childlist = new Response();

                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
                DataContractJsonSerializer ser = new DataContractJsonSerializer(childlist.GetType());
                childlist = ser.ReadObject(ms) as Response;
                string teststring = "";
                foreach (var d in childlist.results)
                {
                    foreach (var s in d.metadata)
                    {
                        teststring = teststring + "" + s.DisplayName + "\t" + s.EntityID + "\n";
                    }
                }

                ms.Dispose();

                // results = json.ToString();
                LatitudeTextBlock.Text = teststring;
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    StatusTextBlock.Text = "location  is disabled in phone settings.";
                }
                //else
                {
                    // something else happened acquring the location
                    StatusTextBlock.Text = "Some error occured.";
                }
            }
        }
        public async Task<string> getData(string requestUrl)
        {
            try
            {
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(requestUrl);
                string responseBodyAsText = await response.Content.ReadAsStringAsync();
                return responseBodyAsText;

            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
