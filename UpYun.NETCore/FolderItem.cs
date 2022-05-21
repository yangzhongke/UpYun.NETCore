namespace UpYun.NETCore
{
    //目录条目类
    public class FolderItem
    {
        public string filename;
        public string filetype;
        public int size;
        public int number;
        public FolderItem(string filename, string filetype, int size, int number)
        {
            this.filename = filename;
            this.filetype = filetype;
            this.size = size;
            this.number = number;
        }

        public override string ToString()
        {
            return $"filename={filename},filetype={filetype},size={size},number={number}";
        }
    }
}
