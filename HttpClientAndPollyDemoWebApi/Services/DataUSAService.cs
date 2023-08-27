namespace HttpClientAndPollyDemoWebApi.Services
{
    public class DataUSAService: IDataUSA
    {
        private readonly HttpClient _httpClient;

        public DataUSAService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> Get()
        {
            var messageResponse = await _httpClient.GetAsync("?drilldowns=Nation&measures=Population");
            return await messageResponse.Content.ReadAsStringAsync();
        }
    }
}
