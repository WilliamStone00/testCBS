namespace CBS.FrontDesk.Data
{
    public class StatementModel
    {
        public string reference { get; set; }
        public string heading { get; set; }

        public string document_type { get; set; }
        public StatementModel()
        {
        }
    }

    //public class BalanceSheetAssetModel
    //{
    //    public string reference { get; set; }
    //    public string heading { get; set; }
    //    public decimal gross { get; set; }
    //    public decimal depreciation { get; set; }
    //    public decimal amount_n { get; set; }
    //    public decimal amount_nM1 { get; set; }
    //    public BalanceSheetAssetModel()
    //    {
    //    }
    //}
}