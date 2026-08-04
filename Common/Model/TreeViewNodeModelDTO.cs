namespace Common.Model
{
    public class TreeViewNodeModelDTO
    {
        public string Id { get; set; }
        public string Parent { get; set; } = "#";
        public string Text { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            TreeViewNodeModelDTO other = (TreeViewNodeModelDTO)obj;
            return string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}
