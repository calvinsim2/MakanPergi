namespace Common.Model.Excel
{
    public class UploadExcelResult
    {
        public string EntityName { get; set; }
        public string Message { get; set; }
        public bool IsValid { get; set; } = false;
        public int SuccessCount { get; set; }
        public string? WarningMessage { get; set; }
        public bool IsWarning { get; set; }
    }
}
