namespace CBS.FrontDesk.Data.Entity.DataTable
{
    public interface ICustomDataTable
    {
        int draw { get; set; }
        int recordsTotal { get; set; }
        int recordsFiltered { get; set; }
        object data { get; set; }
        DataTableOptions DataTableOptions { get; set; }
    }
}