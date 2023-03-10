using Microsoft.SharePoint.Client;

namespace PnP.PowerShell.Commands.Base.PipeBinds
{
    public sealed class ContentTypePipeBind
    {
        private readonly string _id;
        private readonly string _name;
        private readonly ContentType _contentType;

        public ContentTypePipeBind()
        {
            _id = string.Empty;
            _name = string.Empty;
            _contentType = null;
        }

        public ContentTypePipeBind(string id)
        {
            if (id.ToLower().StartsWith("0x0"))
            {
                _id = id;
            }
            else
            {
                _name = id;
            }

        }

        public ContentTypePipeBind(ContentType contentType)
        {
            _contentType = contentType;
        }

        public string Id
        {
            get
            {
                if (_contentType != null)
                {
                    return _contentType.StringId;
                }
                else
                {
                    return _id;
                }
            }
        }

        public string Name => _name;

        public ContentType ContentType => _contentType;

        public ContentType GetContentType(Web web)
        {
            if (ContentType != null)
            {
                return ContentType;
            }
            ContentType ct;
            if (!string.IsNullOrEmpty(Id))
            {
                ct = web.GetContentTypeById(Id,true);

            }
            else
            {
                ct = web.GetContentTypeByName(Name,true);
            }

            return ct;
        }
    }
}
