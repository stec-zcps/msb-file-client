using System;

namespace msb_file_client
{
    public class FileObject : System.IComparable<FileObject>
    {
        public string id;
        public string filename;
        public uint index;
        public uint count;
        public string content;

        public int CompareTo(FileObject other)
        {
            return this.index.CompareTo(other.index);
        }
    }
}