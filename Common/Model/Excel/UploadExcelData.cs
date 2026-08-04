namespace Common.Model.Excel
{
    public class UploadExcelData
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; }
        public List<UploadExcelResult>? ErrorList { get; set; }
        public int TotalCount { get; set; }
    }
}
