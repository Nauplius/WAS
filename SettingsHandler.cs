using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nauplius.WAS
{
    internal class SettingsHandler
    {

    }

    class SettingsOptions
    {
        public string BookmarkOps { get; set; }
        public string BallonOps { get; set; }
        public string[] PdfOps { get; set; }
        public bool DeleteSource { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string StorageControl { get; set; }
    }
}