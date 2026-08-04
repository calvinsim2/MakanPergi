namespace MakanPergi.Common.Helpers
{
    public class ExcelFileStream : MemoryStream
    {
        public ExcelFileStream()
        {
            AllowClose = false;
        }

        public bool AllowClose { get; set; }

        public override void Close()
        {
            if (AllowClose) base.Close();
        }
    }
}
