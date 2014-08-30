/***************************** Module Header ******************************\
* Module Name:	PictureDataSource.cs
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
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

#if WINDOWS_PHONE 
using SecureHome.WinPhone;
#endif

namespace SecureHome.WinStore.DataModel
{
    public class PictureDataSource
    {

        private ObservableCollection<PictureViewModel> allImages = new ObservableCollection<PictureViewModel>();


        public PictureDataSource()
        {
            //GetPictureInfoFromTableStorage();
        }

        public ObservableCollection<PictureViewModel> AllImages
        {
            get { return allImages; }
        }
        public static async Task<bool> UploadPictureToCloud(PictureViewModel pictureViewModel, byte[] image)
        {

            try
            {
                var blockBlobClient = App.account.CreateCloudBlobClient();
                var contianer = blockBlobClient.GetContainerReference(App.contianerName);
                await contianer.CreateIfNotExistsAsync();
                string blobReference = Guid.NewGuid().ToString();
                CloudBlockBlob picture = contianer.GetBlockBlobReference(blobReference);

                await picture.UploadFromByteArrayAsync(image, 0, image.Length);
                //await picture.UploadFromStreamAsync(image);

                pictureViewModel.PictureUrl = picture.Uri.ToString();

                var tableClient = App.account.CreateCloudTableClient();
                var table = tableClient.GetTableReference(App.tableName);
                await table.CreateIfNotExistsAsync();

                var operation = TableOperation.Insert(pictureViewModel.PictureTableEntity);
                await table.ExecuteAsync(operation);

                return true;
            }
            catch (Exception)
            {
                throw;
            }

        }

        internal async Task<bool> UploadPicturesToCloud()
        {
            try
            {
                var blockBlobClient = App.account.CreateCloudBlobClient();
                var contianer = blockBlobClient.GetContainerReference(App.contianerName);
                await contianer.CreateIfNotExistsAsync();

                var tableClient = App.account.CreateCloudTableClient();
                var table = tableClient.GetTableReference(App.tableName);
                await table.CreateIfNotExistsAsync();

                foreach (var pictureViewModel in allImages)
                {

                    string blobReference = pictureViewModel.Name + "_" + Guid.NewGuid().ToString();
                    CloudBlockBlob picture = contianer.GetBlockBlobReference(blobReference);
                    var image = pictureViewModel.PictureFile;
                    await picture.UploadFromByteArrayAsync(image, 0, image.Length);
                    //await picture.UploadFromStreamAsync(image);

                    pictureViewModel.PictureUrl = picture.Uri.ToString();


                    var operation = TableOperation.InsertOrReplace(pictureViewModel.PictureTableEntity);
                    await table.ExecuteAsync(operation);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return true;
        }


        public async Task GetPictureInfoFromTableStorage()
        {
            var client = App.account.CreateCloudTableClient();
            var table = client.GetTableReference(App.tableName);
            await table.CreateIfNotExistsAsync();

            var results = await table.ExecuteQuerySegmentedAsync(new TableQuery(), null);
            foreach (var item in results)
            {
                allImages.Add(new PictureViewModel() { PictureTableEntity = item });
            }
        }

        public void ClearImages()
        {
            if (allImages != null)
                allImages.Clear();
        }


        public ObservableCollection<PictureViewModel> GetAllImages()
        {
            return allImages;
        }

        public async Task<bool> DeletePictureFormCloud(PictureViewModel pictureViewModel)
        {
            try
            {
                var blob = new CloudBlockBlob(new Uri(pictureViewModel.PictureUrl), App.credentials);
                await blob.DeleteAsync();

                var tableClient = App.account.CreateCloudTableClient();
                var table = tableClient.GetTableReference(App.tableName);

                var operation = TableOperation.Delete(pictureViewModel.PictureTableEntity);
                await table.ExecuteAsync(operation);

                allImages.Remove(pictureViewModel);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

#if WINDOWS_APP
        public async Task<InMemoryRandomAccessStream> DownloadPictureFromCloud(PictureViewModel pictureViewModel)
        {
            try
            {
                var blob = new CloudBlockBlob(new Uri(pictureViewModel.PictureUrl), App.credentials);
                var ims = new InMemoryRandomAccessStream();
                await blob.DownloadToStreamAsync(ims);
                return ims;
            }
            catch (Exception)
            {
                throw;
            }
        }
#elif WINDOWS_PHONE
        public async Task<MemoryStream> DownloadPictureFromCloud(PictureViewModel pictureViewModel)
        {
            try
            {
                var blob = new CloudBlockBlob(new Uri(pictureViewModel.PictureUrl), App.credentials);
                var ims = new MemoryStream();
                await blob.DownloadToStreamAsync(ims);
                return ims;
            }
            catch (Exception)
            {
                throw;
            }
        }
#endif

        internal void AddImage(PictureViewModel uploadForm)
        {
            allImages.Add(uploadForm);
        }

    }
}
