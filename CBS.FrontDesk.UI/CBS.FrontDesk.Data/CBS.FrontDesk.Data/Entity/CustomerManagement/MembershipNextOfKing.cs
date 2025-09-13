using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.CustomerManagement
{
    public class MembershipNextOfKing
    {
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string CustomerId { get; set; }
        [Required]
        public string Relation { get; set; }
        [Required]
        public string Ratio { get; set; }
        public string SignatureUrl { get; set; }
        public string PhotoUrl { get; set; }
        public string BranchId { get; set; }
    }
    public class CardSignatureSpecimen
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string IHereByTestifyforAllTheSignatures { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]

        public string IdentityCardNumber { get; set; }
        [Required]

        public string IssuedAt { get; set; }
        [Required]

        public string IssuedOn { get; set; }
        public string SignatureUrl1 { get; set; }
        public string SignatureUrl2 { get; set; }
        public string PhotoUrl1 { get; set; }
        [Required]

        public string Instruction { get; set; }
        public string SpecialInstruction { get; set; }
        public IndividualProfile Customer { get; set; }
    }
    //public class CardSignatureSpecimenDocument
    //{
    //    public string cardSignatureSpecimenDetailId { get; set; }
    //    public string documentType { get; set; }
    //    public HttpFileCollectionBase file { get; set; }
    //}

    public class DocumentRequest
    {
        public string id { get; set; }
        public string urlPath { get; set; }
        public string documentName { get; set; }
        public string extension { get; set; }
        public string baseUrl { get; set; }
        public string documentType { get; set; }//NextofkingsPhoto,NextofkingsSignature,CustomerPhoto,CustomerSignature,CustomerOtherDocument
    }

    public class DocumentBaseURL
    {
        public string id { get; set; }
        public string baseURL { get; set; }
    }

}
