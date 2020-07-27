using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionApp1
{

   
    public class ResourceUsageEntity :TableEntity
    {
        //
        public ResourceUsageEntity(string UsageDateTime, string rowKey )
        {
           // this.PartitionKey = Year;
          //  this.RowKey = Row;
            DateTime usageDateTimeDateTime = DateTime.Parse(UsageDateTime);
            string partitionkey = usageDateTimeDateTime.Year.ToString() + usageDateTimeDateTime.ToString("MM");
           
            this.PartitionKey = partitionkey;
           // string rowKey = UsageDateTime.ToString() + "-" + MeterId + "-" + InstanceId.Replace("/","-");
            this.RowKey = rowKey;

        }
        public ResourceUsageEntity() { }

        private string departmentName;
        private string accountName;
        private string accountOwnerId;
        private string subscriptionGuid;
        private string subscriptionName;
        private string resourceGroup;
        private string resourceLocation;
        private string usageDateTime;
        private string productName;
        private string meterCategory;
        private string meterSubcategory;
        private string meterId;
        private string meterName;
        private string meterRegion;
        private string unitOfMeasure;
        private string usageQuantity;
        private string resourceRate;
        private string preTaxCost;
        private string costCenter;
        private string consumedService;
        private string resourceType;
        private string instanceId;
        private string tags;
        private string offerId;
        private string additionalInfo;




        public string DepartmentName {
            get { return departmentName; }
            set { departmentName = value; } 
        }
        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }

        public string AccountOwnerId
        {
            get { return accountOwnerId; }
            set { accountOwnerId = value; }
        }
        public string SubscriptionGuid
        {
            get
            { return subscriptionGuid; }

            set
            {subscriptionGuid = value; }
        }
        public string SubscriptionName
        {
            get
            {  return subscriptionName;  }

            set
            {  subscriptionName = value;  }
        }

        public string ResourceGroup { 
            get { return resourceGroup;  }  
            set { resourceGroup = value; } 
        }

        public string ResourceLocation
        {
            get { return resourceLocation; }
            set { resourceLocation = value; }
        }
        public string UsageDateTime
        {
            get { return usageDateTime; }
            set { usageDateTime = value; }
        }

        public string ProductName
        {
            get { return productName; }
            set { productName = value; }
        }

        public string MeterCategory
        {
            get { return meterCategory; }
            set { meterCategory = value; }
        }

        public string MeterSubcategory
        {
            get { return meterSubcategory; }
            set { meterSubcategory = value; }
        }
       
        public string MeterId
        {
            get { return meterId; }
            set { meterId = value; }
        }

        public string MeterName
        {
            get { return meterName; }
            set { meterName = value; }
        }

        public string MeterRegion
        {
            get { return meterRegion; }
            set { meterRegion = value; }
        }

        public string UnitOfMeasure
        {
            get { return unitOfMeasure; }
            set { unitOfMeasure = value; }
        }

        public string UsageQuantity
        {
            get { return usageQuantity; }
            set { usageQuantity = value; }
        }
        public string ResourceRate
        {
            get { return resourceRate; }
            set { resourceRate = value; }
        }

        public string PreTaxCost
        {
            get { return preTaxCost; }
            set { preTaxCost = value; }
        }

        public string CostCenter
        {
            get { return costCenter; }
            set { costCenter = value; }
        }

        public string ConsumedService
        {
            get { return consumedService; }
            set { consumedService = value; }
        }

        public string ResourceType
        {
            get { return resourceType; }
            set { resourceType = value; }
        }

        public string InstanceId
        {
            get { return instanceId; }
            set { instanceId = value; }
        }

        public string Tags
        {
            get { return tags; }
            set { tags = value; }
        }

        public string OfferId
        {
            get { return offerId; }
            set { offerId = value; }
        }

        public string AdditionalInfo
        {
            get { return additionalInfo; }
            set { additionalInfo = value; }
        }
        
    }
}
