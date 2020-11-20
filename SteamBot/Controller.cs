using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SteamBot
{
    public class Controller
    {
        HttpClient client;
        string result;
        HttpResponseMessage request;

        RSACryptoServiceProvider rsa;
        CookieContainer cookieContainer;
        HttpClientHandler msgHandler;

        List<Cookie> cookieList;

        public bool FlagAuthorization = true;
        private void SetHeaders()
        {


            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("DNT", "1");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36");

            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            client.DefaultRequestHeaders.Add("donotcache", "155524323");
        }

        public Controller()
        {            
            cookieContainer = new CookieContainer();
            msgHandler = new HttpClientHandler();
            msgHandler.CookieContainer = cookieContainer;
            client = new HttpClient(msgHandler);

            rsa = new RSACryptoServiceProvider();
            cookieList = new List<Cookie>();

            SetHeaders();
        }

       public async void LogIn(string log= "", string pass= "", string code="")
        {
            SteamAuthorization s = new SteamAuthorization(cookieContainer, msgHandler, client);
            if (FlagAuthorization)
            {
                if(!await s.WithCookie())
                {
                    FlagAuthorization = false;
                    return;
                }
                else
                {
                    FlagAuthorization = true;
                    SpamTopic sp = new SpamTopic(client);
                    sp.Spam();

                    return;
                }
            }
            else
            {                
                if (!await s.WithLogPass(pass, log, code))
                {
                    //Сообдщение с неудачей
                    return;
                }
                else
                {
                    //Сообщение с удачей
                }


            }

        }
    }
}
