/***************************** Module Header ******************************\
* Module Name:	PictureViewModel.cs
* Project:		CSAzureWin8WithAzureStorage
* Copyright (c) Microsoft Corporation.
* 
* This sample shows how to store images to Windows Azure Blob storage,
* and save image information to table storage.
* 
* This source is subject to the Microsoft Public License.
* See http://www.microsoft.com/en-us/openness/licenses.aspx#MPL.
* All other rights reserved.
* 
* THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
* EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
* WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\**************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace SecureHome.WinStore.DataModel
{
    public class PictureViewModel
    {
        private DynamicTableEntity entity;

        public PictureViewModel()
        {
            //TODO:This username should be changed to User account in real app
            this.entity = new DynamicTableEntity() { PartitionKey = "cam" };
        }

        public string Name { 
            get
            {
                return entity.RowKey;
            }
            set
            {
                entity.RowKey = value;
            }
        }
        public string PictureUrl {
            get
            {
                return entity.Properties["ImageUrl"].StringValue;
            }
            set
            {
                entity.Properties.Add(new KeyValuePair<string, EntityProperty>("ImageUrl", new EntityProperty(value)));
            }
        }

        public DateTime CreationDate
        {
            get
            {
                return DateTime.FromFileTime(Convert.ToInt64(entity.RowKey));
            }
            set
            {
                entity.RowKey = value.ToFileTime().ToString();
            }
        }

        public string Description
        {
            get
            {
                return entity.Properties["Description"].StringValue;
            }
            set
            {
                entity.Properties.Add(new KeyValuePair<string, EntityProperty>("Description", new EntityProperty(value)));
            }
        }

        public DynamicTableEntity PictureTableEntity { get { return entity; } set {
            entity = value;
        } }
        public byte[] PictureFile { get; set; }
     
    }
}
