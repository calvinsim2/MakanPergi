namespace Common.Model
{
    public class TreeViewNodeModel
    {
        public string Id { get; set; }
        public string Parent { get; set; } = "#";
        public string Text { get; set; }
        public TreeViewState State { get; set; } = new TreeViewState();
    }
}
