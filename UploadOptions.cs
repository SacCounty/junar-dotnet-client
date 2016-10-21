using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JunarUpload
{
    public class UploadOptions
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public List<string> Tags { get; set; }
        public string Notes { get; set; }
        public string License { get; set; }
        public string FilePath { get; set; }
        public string GUID { get; set; }
        public string ContentType { get; set; }
        public string TableId { get; set; }
        public string Get { get; set; }
    }
}
