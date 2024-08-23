using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private static readonly object _lock = new object();
        private readonly ILogger<TestController> _logger;

        public TestController(IConfiguration configuration, IWebHostEnvironment env, ILogger<TestController> logger)
        {
            _configuration = configuration;
            _env = env;
            _logger = logger;
        }

        [HttpPost("/Set")]
        public async Task<IActionResult> UpdateConfig([FromBody] List<Input> list)
        {
            var configFilePath = Path.Combine(_env.ContentRootPath, "appsettings.json");
            string json;

            try
            {
                json = await System.IO.File.ReadAllTextAsync(configFilePath);
                _logger.LogInformation("未更改之前：{json}", json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取配置文件时出错");
                return StatusCode(500, "读取配置文件时出错");
            }

            var jsonObj = JToken.Parse(json);

            lock (_lock)
            {
                EditConfig(jsonObj, list);

                var output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);

                try
                {
                    System.IO.File.WriteAllText(configFilePath, output);
                    var configRoot = (IConfigurationRoot)_configuration;
                    configRoot.Reload();
                    _logger.LogInformation("更改之后：{output}", output);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "写入配置文件时出错");
                    return StatusCode(500, "写入配置文件时出错");
                }
            }

            return Ok(true);
        }

        /// <summary>
        /// 测试更改是否成功
        /// </summary>
        [HttpGet]
        public string GetConfig()
        {
            return _configuration["SM4:AppOne:Key"];
        }

        /// <summary>
        /// 递归更新配置
        /// </summary>
        /// <param name="oldConfig"></param>
        /// <param name="list"></param>
        private static void EditConfig(JToken oldConfig, List<Input> list)
        {
            foreach (var item in list)
            {
                var keys = item.Key.Split('.');
                JToken currentToken = oldConfig;

                for (int i = 0; i < keys.Length - 1; i++)
                {
                    if (currentToken[keys[i]] == null)
                    {
                        //如果这个键不存在，就添加
                        currentToken[keys[i]] = new JObject();
                    }
                    currentToken = currentToken[keys[i]];
                }
                //使用最后一个键，来完成整个JToken对象的更新
                currentToken[keys.Last()] = item.Value;
            }
        }
    }
}
