using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.CustomerManagement
{
    public class MembershipNextOfKingsMember
    {
        public string id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string customerId { get; set; }
        [Required]
        public string accountNumber { get; set; }
        [Required]
        public string relation { get; set; }
        [Required]
        public string ratio { get; set; }
        public string signatureFile { get; set; }
        [Required]
        public string branchId { get; set; }
    }
    public class CardSignatureSpecimen
    {
        public string id { get; set; }
        [Required]
        public string customerId { get; set; }
        [Required]
        public string accountNumber { get; set; }
        [Required]
        public string branchId { get; set; }
        [Required]
        public string branchMangerId { get; set; }
        public string iHererByTestifyforAllTheSignatures { get; set; }
        public CardSignatureSpecimenDetail cardSignatureSpecimenDetail { get; set; }=new CardSignatureSpecimenDetail();
        public List<CardSignatureSpecimenDetail> cardSignatureSpecimenDetails { get; set; }=new List<CardSignatureSpecimenDetail>();
    }
    public class CardSignatureSpecimenDetail
    {
        public string id { get; set; }
        [Required]
        public string cardSignatureSpecimenId { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string identityCardNumber { get; set; }
        [Required]
        public string issuedAt { get; set; }
        [Required]
        public string issuedOn { get; set; }

        public string signatureUrl1 { get; set; }
        public string signatureUrl2 { get; set; }
        public string photoUrl1 { get; set; }
        [Required]
        public string instruction { get; set; }
    }
    public class CardSignatureSpecimenDocument
    {
        public string cardSignatureSpecimenDetailId { get; set; }
        public string documentType { get; set; }
        public HttpFileCollectionBase file { get; set; }
    }

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
