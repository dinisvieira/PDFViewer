using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PDFViewer.iOS.PdfService))]
namespace PDFViewer.iOS
{
    internal class PdfService : IPdfService
    {
        public byte[] LoadPdfThumbnail(string fileName, double resolutionMultiplier = 1.0, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var path = NSBundle.MainBundle.PathForResource(fileName, null);

                using (var fileStream = File.OpenRead(path))
                {
                    using (var stream = new MemoryStream())
                    {
                        fileStream.CopyTo(stream);
                        CGDataProvider provider = new CGDataProvider(stream.ToArray());
                        var pdfDocument = new CGPDFDocument(provider);
                        UIImage pdfImage = null;
                        
                        if (cancellationToken.IsCancellationRequested)
                        {
                            pdfDocument?.Dispose();
                            provider?.Dispose();
                            return null;
                        }

                        using (CGPDFPage pdfPage = pdfDocument.GetPage(1))
                        {
                            if (pdfPage == null) { return null; }

                            var width = pdfPage.GetBoxRect(CGPDFBox.Media).Width;
                            var height = pdfPage.GetBoxRect(CGPDFBox.Media).Height;
                            var targetWidth = width * resolutionMultiplier;
                            var targetHeight = height * resolutionMultiplier;
                            var targetCgSize = new CGSize(targetWidth, targetHeight);
                            var targetRect = new CGRect(0, 0, targetWidth, targetHeight);

                            UIGraphics.BeginImageContext(targetCgSize);
                            var context = UIGraphics.GetCurrentContext();
                            context.SetFillColor(1.0f, 1.0f, 1.0f, 1.0f);

                            System.Diagnostics.Debug.WriteLine($"Page Size: {targetWidth}x{targetHeight}");

                            // Gets page's bounds.
                            CGRect bounds = new CGRect(pdfPage.GetBoxRect(CGPDFBox.Media).X, pdfPage.GetBoxRect(CGPDFBox.Media).Y, width, height);
                            context.FillRect(targetRect);
                            context.TranslateCTM(0, targetRect.Height);
                            context.ScaleCTM(1.0f, -1.0f);

                            //Trying to scale up "manually"
                            var xScale = targetCgSize.Width / bounds.Size.Width;
                            var yScale = targetCgSize.Height / bounds.Size.Height;
                            var scaleToApply = xScale < yScale ? xScale : yScale;
                            context.ConcatCTM(CGAffineTransform.MakeScale(scaleToApply, scaleToApply));

                            //Old base scaling method. Doesn't scale up (above 100% - 1.0)
                            //context.ConcatCTM(pdfPage.GetDrawingTransform(CGPDFBox.Crop, bounds, 0, true));

                            context.SetRenderingIntent(CGColorRenderingIntent.Default);
                            context.InterpolationQuality = CGInterpolationQuality.Default;

                            context.DrawPDFPage(pdfPage);
                            pdfImage = UIGraphics.GetImageFromCurrentImageContext();
                            UIGraphics.EndImageContext();
                        }

                        if (pdfImage == null) { return null; }//If drawing image failed we assume the entire process fails and return null

                        using (var imageData = pdfImage.AsPNG())
                        {
                            var imageBytes = new byte[imageData.Length];
                            System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, imageBytes, 0, Convert.ToInt32(imageData.Length));
                            
                            pdfDocument?.Dispose();
                            provider?.Dispose();

                            return imageBytes;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //TODO: Logger
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        public List<byte[]> LoadPdfImagePages(string fileName, double resolutionMultiplier = 1.0, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var path = NSBundle.MainBundle.PathForResource(fileName, null);
                var list = new List<byte[]>();
                using (var fileStream = File.OpenRead(path))
                {
                    using (var stream = new MemoryStream())
                    {
                        fileStream.CopyTo(stream);
                        CGDataProvider provider = new CGDataProvider(stream.ToArray());
                        var pdfDocument = new CGPDFDocument(provider);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            pdfDocument?.Dispose();
                            provider?.Dispose();
                            return null;
                        }

                        nint pageCount = pdfDocument.Pages;
                        for (int i = 1; i <= pageCount; i++)
                        {
                            UIImage pdfImage = null;
                            using (CGPDFPage pdfPage = pdfDocument.GetPage(i))
                            {
                                if (pdfPage == null) { return null; } //If on pdfPage is null we assume the entire process fails and return null

                                if (cancellationToken.IsCancellationRequested)
                                {
                                    pdfDocument?.Dispose();
                                    provider?.Dispose();
                                    return null;
                                }

                                var width = pdfPage.GetBoxRect(CGPDFBox.Media).Width;
                                var height = pdfPage.GetBoxRect(CGPDFBox.Media).Height;
                                var targetWidth = width * resolutionMultiplier;
                                var targetHeight = height * resolutionMultiplier;
                                var targetCgSize = new CGSize(targetWidth, targetHeight);
                                var targetRect = new CGRect(0, 0, targetWidth, targetHeight);

                                UIGraphics.BeginImageContext(targetCgSize);
                                var context = UIGraphics.GetCurrentContext();
                                context.SetFillColor(1.0f, 1.0f, 1.0f, 1.0f);

                                System.Diagnostics.Debug.WriteLine($"Page Size: {targetWidth}x{targetHeight}");

                                // Gets page's bounds.
                                CGRect bounds = new CGRect(pdfPage.GetBoxRect(CGPDFBox.Media).X, pdfPage.GetBoxRect(CGPDFBox.Media).Y, width, height);
                                context.FillRect(targetRect);
                                context.TranslateCTM(0, targetRect.Height);
                                context.ScaleCTM(1.0f, -1.0f);

                                //Trying to scale up "manually"
                                var xScale = targetCgSize.Width / bounds.Size.Width;
                                var yScale = targetCgSize.Height / bounds.Size.Height;
                                var scaleToApply = xScale < yScale ? xScale : yScale;
                                context.ConcatCTM(CGAffineTransform.MakeScale(scaleToApply, scaleToApply));

                                //Old base scaling method. Doesn't scale up (above 100% - 1.0)
                                //context.ConcatCTM(pdfPage.GetDrawingTransform(CGPDFBox.Crop, bounds, 0, true));

                                context.SetRenderingIntent(CGColorRenderingIntent.Default);
                                context.InterpolationQuality = CGInterpolationQuality.Default;

                                context.DrawPDFPage(pdfPage);
                                pdfImage = UIGraphics.GetImageFromCurrentImageContext();
                                UIGraphics.EndImageContext();
                            }

                            if (pdfImage == null) { return null; }//If drawing image failed we assume the entire process fails and return null

                            // Get bytes from UIImage object.
                            using (var imageData = pdfImage.AsPNG())
                            {
                                var imageBytes = new byte[imageData.Length];
                                System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, imageBytes, 0, Convert.ToInt32(imageData.Length));
                                list.Add(imageBytes);
                            }
                        }

                        pdfDocument?.Dispose();
                        provider?.Dispose();

                        return list;
                    }
                }
            }
            catch (Exception e)
            {
                //TODO: Logger
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }
    }
}