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
                var source = await GetHtmlPage();


                //request = await client.GetAsync($"https://steamcommunity.com/groups/SearchForFriends/discussions/0/364043054108978276");

                //result = await request.Content.ReadAsStringAsync();

               





                int b = 0;
            }
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