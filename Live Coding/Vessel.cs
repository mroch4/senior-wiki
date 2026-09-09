namespace SPT.GiftCards.Web.Services
{
    // no interface
    public class VesselService(IConfiguration config, ILogger<VesselService> logger)
    {
        private static readonly Dictionary<string, Vessel> Vessels = [];

        public void GetPositionAsync(string imo)
        {
            Task.Run(() => GetPosition(imo);
        }

        private Vessel GetPosition(string imo)
        {
            GetPositionAsync([imo]).Result.FirstOrDefault();
        }

        // method for one and multiple imo numbers

        private async Vessel[] GetPositionsAsync(string[] imos)
        {
            var vessels = new List<Vessel>();

            foreach (var imo in imos)
            {
                if (Vessels.ContainsKey(imo))
                {

                }

                using (var client = new HttpClient())
                {
                    var url = config["Ais:BaseUrl"] + $"/vessels/{imo}";

                    var response = await client.GetAsync(url);
                    // missing if(response.IsSuccess)

                    var vessel = await response.Content.ReadAsAsync<Vessel>();

                    Vessels[imo] = vessel;
                    vessels.Add(vessel);
                }

                return vessels.ToArray();
            }
        }
    }



    public class Vessel
    {
        public string Imo { get; set; }
        public long Latitude { get; set; }
        public long Longitude { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}