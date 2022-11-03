using System;
using System.Collections.Generic;
using System.IO;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PDFViewer.iOS.PdfService))]
namespace PDFViewer.iOS
{
    internal class PdfService : IPdfService
    {
        public byte[] LoadPdfThumbnail(string fileName)
        {
            try
            {
                //initialize PDFRenderer by passing PDF file from location.
                //PdfRenderer renderer = new PdfRenderer(GetSeekableFileDescriptor(fileName)); 
                //int pageCount = renderer.PageCount;
                //for(int i = 0 ; i < pageCount ; i++)
                //{
                //    // Use `openPage` to open a specific page in PDF.
                //    Android.Graphics.Pdf.PdfRenderer.Page page =  renderer.OpenPage(i);
                    
                //    //Creates bitmap
                //    Bitmap bmp = Bitmap.CreateBitmap(page.Width, page.Height, Bitmap.Config.Argb8888); 
                    
                //    //renderers page as bitmap, to use portion of the page use second and third parameter
                //    page.Render(bmp, null, null, PdfRenderMode.ForDisplay);
                    
                //    //Save the bitmap
                //    using (var stream = new MemoryStream()) {
                //        bmp.Compress(Bitmap.CompressFormat.Png, 0, stream);
                //        byte[] bitmapData = stream.ToArray();
                //        page.Close();
                //        return bitmapData; //we only return this page in this case
                //    }
                //}

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
                //Load stream Stream fileStream = ;
                var path = NSBundle.MainBundle.PathForResource(fileName, null);
                var path1 = NSBundle.MainBundle.PathForResource("Ladders.compressed", ".pdf");

                var fileStream = File.OpenRead(path1);

                var list = new List<byte[]>();
                MemoryStream stream = new MemoryStream();

                // Create memory stream from file stream.
                fileStream.CopyTo(stream);

                // Create data provider from bytes.
                CGPDFDocument m_pdfDocument = null;
                CGDataProvider provider = new CGDataProvider(stream.ToArray());
                try
                {
                    //Load a PDF file.
                    m_pdfDocument = new CGPDFDocument(provider);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }

                nint pageCount = m_pdfDocument.Pages;
                for (int i = 0; i < pageCount; i++)
                {
                    UIImage pdfImage = null;

                    //Get PDF's page and convert as image.
                    using (CGPDFPage pdfPage = m_pdfDocument.GetPage(i))
                    {
                        //initialise image context.
                        UIGraphics.BeginImageContext(pdfPage.GetBoxRect(CGPDFBox.Media).Size);
                        // get current context.
                        CGContext context = UIGraphics.GetCurrentContext();
                        context.SetFillColor(1.0f, 1.0f, 1.0f, 1.0f);
                        // Gets page's bounds.
                        CGRect bounds = new CGRect(pdfPage.GetBoxRect(CGPDFBox.Media).X, pdfPage.GetBoxRect(CGPDFBox.Media).Y, pdfPage.GetBoxRect(CGPDFBox.Media).Width, pdfPage.GetBoxRect(CGPDFBox.Media).Height);
                        if (pdfPage != null)
                        {
                            context.FillRect(bounds);
                            context.TranslateCTM(0, bounds.Height);
                            context.ScaleCTM(1.0f, -1.0f);
                            context.ConcatCTM(pdfPage.GetDrawingTransform(CGPDFBox.Crop, bounds, 0, true));
                            context.SetRenderingIntent(CGColorRenderingIntent.Default);
                            context.InterpolationQuality = CGInterpolationQuality.Default;
                            // Draw PDF page in the context.
                            context.DrawPDFPage(pdfPage);
                            // Get image from current context.
                            pdfImage = UIGraphics.GetImageFromCurrentImageContext();
                            UIGraphics.EndImageContext();
                        }
                    }

                    // Get bytes from UIImage object.
                    using (var imageData = pdfImage.AsPNG())
                    {
                        var imageBytes = new byte[imageData.Length];
                        System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, imageBytes, 0, Convert.ToInt32(imageData.Length));
                        list.Add(imageBytes);
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
    }
}