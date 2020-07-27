using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;


namespace FunctionApp1
{
    public class CostMasterEntity :TableEntity
    {
        public CostMasterEntity(string Year, string Row)
        {
            this.PartitionKey = Year;
            this.RowKey = Row;
        }
        public CostMasterEntity() { }

        private string lastProcessedDate;
        //public void AssignRowKey()
        //{
        //    this.RowKey = "LastProcessed";
        //}
        //public void AssignPartitionKey()
        //{
        //    this.PartitionKey = "1";
        //}
        public string LastProcessedDate
        {
            get
            { return lastProcessedDate; }

            set
            { lastProcessedDate = value; }
        }
    }
}
