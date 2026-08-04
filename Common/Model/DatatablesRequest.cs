namespace Common.Model
{
    public class DatatablesRequest
    {
        public int Draw { get; set; }
        public List<DatatablesColumn> Columns { get; set; }
        public List<DatatablesOrder> Order { get; set; }
        public int Start { get; set; } // to indicate the start of the index we want to take
        public int Length { get; set; } // to take the number of records
        public DatatablesSearch Search { get; set; }
    }
}
