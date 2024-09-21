using System.Collections.Generic;

namespace CBS.FrontDesk.Data
{
    public class DocumentType 
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string documentId { get; set; }
    }
    public class Document 
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
    public class DocumentReferenceCode
    {
        public string Id { get; set; }
        public string Document { get; set; }
        public string DocumentType { get; set; }
        public string ReferenceCode { get; set; }
        public string Description { get; set; }
        public string Interface { get; set; }
        public List<string> GrossCorrespondingAccount { get; set; }

        public List<string> GrossCorrespondingExceptionAccount { get; set; }
        public List<string> ProvCorrespondingExceptionAccount { get; set; }
        public List<string> ProvCorrespondingAccount { get; set; }

        
    }
 
    public static class BalanceSheetCartegory
    {
        public static string GROSS { get; set; } = "GROSS";
        public static string PROVISION { get; set; } = "PROVISION";

    }
    public class DocumentReferenceCodeDto
    {
        public string Id { get; set; }

        public string ReferenceCode { get; set; }

        public string Description { get; set; }

        public bool HasException { get; set; }
        public string Document{ get; set; }

        public string DocumentType { get; set; }
        public string DocumentId { get; set; }

        public string DocumentTypeId { get; set; }
    }

    public class CorrespondingMappingDto
    {
        public string Id { get; set; }
        public string ChartOfAccountId { get; set; }
        public string DocumentRefenceCodeId { get; set; }
        public string AccountNumber { get; set; }
        public string Cartegory { get; set; }
        public string EndingBalanceNature { get; set; }
    }

    public class CorrespondingMappingException
    {
        public string Id { get; set; }
        public string ChartOfAccountId { get; set; }
        public string DocumentRefenceCodeId { get; set; }
        public string AccountNumber { get; set; }
        public string Cartegory { get; set; }
        public string EndingBalanceNature { get; set; }
    }

    public class CorrespondingMappingExceptionDto
    {
        public string Id { get; set; }
        public string ChartOfAccountId { get; set; }
        public string DocumentRefenceCodeId { get; set; }
        public string AccountNumber { get; set; }
        public string Cartegory { get; set; }
        public string EndingBalanceNature { get; set; }
    }

    public class CorrespondingMapping
    {
        public string Id { get; set; }
        public string ChartOfAccountId { get; set; }
        public string DocumentRefenceCodeId { get; set; }
        public string AccountNumber { get; set; }
        public string Cartegory { get; set; }
        public string EndingBalanceNature { get; set; }
    }
}