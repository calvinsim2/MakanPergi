namespace MakanPergi.Models
{
    public class ResultWrapper<T>
    {
        public string? Message { get; set; }
        public T? Result {  get; set; }
    }
}
