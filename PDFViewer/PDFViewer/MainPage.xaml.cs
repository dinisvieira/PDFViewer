using System;
using System.Collections.ObjectModel;
using System.IO;
using Xamarin.Forms;

namespace PDFViewer
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void ButtonLadders_OnClicked(object sender, EventArgs e)
        {
            IPdfService service = DependencyService.Get<IPdfService>();
            var byteArrList = service.LoadPdfImagePages("Ladders.compressed.pdf");

            var imgPages = new ObservableCollection<ImageItem>();
            var isThumbnail = true;
            foreach (var byteArr in byteArrList)
            {
                if (isThumbnail)
                {
                    isThumbnail = false;
                    var streamSource = ImageSource.FromStream(() => new MemoryStream(byteArr));
                    ImageViewThumbnail.Source = streamSource;
                }

                imgPages.Add(new ImageItem(byteArr));
            }
            PdfCollectionView.ItemsSource = imgPages;
        }

        private void Button10_OnClicked(object sender, EventArgs e)
        {
            IPdfService service = DependencyService.Get<IPdfService>();
            var byteArrList = service.LoadPdfImagePages("10page.pdf");

            var isThumbnail = true;
            var imgPages = new ObservableCollection<ImageItem>();
            foreach (var byteArr in byteArrList)
            {
                if (isThumbnail)
                {
                    isThumbnail = false;
                    var streamSource = ImageSource.FromStream(() => new MemoryStream(byteArr));
                    ImageViewThumbnail.Source = streamSource;
                }

                imgPages.Add(new ImageItem(byteArr));
            }
            PdfCollectionView.ItemsSource = imgPages;
        }

        private void Button100_OnClicked(object sender, EventArgs e)
        {
            IPdfService service = DependencyService.Get<IPdfService>();
            var byteArrList = service.LoadPdfImagePages("100page.pdf");

            var isThumbnail = true;
            var imgPages = new ObservableCollection<ImageItem>();
            foreach (var byteArr in byteArrList)
            {
                if (isThumbnail)
                {
                    isThumbnail = false;
                    var streamSource = ImageSource.FromStream(() => new MemoryStream(byteArr));
                    ImageViewThumbnail.Source = streamSource;
                }

                imgPages.Add(new ImageItem(byteArr));
            }
            PdfCollectionView.ItemsSource = imgPages;
        }
    }

    public class ImageItem
    {
        private byte[] _byteArr;

        public ImageItem(byte[] byteArr)
        {
            _byteArr = byteArr;
        }

        public ImageSource Image => ImageSource.FromStream(() => new MemoryStream(_byteArr));
    }
}
