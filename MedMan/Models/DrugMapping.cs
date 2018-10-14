using sThuoc.DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace sThuoc.Models
{
    [Table("DrugMapping")]
    public class DrugMapping 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string DrugStoreID { get; set; }
        public int MasterDrugID { get; set; }
        public int SlaveDrugID { get; set; }
        public int? UnitID { get; set; }
        public decimal InPrice { get; set; }
        public DateTime? InLastUpdateDate { get; set; }
        public decimal OutPrice { get; set; }
        public DateTime? OutLastUpdateDate { get; set; }
    }
}