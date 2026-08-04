namespace MakanPergi.Common.AppConfig
{
    public class ApiSetting
    {
        public List<UrlItem> UrlItems { get; set; }
    }

    public class UrlItem
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
