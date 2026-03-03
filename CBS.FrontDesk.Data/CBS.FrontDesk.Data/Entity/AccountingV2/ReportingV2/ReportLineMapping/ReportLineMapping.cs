using System;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLineMapping
{
	//public class ReportLineMapping
	//{
	//	public string Id { get; set; }
	//	public string LineId { get; set; }
	//	public string MatchType { get; set; }
	//	public string FromValue { get; set; }
	//	public string ToValue { get; set; }
	//	public string Side { get; set; }
	//	public short Sign { get; set; }
	//	public bool IncludeLiaison { get; set; }
	//	public DateTime? CreatedDate { get; set; }
	//}

    public sealed class ReportLineMapping 
    {
        public string Id { get; set; }

        public string LineId { get; set; } 
        public string MainSection { get; set; } 
        public string SubSection { get; set; } 


        // NEW: how this mapping matches BranchAccount
        public ReportMatchType MatchType { get; set; }

        // Snapshots (Excel-like)
        public string AccountCode { get; set; }    // required when MatchType = AccountCode
        public string AccountName { get; set; }    // always stored for display/reporting
        public string CodePoste { get; set; }      // required when MatchType = CodePoste

        // Always required
        public EntrySide Side { get; set; }

        // +1 or -1
        public short Sign { get; set; } = 1;

        public bool IncludeLiaison { get; set; }

        public string Line { get; set; } 
    }

    public enum ReportMatchType : short
    {
        AccountCode = 1,   // map by account number (e.g. 112100000000)
        CodePoste = 2      // map by poste (e.g. CP_1121 / 1121 / "POSTE-01" depending on your format)
    }

    public enum EntrySide : short
    {
        Dr = 1,
        Cr = 2
    }
}

