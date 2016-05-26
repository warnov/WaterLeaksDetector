using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CityRadarWebApi.Controllers
{
    public class CityAlarmController : ApiController
    {
        // GET: api/CityAlarm
        public IEnumerable<Double> Get()
        {

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                 CloudConfigurationManager.GetSetting("StorageConnectionString"));
            //new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials("sawarkathon",
            //Convert.ToBase64String(
            //    "Cn1PoHKO2i/Zejk7SvyPyQzi3pHaOU+blb39VLiWc+u3NwPzj+fQEKXFb+vVC454hn9J1t2iVjR8S8WDsMrQA==")), false);
            // Create the table client.
            //StorageCredentials credentials = new StorageCredentials(accountName, accountKey);
            //CloudStorageAccount storageAccount = new CloudStorageAccount(credentials, true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable tableSource = tableClient.GetTableReference("device0");
            CloudTable tableDest = tableClient.GetTableReference("device1");
            var entities = tableSource.ExecuteQuery(new TableQuery()).ToList();
            var entitiesdest = tableDest.ExecuteQuery(new TableQuery()).ToList();
            // Create the table if it doesn't exist.
            //table.CreateIfNotExists();

            return new Double[] { entities[entities.Count()-1]["sum"].DoubleValue.Value,entitiesdest[entitiesdest.Count()-1]["sum"].DoubleValue.Value };
            //return new int[] { DateTime.Now.Second + 10, DateTime.Now.Second + 1 };


        }

        // GET: api/CityAlarm/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/CityAlarm
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/CityAlarm/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/CityAlarm/5
        public void Delete(int id)
        {
        }
    }
}
