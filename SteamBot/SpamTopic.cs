using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace SteamBot
{
    public class SpamTopic
    {
        private HttpClient client;
        private string result;
        private HttpResponseMessage request;

        public SpamTopic(HttpClient client)
        {
            this.client = client;
        }      

        public async void Spam()
        {
            while(true)
            {
                GetMaxPage();


                //request = await client.GetAsync($"https://steamcommunity.com/groups/SearchForFriends/discussions/0/364043054108978276");

                //result = await request.Content.ReadAsStringAsync();







                int b = 0;
            }
        }


        private int GetMaxPage()
        {
            IWebDriver webDriver;

            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            ChromeOptions options = new ChromeOptions();

            //off notifications
            options.AddArguments("--disable-notifications");

            //hide Chrome
            options.AddArguments("--headless");

            //Options wd
            webDriver = new ChromeDriver(driverService, options);

            webDriver.Navigate().GoToUrl("https://steamcommunity.com/groups/SearchForFriends/discussions/0/364043054108978276");
          
            System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> wbElement = webDriver.FindElements(By.ClassName("commentthread_pagelink"));

            webDriver.Close();
            webDriver.Quit();

            List<int> listWithPages = new List<int>();

            foreach(IWebElement tmp in wbElement)
            {
                listWithPages.Add(Convert.ToInt32( tmp.GetAttribute("text")));
            }  

            return listWithPages.Max();
        }

        async Task<string> GetHtmlPage()
        {
            //Код стр.
            string source = null;

            //КЛиент
            HttpClient client = new HttpClient();

            //Скачиваем стр.
            var respose = await client.GetAsync($"https://steamcommunity.com/groups/SearchForFriends/discussions/0/364043054108978276");
            if (respose != null && respose.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Сохраняем и отдаём стр.
                source = await respose.Content.ReadAsStringAsync();
            }

            return source;
        }
    }
}