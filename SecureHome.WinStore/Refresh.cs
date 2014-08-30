using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using SecureHome.WinStore.DataModel;

namespace SecureHome.WinStore
{
    class Refresh
    {
        //Camera uri
        private string _uri = "http://10.0.0.9:60001/cgi-bin/snapshot.cgi?chn=0&u=berndt73&p=berndt74&q=0&d=1";
        private DispatcherTimer _dispatcherTimer;
        //Get the images from Azure, or from HTTP?
        private bool _azureReadMode = false;
        //Store the image in Azure?
        private bool _storeInAzure = true;

        public PictureDataSource _pictureDataSource = new PictureDataSource();
        private bool _uploadInprogress;

        private TextBlock CurrentDateTextBlock;
        private Windows.UI.Xaml.Controls.Image WebView0x1;

        public Refresh(Image image, TextBlock info)
        {
            CurrentDateTextBlock = info;
            WebView0x1 = image;
        }

        public void DispatcherTimerSetup()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += dispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
            _dispatcherTimer.Start();
        }

        public void Start()
        {
            DispatcherTimerSetup();
        }

        void dispatcherTimer_Tick(object sender, object e)
        {
            RefreshCams();
        }


        internal async void RefreshCams()
        {
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            var httpClient = new HttpClient(handler);

            int currentCam = 1;

            try
            {
#if WINDOWS_APP
                InMemoryRandomAccessStream ims = null;
#elif WINDOWS_PHONE 
                MemoryStream ims = null;
#endif

                if (_azureReadMode)
                {
                    //TODO: Read byte[] from Azure
                    await _pictureDataSource.GetPictureInfoFromTableStorage();
                    var pictures = _pictureDataSource.GetAllImages();
                    if (pictures.Count > 0)
                    {
                        ims = await _pictureDataSource.DownloadPictureFromCloud(pictures[0]);
                    }
                }
                else
                {
                    HttpResponseMessage reponse = await httpClient.GetAsync(new Uri(_uri, UriKind.Absolute));
                    byte[] contentBytes = reponse.Content.ReadAsByteArrayAsync().Result;
#if WINDOWS_APP
                    ims = new InMemoryRandomAccessStream();
                    var dataWriter = new DataWriter(ims);
                    dataWriter.WriteBytes(contentBytes);
                    await dataWriter.StoreAsync();
#elif WINDOWS_PHONE 
                    ims = new MemoryStream(contentBytes);
#endif

                    if (!_uploadInprogress)
                        AddToAzureSaveList(currentCam, contentBytes);
                }

#if WINDOWS_APP
                ims.Seek(0);
#elif WINDOWS_PHONE 
                ims.Seek(0, SeekOrigin.Begin);
#endif

                //if(ims.Size == 1081)
                //    continue;

                var bitmap = new BitmapImage();
                bitmap.SetSource(ims);

                WebView0x1.Source = bitmap;

                if (!_azureReadMode && _storeInAzure && !_uploadInprogress)
                {
                    _uploadInprogress = true;
                    //Upload picture to Azure
                    if (!await _pictureDataSource.UploadPicturesToCloud())
                    {
                        MessageDialog messageBox = new MessageDialog("Failed to upload, please try it again later.");
                        await messageBox.ShowAsync();
                    }
                    _pictureDataSource.ClearImages();
                    _uploadInprogress = false;
                }
                CurrentDateTextBlock.Text = DateTime.Now.ToString();

            }
            catch (Exception ex)
            {
                Debug.WriteLine("*ERROR: " + ex.Message);

            }
        }

        private async void AddToAzureSaveList(int camNum, byte[] image)
        {
            //Remember byte[] to upload more cam pics in the future
            var uploadForm = new PictureViewModel();
            uploadForm.Name = camNum.ToString();
            uploadForm.PictureFile = image;
            _pictureDataSource.AddImage(uploadForm);
        }

    }

}
