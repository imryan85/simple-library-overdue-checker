using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace simple_library_overdue_func
{
    public class BooklendedId
    {
        public string _id { get; set; }
    }

    public class Function1
    {
        [FunctionName("simple-library-overduecheck-trigger")]
        public async Task RunAsync([TimerTrigger("0 0 0 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            using (var client = new HttpClient())
            {
                var resBooklendedIds = await client.GetStringAsync("https://ryan-simple-library-app.herokuapp.com/overdue/all");
                var booklendedIds = JsonConvert.DeserializeObject<List<BooklendedId>>(resBooklendedIds);

                log.LogInformation($"{booklendedIds.Count} overdue books found");

                foreach (var id in booklendedIds)
                {
                    log.LogInformation($"Notifying overdue for bookLendedId: {id}");
                    var body = JsonConvert.SerializeObject(new { bookLendedId = id });
                    var content = new StringContent(body, Encoding.UTF8, "application/json");
                    await client.PostAsync("https://ryan-simple-library-subsystem.herokuapp.com/queue/handleOverdue", content);
                }
            }
        }
    }
}
