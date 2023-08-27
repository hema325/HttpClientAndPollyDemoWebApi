using HttpClientAndPollyDemoWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientAndPollyDemoWebApi.Controllers
{
    [ApiController]
    [Route("httpClient")]
    public class HttpClientController : ControllerBase
    {
        private readonly static HttpClient _httpClient = new();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDataUSA _dataUsa;

        public HttpClientController(IHttpClientFactory httpClientFactory, IDataUSA dataUsa)
        {
            _httpClientFactory = httpClientFactory;
            _dataUsa = dataUsa;
        }

        [HttpGet("createHttpClientManuallyWithoutUsing")]
        public async Task<IActionResult> CreateHttpClientManuallyWithoutUsing()
        {
            //this is approach is not good because every request will establish
            //a new connection and leaving it opened while the object is alive
            
            var httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync("https://datausa.io/api/data?drilldowns=Nation&measures=Population");
            return Ok(await responseMessage.Content.ReadAsStringAsync());
        }

        [HttpGet("createHttpClientManuallyWithUsing")]
        public async Task<IActionResult> CreateHttpClientManuallyWithUsing()
        {
            //this is approach is not good because every request will establish
            //a new connection and closed it after a while(waiting for any lating packet) of disposing which leads to socket exhaustion problem
            
            using var httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync("https://datausa.io/api/data?drilldowns=Nation&measures=Population");
            return Ok(await responseMessage.Content.ReadAsStringAsync());
        }

        [HttpGet("useSingletonHttpClient")]
        public async Task<IActionResult> UseSingletonHttpClient()
        {
            //now we have one connection that handles all of our requests/responses which solves the prvious to problems
            //but this approach has a problem that is an unexpected failure/changes occured on the network that affects our connection
            //this connection will fail and we need to restart our application to use a new connection

            var responseMessage = await _httpClient.GetAsync("https://datausa.io/api/data?drilldowns=Nation&measures=Population");
            return Ok(await responseMessage.Content.ReadAsStringAsync());
        }

        [HttpGet("useHttpClientFactory")]
        public async Task<IActionResult> UseHttpClientFactory()
        {
            //this approach solved all of the previous approaches because all requests are gettings handled on the same connection
            //and after a period of time this connection is closed and other connection is opend to avoid network failures

            var responseMessage = await _httpClientFactory.CreateClient().GetAsync("https://datausa.io/api/data?drilldowns=Nation&measures=Population");
            return Ok(await responseMessage.Content.ReadAsStringAsync());
        }

        [HttpGet("useNamedHttpClientFactory")]
        public async Task<IActionResult> UseNamedHttpClientFactory()
        {
            //this approach is not good because it uses hardcoded string values

            var responseMessage = await _httpClientFactory.CreateClient("datausa").GetAsync("?drilldowns=Nation&measures=Population");
            return Ok(await responseMessage.Content.ReadAsStringAsync());
        }

        [HttpGet("useTypedHttpClientFactory")]
        public async Task<IActionResult> UseTypedHttpClientFactory()
        {
            //this approach is good than the previous

            return Ok(await _dataUsa.Get());
        }

        [HttpGet("testPolly")]
        public async Task<IActionResult> TestPolly()
        {
            var responseMessage = await _httpClientFactory.CreateClient("dummyHandler").GetAsync("https://localhost:7169/home/dummy");
            
            if (responseMessage.IsSuccessStatusCode)
                return Ok(await responseMessage.Content.ReadAsStringAsync());

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
