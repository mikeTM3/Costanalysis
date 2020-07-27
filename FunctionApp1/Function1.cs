using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Reflection.Metadata;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using System.Threading;

namespace FunctionApp1
{
    public static class Function1
    {
        public static string SourceConnection = "";

        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            //
            string sourceConnection = "DefaultEndpointsProtocol=https;AccountName=stcqccostanalysis001;AccountKey=rM/fatcsRHEjJc6qx42t14wInWnLCiZ7sE04nGthCu6Y2Ux5SOyiVT09uC3sj9gnDFHt2/DCh4Yi9ZD8QpLJCg==;EndpointSuffix=core.windows.net";
            SourceConnection = sourceConnection;
            CloudStorageAccount sourceAccount = CloudStorageAccount.Parse(sourceConnection);
            CloudBlobClient sourceClient = sourceAccount.CreateCloudBlobClient();
            string sourceContainer = "costanalysis";
            CloudBlobContainer sourceBlobContainer = sourceClient.GetContainerReference(sourceContainer);
            ICloudBlob sourceBlob = await sourceBlobContainer.GetBlobReferenceFromServerAsync("costs/control.txt");
            // ICloudBlob sourceBlob = await sourceBlobContainer.GetBlobReferenceFromServerAsync("costs/PersonalFile01.txt");
            //
            BlobServiceClient iblobServiceClient = new BlobServiceClient(SourceConnection);
            BlobContainerClient icontainerClient = iblobServiceClient.GetBlobContainerClient("costanalysis");
            //
            CloudStorageAccount icloudStorageAccount = CloudStorageAccount.Parse(sourceConnection);
            CloudTableClient itableClient = icloudStorageAccount.CreateCloudTableClient();

            string tableName = "Costs";
            CloudTable icloudTable = itableClient.GetTableReference(tableName);
            // List all blobs
            //BlobServiceClient blobServiceClient = new BlobServiceClient(sourceConnection);
            //BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("costanalysis");

            //await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            //{
            //    log.LogInformation("\t" + blobItem.Name);
            //}

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;
            string lastmodified = sourceBlob.Properties.LastModified.ToString();

            //string tableResult = Test(log);
            //CheckIfDataHasBeenProcessedForYesterday();
            ProcessYesterdaysData(log, icontainerClient, icloudTable);
            UpdateCostMasterLastProcessedDateWithTodaysDate(log);

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed {lastmodified} successfully.";

            return new OkObjectResult(responseMessage);
        }

