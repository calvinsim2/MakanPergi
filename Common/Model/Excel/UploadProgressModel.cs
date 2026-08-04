namespace Common.Model.Excel
{
    public class UploadProgressModel
    {
        public int Current { get; set; }
        public int Total { get; set; }
        public bool IsComplete { get; set; } = false;
        public string? Message { get; set; }
        public string Result { get; set; }
    }
}
