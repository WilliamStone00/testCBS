namespace CBS.BusinessService.CheckManagementSystem.LossManagementSystem
{
    internal class DataTableResult<T>
    {
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public object Data { get; set; }
    }
}