        private static async void ProcessYesterdaysData(ILogger log, BlobContainerClient icontainerClient,
             CloudTable icloudTable )
        {
            // Assume have a date of 30 June
            // List files that have created date of higher than that date
            // Grab each file, read data and post data into table
            // Move processed file into costanalysisarchive
            int daydiff = 0;
            DateTime CreatedOnDate;
            TimeSpan timespan;
            //List files and date
            DateTimeOffset CreatedOnDateDTO;

            BlobServiceClient blobServiceClient = new BlobServiceClient(SourceConnection);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("costanalysis");

            // BlobClient b = containerClient.GetBlobClient("costs/control.txt");
            CloudStorageAccount sourceAccount = CloudStorageAccount.Parse(SourceConnection);
            CloudBlobClient sourceClient = sourceAccount.CreateCloudBlobClient();
            string sourceContainer = "costanalysis";
            CloudBlobContainer sourceBlobContainer = sourceClient.GetContainerReference(sourceContainer);
            ICloudBlob sourceBlob = await sourceBlobContainer.GetBlobReferenceFromServerAsync("costs/control.txt");


            await sourceBlob.FetchAttributesAsync();//Gets the properties & metadata for the blob.
            var created = sourceBlob.Properties.Created;
            
            log.LogInformation("\t" + sourceBlob.Name + "V=" + created);
            string iDate = created.ToString();
            DateTime oDate = DateTime.Parse(iDate);
            log.LogInformation("date is " + oDate.Day + " " + oDate.Month + "  " + oDate.Year);
            string LastProcessedDateString = GetCostMasterEntityDate();
            DateTime LastProcessedDate = DateTime.Parse(LastProcessedDateString);
            log.LogInformation("Last proc date is " + LastProcessedDateString);
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                if (blobItem != null)
                {
                    CreatedOnDateDTO = (DateTimeOffset)blobItem.Properties.CreatedOn;
                    CreatedOnDate = CreatedOnDateDTO.DateTime;              

                    timespan = CreatedOnDate.Date.Subtract(LastProcessedDate.Date);
                    daydiff = timespan.Days;

                   // log.LogInformation("Date diff is" + daydiff.ToString());
                  //  if (daydiff == 0 && blobItem.Name.Contains("AzurePOC"))
                    if (daydiff == 0 )

                    {
                       ImportCsvFileIntoTable(blobItem, log, LastProcessedDate, icontainerClient, icloudTable);

                       log.LogInformation("\tProcessed " + blobItem.Name + " " + blobItem.Properties.CreatedOn + "diff= " + daydiff.ToString());
                        //        MoveCsvFileToArchive(blobItem);
                    }
                    else
                    {
                        //    log.LogInformation("No csv files found greater than lastprocessed date");
                    }


                }
                else
                {
                  
                    log.LogInformation("Blob item is null, no csv files found  to process ");
                }
            }
         
        }

        private static void MoveCsvFileToArchive(BlobItem blobItem)
        {
            throw new NotImplementedException();
        }

        private static async void ImportCsvFileIntoTable(BlobItem blobItem, ILogger log, 
            DateTime lastProcessedDate, BlobContainerClient icontainerClient, CloudTable icloudTable)
        {
            //
            string lastProcessedDateString = lastProcessedDate.Date.ToString("dd/MM/yyyy");
            DateTime resourceUsageDateTime; 

            log.LogInformation("lastproccessdatestring= " + lastProcessedDateString);

            log.LogInformation("**** CSV Import ****");

            TableOperation itableOperation;

            int DayDiff = 0; 
            //string storageConnectionString = SourceConnection;
            //CloudStorageAccount icloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
            //CloudTableClient itableClient = icloudStorageAccount.CreateCloudTableClient();

            //string tableName = "Costs";
            //CloudTable icloudTable = itableClient.GetTableReference(tableName);

           

            BlobClient blobClient = icontainerClient.GetBlobClient(blobItem.Name);
            log.LogInformation("blob is " + blobItem.Name);
            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadAsync();
                using (var streamReader = new StreamReader(response.Value.Content))
                {
                    bool Continue = true;
                    int count = 0;
                    while (!streamReader.EndOfStream && Continue)
                    {
                        var csvLine = await streamReader.ReadLineAsync();
                      //  log.LogInformation(csvLine);
                        string[] values = csvLine.Split(',');
                        // data usage record date must be higher than last pprocessed date
                       
                        if (values[0] != "DepartmentName") 
                        {
                            resourceUsageDateTime = DateTime.Parse(values[7]);
                            TimeSpan t = resourceUsageDateTime.Date.Subtract(lastProcessedDate.Date);
                            DayDiff = t.Days;
                            // DayDiff = 1 ;
                            
                            if (DayDiff >= 0 || DayDiff < 0)
                            {
                                // Skip first line as contains header
                                string rowKey = Guid.NewGuid().ToString("N");
                                ResourceUsageEntity resourceUsageEntity = new ResourceUsageEntity(values[7], rowKey);
                                resourceUsageEntity.DepartmentName = values[0];
                                resourceUsageEntity.AccountName = values[1];
                                resourceUsageEntity.AccountOwnerId = values[2];
                                resourceUsageEntity.SubscriptionGuid = values[3];
                                resourceUsageEntity.SubscriptionName = values[4];
                                resourceUsageEntity.ResourceGroup = values[5];
                                resourceUsageEntity.ResourceLocation = values[6];
                                resourceUsageEntity.UsageDateTime = values[7];
                                resourceUsageEntity.ProductName = values[8];
                                resourceUsageEntity.MeterCategory = values[9];
                                resourceUsageEntity.MeterSubcategory = values[10];
                                resourceUsageEntity.MeterId = values[11];
                                resourceUsageEntity.MeterName = values[12];
                                resourceUsageEntity.MeterRegion = values[13];
                                resourceUsageEntity.UnitOfMeasure = values[14];
                                resourceUsageEntity.UsageQuantity = values[15];
                                resourceUsageEntity.ResourceRate = values[16];
                                resourceUsageEntity.PreTaxCost = values[17];
                                resourceUsageEntity.CostCenter = values[18];
                                resourceUsageEntity.ConsumedService = values[19];
                                resourceUsageEntity.ResourceType = values[20];
                                resourceUsageEntity.InstanceId = values[21];
                                resourceUsageEntity.Tags = values[22];
                                resourceUsageEntity.OfferId = values[23];
                                resourceUsageEntity.AdditionalInfo = values[24];
                                //  resourceUsageEntity.ETag = "*";

                                // log.LogInformation("About to write to table");
                                itableOperation = TableOperation.Insert(resourceUsageEntity);
                                var tableresult = icloudTable.ExecuteAsync(itableOperation);
                                count++;
                            }
                            
                        }
                        //  
                    }
                    log.LogInformation("Completed write of csv, count= " + count.ToString());
                }
            }

       
        }

        private static string GetCostMasterEntityDate()
        {
           // TableOperation tableOperation;

            string storageConnectionString = SourceConnection;
            //log.LogInformation("**** In Test Method ****");
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();

            string tableName = "CostsMaster";
            CloudTable cloudTable = tableClient.GetTableReference(tableName);
            TableResult retrieverResult = RetrieveRecord(cloudTable, "2020", "1");
            CostMasterEntity costMasterEntity = (CostMasterEntity)retrieverResult.Result;

            //DateTime costMasterEntityDate = DateTime.Parse(retrieverResult.ToString());
            return costMasterEntity.LastProcessedDate;

        }
        private static string UpdateCostMasterLastProcessedDateWithTodaysDate(ILogger log)
        {
         
            TableOperation tableOperation;

            string storageConnectionString = SourceConnection;
            log.LogInformation("**** In Test Method ****");
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();

            string tableName = "CostsMaster";
            CloudTable cloudTable = tableClient.GetTableReference(tableName);

            CostMasterEntity costMasterEntity = new CostMasterEntity("2020", "1");
            costMasterEntity.LastProcessedDate = DateTime.Now.ToString("dd'/'MM'/'yyyy");

            // retrieve record
            var retrieverResult = RetrieveRecord(cloudTable, "2020", "1");
            if (retrieverResult == null)
            {
                // if not found
                tableOperation = TableOperation.Insert(costMasterEntity);
            }
            else
            {
                // update
                costMasterEntity.ETag = "*";
                tableOperation = TableOperation.Replace(costMasterEntity);
            }
            var result = cloudTable.ExecuteAsync(tableOperation);
            //CreateNewTable(cloudTable, log);
            return result.Status.ToString();
        }

       

        public static TableResult RetrieveRecord(CloudTable table, string partitionKey, string rowKey)
        {
            TableOperation tableOperation = TableOperation.Retrieve<CostMasterEntity>(partitionKey, rowKey);
            var tableresult = table.ExecuteAsync(tableOperation);
            
            return tableresult.Result;
        }
        public static void CreateNewTable(CloudTable table, ILogger log)
        {
            var result = table.CreateIfNotExistsAsync();

            
            if (!result.IsCompletedSuccessfully)
            {
                log.LogInformation("Table {0} already exists", table.Name);
                return;
            }
            log.LogInformation("Table {0} created", table.Name);
        }

    }
  
}
