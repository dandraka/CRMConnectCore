using Microsoft.PowerPlatform.Cds.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Security;

namespace Dandraka.CRMConnectCore
{
    public class CRMConnector
    {
        private CdsServiceClient client;

        /// <summary>
        /// Connect to Microsoft CRM
        /// </summary>
        /// <param name="instanceUrl">The base CRM Url, e.g. https://megacorp.crm.dynamics.com/ </param>
        /// <param name="clientId">An Azure app client id, see https://docs.microsoft.com/en-us/powerapps/developer/data-platform/walkthrough-register-app-azure-active-directory </param>
        /// <param name="clientSecret">An Azure app client secret</param>
        public CRMConnector(string instanceUrl, string clientId, SecureString clientSecret)
        {
            this.client = new CdsServiceClient(new Uri(instanceUrl), clientId, clientSecret, false);
        }

        /// <summary>
        /// Gets records from Microsoft CRM based on a Fetch XML.
        /// </summary>
        /// <param name="fetchXml">The Fetch XML</param>
        /// <returns>A collection of CRM records</returns>
        public EntityCollection GetCRMRecords(string fetchXml)
        {
            var query = new Microsoft.Xrm.Sdk.Query.FetchExpression(fetchXml);
            return this.client.RetrieveMultiple(query);
        }

        /// <summary>
        /// Updates a CRM record.
        /// </summary>
        /// <param name="entityName">Logical name of the entity, e.g. contact</param>
        /// <param name="crmId">The CRM guid</param>
        /// <param name="properties">Property names and values to change</param>
        public void UpdateCRMRecord(string entityName, Guid crmId, Dictionary<string, object> properties)
        {
            var entity = new Entity(entityName, crmId);
            foreach (var prop in properties)
            {
                entity.Attributes.Add(new KeyValuePair<string, object>(prop.Key, prop.Value));
            }
            UpdateCRMRecord(entity);
        }

        /// <summary>
        /// Updates a CRM record.
        /// </summary>
        /// <param name="entity">The CRM entity</param>
        public void UpdateCRMRecord(Entity entity)
        {
            this.client.Update(entity);
        }

        /// <summary>
        /// Updates many CRM records.
        /// </summary>
        /// <param name="entity">The CRM entity collection</param>
        public void UpdateCRMRecords(EntityCollection entities)
        {
            foreach (var entity in entities.Entities)
            {
                UpdateCRMRecord(entity);
            }
        }
    }
}
