using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sThuoc.Models.ViewModels
{
    public class Permission
    {
        public string Title { get; set; }
        public string Controller { get; set; }
        public List<PermissionItem> Permissions { get; set; }

        public Permission()
        {
        }

        public Permission(string title, string controler)
        {

            this.Controller = controler;
            this.Title = title;
            this.Permissions = new List<PermissionItem>();
        }
    }

    public class PermissionItem
    {
        //public string Title
        //{
        //    get; set;
        //}
        public string Permission { get; set; }
        public string action { get; set; }
        public string controller { get; set; }
        public bool Checked { get; set; }
        public string CheckedValue;       
        public bool Visible { get; set; }
    }

    public enum QuyenEnum
    {
        View = 1,
        Add = 2,
        Modify = 3,
        Delete = 4
    }
}