using System.Collections.Generic;

namespace PDFViewer
{
    public interface IPdfService
    {
        byte[] LoadPdfThumbnail(string fileName);

        List<byte[]> LoadPdfImagePages(string fileName);
    }
}
