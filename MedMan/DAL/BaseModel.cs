using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace sThuoc.DAL
{
    public class BaseModel
    {
        public DateTime? Created { get; set; }

        public virtual Models.UserProfile CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public virtual Models.UserProfile ModifiedBy { get; set; }
        public bool? Active { get; set; }
    }
}