using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{

    public class PcmfLoanPurpose
    {
        public string Id { get; set; }
        public PcmfPurposeKey PcmfPurposeKey { get; set; }
        public string NameEn { get; set; }
        public string NameFr { get; set; }
        public bool IsActive { get; set; }
        public int? LtGroupCode { get; set; }
        public int? MtGroupCode { get; set; }
        public int? CtGroupCode { get; set; }
        public int? OdGroupCode { get; set; }

        public string Keys =>
      PcmfPurposeKey.ToString();
    }

  

    public class UpdatePcmfLoanPurposeRequest
    {
        public string Id { get; set; }
        public int Key { get; set; }
        public string NameEn { get; set; }
        public string NameFr { get; set; }
        public bool IsActive { get; set; }
        public int? LtGroupCode { get; set; }
        public int? MtGroupCode { get; set; }
        public int? CtGroupCode { get; set; }
        public int? OdGroupCode { get; set; }
    }

    public class PcmfLoanPurposeQuery
    {
        public string SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int? LtGroupCode { get; set; }
        public int? MtGroupCode { get; set; }
        public int? CtGroupCode { get; set; }
        public int? OdGroupCode { get; set; }
    }

    // Add to your existing enums file
    public enum LoanTermKind
    {
        ShortTerm = 1,
        MediumTerm = 2,
        LongTerm = 3
    }

    public enum PcmfPurposeKey
    {
        Immobilier = 0,
        Habitat = 1,
        Equipement = 2,
        MoratoireEtat = 3,
        CampagneMoratoire = 4,
        Consommation = 5,
        CreditBail = 6,
        AutresCredits = 7,
        CreancesRattachees = 8,

        // CT detailed
        Ct_EffetsCommerciauxEscomptes = 9,
        Ct_Affacturage = 10,
        Ct_ChequesLocauxEscomptes = 11,
        Ct_MoratoiresConsolidesEtat = 12,
        Tresorerie = 13,
        Ct_Equipement = 14,
        Ct_AvancesMarchesPublicsNanties = 15,
        Ct_AutresCreditsAccompagnement = 16,
        Ct_Campagne = 17,
        Ct_Consommation = 18,
        Ct_CreditBail = 19,
        Ct_AutresCredits = 20,
        Ct_CreancesRattachees = 21,

        // OD
        Od_ComptesCourantsDebiteurs = 22,
        Od_ComptesChequesDebiteurs = 23,
        Od_ComptesOrdinairesDebiteurs = 24,
        Od_DepotsGarantie = 25,
        Od_AvancesDAT = 26,
        Od_LoyersCreditBail = 27,
        Od_DepotsGarantieCreditBail = 28,
        Od_CreancesDettesRattachees = 29
    }
}
