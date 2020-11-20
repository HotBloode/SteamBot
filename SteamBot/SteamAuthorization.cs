using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Numerics;

namespace SteamBot
{
    public class SteamAuthorization
    {        
        private HttpClient client;
        private string result;
        private HttpResponseMessage request;
        public RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        CookieContainer cookieContainer;
        HttpClientHandler msgHandler;

        List<Cookie> cookieList = new List<Cookie>();



        public SteamAuthorization(CookieContainer cookieContainer, HttpClientHandler msgHandler, HttpClient client)
        {
            this.cookieContainer = cookieContainer;
            this.msgHandler = msgHandler;           
            this.client = client;            
        }
        public async Task<bool> WithCookie()
        {

            if (!File.Exists("Cookes.json") || File.ReadAllText("Cookes.json")==""|| File.ReadAllText("Cookes.json")==" ")
            {
                return false;
            }
            else
            {

                //Считывание файла с Печеньками в список и добавление его в контейнер с печеньками
                cookieList = JsonConvert.DeserializeObject<List<Cookie>>(File.ReadAllText("Cookes.json"));
                foreach (var v in cookieList)
                {
                    cookieContainer.Add(v);
                }


                //Делаем запрос на стим, авось печеньки работают
                request = await client.GetAsync($"https://steamcommunity.com");
                //Получаем html страницу в ответ
                result = await request.Content.ReadAsStringAsync();

                //Проверяем на авторизацию (Проверяем если ли кнока выйти)
                if (result.Contains("Logout"))
                {

                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public async Task<bool> WithLogPass(string password, string login, string code)
        {
            //Первый запрос стиму
            request = await client.GetAsync($"https://steamcommunity.com/login/getrsakey?username=" + login);
            //Ответ от запроса
            result = await request.Content.ReadAsStringAsync();

            //Вытаскиваем из етвета ключи шифрования
            RsaKey rsaKey = JsonConvert.DeserializeObject<RsaKey>(result);

            //Заполняем объект класса ключами которые вытащили
            RsaParameters rsaParam = new RsaParameters
            {
                Exponent = rsaKey.publickey_exp,
                Modulus = rsaKey.publickey_mod,
                Password = password
            };

            //Переменная для расшифровки пароля
            var encrypted = string.Empty;

            //Проверка на заполненость
            while (encrypted.Length < 2 || encrypted.Substring(encrypted.Length - 2) != "==")
            {
                //RSA шифрование пароля
                encrypted = EncryptPassword(rsaParam);
            }

            //Создание и заполнение библиотеки для отправки в следующем запросе
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("username", login);
            data.Add("password", encrypted);
            data.Add("twofactorcode", code);
            data.Add("emailauth", "");
            data.Add("loginfriendlyname", "");
            data.Add("captchagid", "-1");
            data.Add("captcha_text", "");
            data.Add("emailsteamid", "");
            data.Add("rsatimestamp", rsaKey.timestamp);
            data.Add("remember_login", "true");

            //Отправка 2го запроса с нужными данными
            request = await client.PostAsync($"https://steamcommunity.com/login/dologin/", new FormUrlEncodedContent(data));

            //Ответ
            result = await request.Content.ReadAsStringAsync();

            //Достаём результаты авторизации
            LoginResult loginResult = JsonConvert.DeserializeObject<LoginResult>(result);

            //Проверка флага авторизации в результатах
            if (loginResult.success)
            {
               
                //Вытаскиваем нужные нам Печеньки
                IEnumerable<Cookie> responseCookies = cookieContainer.GetCookies(new Uri("https://steamcommunity.com/login/dologin")).Cast<Cookie>();

                //Чистим список старых пеенек
                cookieList.Clear();

                Cookie tmp;

                //Перебираем данные полученных печенек и пихаем их в список
                foreach (var cookie in responseCookies)
                {
                    tmp = new Cookie();
                    tmp.Comment = cookie.Comment;
                    tmp.CommentUri = cookie.CommentUri;
                    tmp.HttpOnly = cookie.HttpOnly;
                    tmp.Discard = cookie.Discard;
                    tmp.Domain = cookie.Domain;
                    tmp.Expired = cookie.Expired;
                    tmp.Expires = cookie.Expires;
                    tmp.Name = cookie.Name;
                    tmp.Path = cookie.Path;
                    tmp.Port = cookie.Port;
                    tmp.Secure = cookie.Secure;
                    tmp.Value = cookie.Value;
                    tmp.Version = cookie.Version;
                    cookieList.Add(tmp);
                }

                Console.WriteLine("Successfully logged in.");
                Console.WriteLine(result);

                //Сохраняем полученные Печеньки в json
                File.WriteAllText("Cookes.json", JsonConvert.SerializeObject(cookieList, Formatting.Indented));
                               
                return true;
            }
            else
            {
                //Вывод сообщения о работе
                
               

                Console.WriteLine("Couldn't login...");
                Console.WriteLine(result);
                return false;
            }
        }
        
        public string EncryptPassword(RsaParameters rsaParam)
        {
            // Convert the public keys to BigIntegers
            var modulus = CreateBigInteger(rsaParam.Modulus);
            var exponent = CreateBigInteger(rsaParam.Exponent);

            // (modulus.ToByteArray().Length - 1) * 8
            //modulus has 256 bytes multiplied by 8 bits equals 2048
            var encryptedNumber = Pkcs1Pad2(rsaParam.Password, (2048 + 7) >> 3);

            // And now, the RSA encryption
            encryptedNumber = BigInteger.ModPow(encryptedNumber, exponent, modulus);

            //Reverse number and convert to base64
            var encryptedString = Convert.ToBase64String(encryptedNumber.ToByteArray().Reverse().ToArray());

            return encryptedString;
        }
        public static BigInteger CreateBigInteger(string hex)
        {
            return BigInteger.Parse("00" + hex, NumberStyles.AllowHexSpecifier);
        }

        public static BigInteger Pkcs1Pad2(string data, int keySize)
        {
            if (keySize < data.Length + 11)
                return new BigInteger();

            var buffer = new byte[256];
            var i = data.Length - 1;

            while (i >= 0 && keySize > 0)
            {
                buffer[--keySize] = (byte)data[i--];
            }

            // Padding, I think
            var random = new Random();
            buffer[--keySize] = 0;
            while (keySize > 2)
            {
                buffer[--keySize] = (byte)random.Next(1, 256);
                //buffer[--keySize] = 5;
            }

            buffer[--keySize] = 2;
            buffer[--keySize] = 0;

            Array.Reverse(buffer);

            return new BigInteger(buffer);
        }
    }
}
