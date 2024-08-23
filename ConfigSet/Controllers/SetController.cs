using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System.Net.Http;
using System.Text;

namespace ConfigSet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SetController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("/SetConfig")]
        public async Task ReloadConfigurationAsync2()
        {

            var list = new List<Input>
            {
                new Input{Key="SM4.AppOne.Key",Value="更改了App1的key值" },
                new Input{Key="SM4.AppTwo.Secret",Value="更改了App2的Secret值" },
                new Input{Key="Log.Path",Value="新增了一个LogPath值" },
            };
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(list), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:5126/Set", content);
            response.EnsureSuccessStatusCode();
        }

    }
}
