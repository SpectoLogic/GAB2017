using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Cache;
using System.Net.Http.Headers;
using ConUtility;

namespace CalcJWTDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpClient _httpClient = null;

            _httpClient = new HttpClient(new ConsoleLoggerMessageHandler(
                new WebRequestHandler() { CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache) }
                ));
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "<your subscription key>"); 
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("MyConsoleApp", "1.0"));

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://demoGAB2017.azure-api.net/calc/api/Calc/Add?a=23&b=5")
            };

            AddJwtToken(request);

            var response = _httpClient.SendAsync(request);

            Console.ReadLine();
        }

        private static void AddJwtToken(HttpRequestMessage request)
        {
            var utc0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var issueTime = DateTime.Now;
            var iat = (int)issueTime.Subtract(utc0).TotalSeconds;
            var exp = (int)issueTime.AddMinutes(10).Subtract(utc0).TotalSeconds;

            var payload = new Dictionary<string, object>()
            {
                { "iss", "CalcJWTDemo" },
                { "iat", iat.ToString() },
                { "exp", exp.ToString() },
                { "name", "Marion Huber" },
                { "role", "Mathprofessor" }
            };

            #region Commented code to generate a random key 
            //var hmac = System.Security.Cryptography.HMACSHA512.Create();
            //string secretkey = Convert.ToBase64String(hmac.Key);
            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.WriteLine("Secret Key in JWT:" + secretkey);
            #endregion
            string secretkey = "Ck4KtYlRoC/25RJBcOnrbLowfmOiKnnyBwkRJhmQVS1MXuNKCpTecMGyFRpPlAF9HSmY3wmkA4HbGynKY//VUQ==";
            #region Commented code to calculate Base64 of secretkey
            // string b64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes(secretkey));
            // b64Key == Q2s0S3RZbFJvQy8yNVJKQmNPbnJiTG93Zm1PaUtubnlCd2tSSmhtUVZTMU1YdU5LQ3BUZWNNR3lGUnBQbEFGOUhTbVkzd21rQTRIYkd5bktZLy9WVVE9PQ==
            #endregion
            string token = JWT.JsonWebToken.Encode(payload, secretkey, JWT.JwtHashAlgorithm.HS256);
            request.Headers.Add("Authorization", token);
        }
    }

    /*  Sample JWT Policy
        <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized">
            <issuer-signing-keys>
                <key>Q2s0S3RZbFJvQy8yNVJKQmNPbnJiTG93Zm1PaUtubnlCd2tSSmhtUVZTMU1YdU5LQ3BUZWNNR3lGUnBQbEFGOUhTbVkzd21rQTRIYkd5bktZLy9WVVE9PQ==</key>
            </issuer-signing-keys>
            <issuers>
                <issuer>CalcJWTDemo</issuer>
            </issuers>
            <required-claims>
                <claim name="name" match="any">
                    <value>Andreas Bauer</value>
                    <value>Marion Huber</value>
                </claim>
            </required-claims>
        </validate-jwt>
    */
}
