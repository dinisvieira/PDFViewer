using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        public byte[] LoadPdfThumbnail(string fileName)
        {
            try
            {
                //initialize PDFRenderer by passing PDF file from location.
                PdfRenderer renderer = new PdfRenderer(GetSeekableFileDescriptor(fileName)); 
                int pageCount = renderer.PageCount;
                for(int i = 0 ; i < pageCount ; i++)
                {
                    // Use `openPage` to open a specific page in PDF.
                    Android.Graphics.Pdf.PdfRenderer.Page page =  renderer.OpenPage(i);
                    
                    //Creates bitmap
                    Bitmap bmp = Bitmap.CreateBitmap(page.Width, page.Height, Bitmap.Config.Argb8888); 
                    
                    //renderers page as bitmap, to use portion of the page use second and third parameter
                    page.Render(bmp, null, null, PdfRenderMode.ForDisplay);
                    
                    //Save the bitmap
                    using (var stream = new MemoryStream()) {
                        bmp.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        byte[] bitmapData = stream.ToArray();
                        page.Close();
                        return bitmapData; //we only return this page in this case
                    }
                }

                return null;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        public List<byte[]> LoadPdfImagePages(string fileName)
        {
            try
            {
                var list = new List<byte[]>();
                //initialize PDFRenderer by passing PDF file from location.
                PdfRenderer renderer = new PdfRenderer(GetSeekableFileDescriptor(fileName)); 
                int pageCount = renderer.PageCount;
                for(int i = 0 ; i < pageCount ; i++)
                {
                    // Use `openPage` to open a specific page in PDF.
                    Android.Graphics.Pdf.PdfRenderer.Page page =  renderer.OpenPage(i);
                    
                    //Creates bitmap
                    Bitmap bmp = Bitmap.CreateBitmap(page.Width, page.Height, Bitmap.Config.Argb8888); 
                    
                    //renderers page as bitmap, to use portion of the page use second and third parameter
                    page.Render(bmp, null, null, PdfRenderMode.ForDisplay);
                    
                    //Save the bitmap
                    using (var stream = new MemoryStream()) {
                        bmp.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        byte[] bitmapData = stream.ToArray();
                        page.Close();
                        list.Add(bitmapData);
                    }
                }

                return list;
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