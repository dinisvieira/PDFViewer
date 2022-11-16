using System.Collections.Generic;
using System.Threading;

namespace PDFViewer
{
    public interface IPdfService
    {
        /// <summary>
        /// Returns an image for the first page of a PDF
        /// </summary>
        /// <param name="fileName">Path to the PDF</param>
        /// <param name="resolutionMultiplier">Quality control. '1.0' is default quality. Recommended range is '0.25' (very low quality) to '2.0' (very good quality but slow)</param>
        /// <returns>Returns an array of bytes representing an image. Or null if something fails.</returns>
        byte[] LoadPdfThumbnail(string fileName, double resolutionMultiplier = 1.0, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns a list of images that correspond to the pages of the given PDF
        /// </summary>
        /// <param name="fileName">Path to the PDF</param>
        /// <param name="resolutionMultiplier">Quality control. '1.0' is default quality. Recommended range is '0.25' (very low quality) to '2.0' (very good quality but slow)</param>
        /// <returns>Returns a list of 'array of bytes' that represent images. Or null if something fails.</returns>
        List<byte[]> LoadPdfImagePages(string fileName, double resolutionMultiplier = 1.0, CancellationToken cancellationToken = default(CancellationToken));
    }
}
