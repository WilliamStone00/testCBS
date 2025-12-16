namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.GlobalConfig
{
	public class GlobalConfig
	{
		public string Id { get; set; }
		public bool IsCentralised { get; set; }
		public int MinimumOfChequeLeave { get; set; }
		public int MaximumOfChequeLeave { get; set; }
		public int ChequeValidityDays { get; set; }
	}
}
