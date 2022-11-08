using System.Collections.Generic;

namespace PDFViewer
{
    public interface IPdfService
    {
        byte[] LoadPdfThumbnail(string fileName, double resolutionMultiplier = 1.0);

        List<byte[]> LoadPdfImagePages(string fileName, double resolutionMultiplier = 1.0);
    }
}
