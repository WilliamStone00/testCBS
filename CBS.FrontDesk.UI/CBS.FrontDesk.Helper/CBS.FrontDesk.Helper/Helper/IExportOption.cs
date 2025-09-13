namespace CBS.FrontDesk.Helper.Helper
{
    public interface IExportOption
    {
        object data { get; set; }
        byte[] bytes { get; set; }
        string path { get; set; }
        string filename { get; set; }
    }
}