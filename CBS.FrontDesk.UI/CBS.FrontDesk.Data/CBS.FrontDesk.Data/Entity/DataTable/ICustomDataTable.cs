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
    public interface ICustomDataTable<T> where T : class
    {
        int draw { get; set; }
        int recordsTotal { get; set; }
        int recordsFiltered { get; set; }
        T data { get; set; }
        DataTableOptions DataTableOptions { get; set; }
    }
}