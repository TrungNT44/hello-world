using System;


namespace Med.ServiceModel.Drug
{
    public partial class DrugPriceModel
    {
        public int DrugID { get; set; }
        public decimal? InPrice { get; set; }
        public DateTime? InLastUpdateDate { get; set; }
        public decimal? OutPrice { get; set; }
        public DateTime? OutLastUpdateDate { get; set; }
    }
}
