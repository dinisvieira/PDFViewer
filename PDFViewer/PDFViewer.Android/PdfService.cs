using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Pdf;
using Android.OS;
using Xamarin.Essentials;
using Xamarin.Forms;
using File = System.IO.File;

[assembly: Dependency(typeof(PDFViewer.Droid.PdfService))]
namespace PDFViewer.Droid
{
    internal class PdfService : IPdfService
    {
        public byte[] LoadPdfThumbnail(string fileName, double resolutionMultiplier = 1.0, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var fileDescriptor = GetSeekableFileDescriptor(fileName);
                PdfRenderer renderer = new PdfRenderer(fileDescriptor); 
                int pageCount = renderer.PageCount;
                if(pageCount > 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        renderer?.Close();
                        fileDescriptor?.Close();
                        return null;
                    }

                    //We only want a thumbnail so we just get the first one
                    PdfRenderer.Page page =  renderer.OpenPage(0);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        page?.Close();
                        renderer?.Close();
                        fileDescriptor?.Close();
                        return null;
                    }
                    else if (page != null)
                    {
                        var width = page.Width * resolutionMultiplier;
                        var height = page.Height * resolutionMultiplier;

                        //Creates bitmap
                        Bitmap bmp = Bitmap.CreateBitmap((int)width, (int)height, Bitmap.Config.Argb8888); 
                    
                        //Renders page as bitmap
                        page.Render(bmp, null, null, PdfRenderMode.ForDisplay);
                    
                        //Save the bitmap
                        using (var stream = new MemoryStream()) 
                        {
                            bmp.Compress(Bitmap.CompressFormat.Png, 0, stream);
                            byte[] bitmapData = stream.ToArray();

                            page.Close();
                            renderer?.Close();
                            fileDescriptor?.Close();

                            return bitmapData;
                        }
                    }
                }

                renderer?.Close();
                fileDescriptor?.Close();

                return null;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        public List<byte[]> LoadPdfImagePages(string fileName, double resolutionMultiplier = 1.0, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var imagesList = new List<byte[]>();
                var fileDescriptor = GetSeekableFileDescriptor(fileName);
                PdfRenderer renderer = new PdfRenderer(fileDescriptor); 

                if (cancellationToken.IsCancellationRequested)
                {
                    renderer?.Close();
                    fileDescriptor?.Close();
                    return null;
                }

                int pageCount = renderer.PageCount;
                for(int i = 0 ; i < pageCount ; i++)
                {
                    PdfRenderer.Page page =  renderer.OpenPage(i);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        page?.Close();
                        renderer?.Close();
                        fileDescriptor?.Close();
                        return null;
                    }

                    if (page != null)
                    {
                        var width = page.Width * resolutionMultiplier;
                        var height = page.Height * resolutionMultiplier;
                        System.Diagnostics.Debug.WriteLine($"Page Size: {width}x{height}");

                        //Creates bitmap
                        Bitmap bmp = Bitmap.CreateBitmap((int)width, (int)height, Bitmap.Config.Argb8888); 
                    
                        //Renders page as bitmap
                        page.Render(bmp, null, null, PdfRenderMode.ForDisplay);
                    
                        //Save the bitmap
                        using (var stream = new MemoryStream()) 
                        {
                            bmp.Compress(Bitmap.CompressFormat.Png, 0, stream);
                            byte[] bitmapData = stream.ToArray();
                            page.Close();
                            imagesList.Add(bitmapData);
                        }
                    }
                }

                renderer?.Close();
                fileDescriptor?.Close();

                return imagesList;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        //Method to retrieve PDF file from the location
        private ParcelFileDescriptor GetSeekableFileDescriptor(string fileName)
        {
            ParcelFileDescriptor fileDescriptor = null;
            try
            {
                var activity = Platform.CurrentActivity;

                string pdfPath = null;
                AssetManager assets = activity.Assets;
                using (Stream sr = (assets.Open(fileName)))
                {
                    string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                    string localFilename = fileName;
                    pdfPath = System.IO.Path.Combine(path, localFilename);
                    File.WriteAllBytes(pdfPath, ReadFully(sr));
                }

                var file = new Java.IO.File(pdfPath);
                fileDescriptor = ParcelFileDescriptor.Open(file, ParcelFileMode.ReadOnly);
            }
            catch (Java.IO.FileNotFoundException e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            catch (System.IO.FileNotFoundException e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            return fileDescriptor;
        }

        private static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16*1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}