using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class DocumentConfiguration
    {
        public Document Document { get; set; }
        public DocumentType DocumentType { get; set; }
        public DocReferenceCode DocumentReferenceCode { get; set; }
        public CorrespondingMapping CorrespondingMapping { get; set; }

        public CorrespondingMappingException CorrespondingMappingException { get; set; }
        public List<Document> Documents { get; set; } = new List<Document>();
        public List<DocumentType> DocumentTypes { get; set; } = new List<DocumentType>();
        public List<DocumentTypeDto> DocumentTypeDtos { get; set; } = new List<DocumentTypeDto>();
        public List<DocumentReferenceCodeDto> DocumentReferenceCodes { get; set; } = new List<DocumentReferenceCodeDto>();

        public List<CorrespondingMapping> CorrespondingMappings { get; set; } = new List<CorrespondingMapping>();

        public List<CorrespondingMappingException> CorrespondingMappingExceptions { get; set; } = new List<CorrespondingMappingException>();
        public string ServiceOption { get; set; }
        public DocumentReferenceCodeDto DocumentReferenceCodeDto { get; set; }
        public List<CorrespondingMappingDto> CorrespondingMappingDataDto { get; set; }
        public List<CorrespondingMappingExceptionDto> CorrespondingMappingExceptionDataDto { get; set; }
        public CorrespondingMappingDto CorrespondingMappingDto { get; set; }
    }
}
