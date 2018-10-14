using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sThuoc.Models.ViewModels
{
    [Serializable]
    public class UploadObjectInfo
    {
        public string Title;
        public int TotalAdded;
        public int TotalUpdated;
        public int TotalError;
        public List<string> ErrorMsg = new List<string>();
    }
}