﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PDFViewer.iOS.PdfService))]
namespace PDFViewer.iOS
{
    internal class PdfService : IPdfService
    {
        public byte[] LoadPdfThumbnail(string fileName, double resolutionMultiplier = 1.0)
        {
            try
            {
                //Load stream Stream fileStream = ;
                var path = NSBundle.MainBundle.PathForResource(fileName, null);
                //var path1 = NSBundle.MainBundle.PathForResource("Ladders.compressed", ".pdf");

                var fileStream = File.OpenRead(path);

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

                UIImage pdfImage = null;

                //Get PDF's page and convert as image.
                using (CGPDFPage pdfPage = m_pdfDocument.GetPage(1))
                {
                    //initialise image context.
                    UIGraphics.BeginImageContext(pdfPage.GetBoxRect(CGPDFBox.Media).Size);
                    // get current context.
                    CGContext context = UIGraphics.GetCurrentContext();
                    context.SetFillColor(1.0f, 1.0f, 1.0f, 1.0f);

                    var width = pdfPage.GetBoxRect(CGPDFBox.Media).Width;
                    var height = pdfPage.GetBoxRect(CGPDFBox.Media).Height;
                    System.Diagnostics.Debug.WriteLine($"Page Size: {width}x{height}");

                    // Gets page's bounds.
                    CGRect bounds = new CGRect(pdfPage.GetBoxRect(CGPDFBox.Media).X, pdfPage.GetBoxRect(CGPDFBox.Media).Y, width, height);
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
                    return imageBytes;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        public List<byte[]> LoadPdfImagePages(string fileName, double resolutionMultiplier = 1.0)
        {
            try
            {
                //Load stream Stream fileStream = ;
                var path = NSBundle.MainBundle.PathForResource(fileName, null);
                //var path1 = NSBundle.MainBundle.PathForResource("Ladders.compressed", ".pdf");

                var fileStream = File.OpenRead(path);

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
                for (int i = 1; i <= pageCount; i++)
                {
                    UIImage pdfImage = null;

                    //Get PDF's page and convert as image.
                    using (CGPDFPage pdfPage = m_pdfDocument.GetPage(i))
                    {
                        var width = pdfPage.GetBoxRect(CGPDFBox.Media).Width;
                        var height = pdfPage.GetBoxRect(CGPDFBox.Media).Height;
                        var targetWidth = width * resolutionMultiplier;
                        var targetHeight = height * resolutionMultiplier;
                        var targetCgSize = new CGSize(targetWidth, targetHeight);
                        var targetRect = new CGRect(0, 0, targetWidth, targetHeight);
                        var origCGSize = pdfPage.GetBoxRect(CGPDFBox.Media).Size;

                        //initialise image context.
                        UIGraphics.BeginImageContext(targetCgSize);
                        // get current context.
                        CGContext context = UIGraphics.GetCurrentContext();
                        context.SetFillColor(1.0f, 1.0f, 1.0f, 1.0f);

                        System.Diagnostics.Debug.WriteLine($"Page Size: {targetWidth}x{targetHeight}");

                        // Gets page's bounds.
                        CGRect bounds = new CGRect(pdfPage.GetBoxRect(CGPDFBox.Media).X, pdfPage.GetBoxRect(CGPDFBox.Media).Y, width, height);
                        if (pdfPage != null)
                        {
                            context.FillRect(targetRect);
                            context.TranslateCTM(0, targetRect.Height);
                            context.ScaleCTM(1.0f, -1.0f);

                            //Trying to scale up "manually"
                            var xScale = targetCgSize.Width / bounds.Size.Width;
                            var yScale = targetCgSize.Height / bounds.Size.Height;
                            var scaleToApply = xScale < yScale ? xScale : yScale;
                            context.ConcatCTM(CGAffineTransform.MakeScale(scaleToApply, scaleToApply));

                            //Base scaling method. Doesn't scale up (above 100% - 1.0)
                            //context.ConcatCTM(pdfPage.GetDrawingTransform(CGPDFBox.Crop, bounds, 0, true));

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