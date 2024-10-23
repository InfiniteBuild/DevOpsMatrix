using DevOpsMatrix.Interface;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace DevOpsMatrix.Tfs.Server
{
    public class TfvcSourceItem : ISourceCodeItem
    {
        private TfvcItem m_item;

        public string ItemPath { get { return m_item.Path; } }
        public SourceCodeItemType Itemtype { get { return m_item.IsFolder ? SourceCodeItemType.Folder : SourceCodeItemType.File; } }
        public int Version { get { return m_item.ChangesetVersion; } }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public int Encoding { get; set; }
        public bool IsBinary { get; set; }
        public bool ChangeEncoding { get; set; } = false;

        public TfvcSourceItem(TfvcItem item) 
        { 
            m_item = item;
            Content = null;
            ContentType = item.ContentMetadata.ContentType;
            IsBinary = item.ContentMetadata.IsBinary;
            Encoding = item.ContentMetadata.Encoding;
        }

        public TfvcSourceItem(string itemPath)
        {
            m_item = new TfvcItem();

            m_item.Path = itemPath;
        }

        public TfvcSourceItem(string path, string name, byte[] content, string contentType, bool isBinary, int encoding)
        {
            m_item = new TfvcItem();

            m_item.Path = path.Trim('/') + "/" + name.Trim('/');
            Content = content;
            ContentType = contentType;
            IsBinary = isBinary;
            Encoding = encoding;
        }
    }
}
