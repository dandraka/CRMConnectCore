using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using Xunit;

namespace Dandraka.CRMConnectCore.Tests
{
    public class CRMConnectorTests
    {
        private TestsConfig testConfig = new TestsConfig();

        private CRMConnector connector;

        private void Connect()
        {
            if (this.connector == null)
            {
                this.connector = new CRMConnector(
                    testConfig.CRMUrl,
                    testConfig.ClientID,
                    testConfig.ClientSecret);
            }
        }

        private EntityCollection GetContacts(int top)
        {
            string fetchXml = $"<fetch top='{top}'><entity name='contact'><attribute name='contactid'/><attribute name='firstname'/><attribute name='lastname'/></entity></fetch>";
            var records = this.connector.GetCRMRecords(fetchXml);
            return records;
        }

        [Fact]
        public void T01_CreateConnection()
        {
            Connect();
        }

        [Fact]
        public void T02_GetViaFetchXml()
        {
            Connect();
            EntityCollection records = GetContacts(5);

            Assert.NotNull(records);
            Assert.Equal(5, records.Entities.Count);
        }

        [Fact]
        public void T03_UpdateRecord()
        {
            Connect();
            EntityCollection records = GetContacts(1);
            var record = records[0];

            string before = Convert.ToString(record.Attributes["firstname"]);
            if (before.Contains(" TEST"))
            {
                before = before.Remove(before.IndexOf(" TEST"));
            }
            string after = before + " TEST" + DateTime.Now.Millisecond.ToString();
            var properties = new Dictionary<string, object>();
            properties.Add("firstname", after);

            this.connector.UpdateCRMRecord(record.LogicalName,
                record.Id,
                properties);

            EntityCollection recordsAfter = GetContacts(1);
            var recordAfter = recordsAfter[0];

            Assert.Equal(after, Convert.ToString(recordAfter.Attributes["firstname"]));
        }
    }
}
