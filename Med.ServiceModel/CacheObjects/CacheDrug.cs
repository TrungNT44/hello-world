using Med.Common;
using Med.ServiceModel.Delivery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.CacheObjects
{
    public class CacheDrug
    {
        public int DrugId { get; set; }
        public string DrugCode { get; set; }
        public string DrugName { get; set; }
        public string ExtraInfo { get; set; }
        public int? UnitId { get; set; }
        public string DrugBarcode { get; set; }
        public int? RetailUnitId { get; set; }
        public List<DrugUnit> Units { get; set; }
        public double Factors { get; set; }
        public double RetailInPrice { get; set; }
        public double RetailOutPrice { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? ExpiredDateTime { get; set; }
        public string FullInfo
        {
            get
            {
                var fullInfo = string.Format("{0} - {1}", DrugCode, DrugName);
                if (!string.IsNullOrWhiteSpace(ExtraInfo))
                {
                    fullInfo = string.Format("{0} - {1}", fullInfo, ExtraInfo);
                }

                return fullInfo;
            }
        }
        public double LastInventoryQuantity { get; set; }
        public double LastInPrice { get; set; }
        public double LastOutPrice { get; set; }
        public double RetailBatchOutPrice { get; set; }
        public string GetFullSearchString()
        {
            return string.Format("{0} - {1}", FullInfo, DrugBarcode).ToLower();
        }
        #region Pre Values
        public decimal PreFactors { get; set; }
        public int? PreRetailUnitID { get; set; }
        public int? PreUnitID { get; set; }
        public decimal PreInitQuantity { get; set; }
        public decimal PreInitPrice { get; set; }
        public decimal InitQuantity { get; set; }
        public decimal InitPrice { get; set; }
        public DateTime? PreExpiredDate { get; set; }
        public bool ShouldUpdateInitialInventoryReceiptItems()
        {
            return (double)Math.Abs(PreInitPrice - InitPrice) > MedConstants.Esp
                || (double)Math.Abs(PreInitQuantity - InitQuantity) > MedConstants.Esp
                || (PreExpiredDate.GetValueOrDefault(DateTime.MaxValue).Date != ExpiredDateTime.GetValueOrDefault(DateTime.MaxValue).Date)
                || InitReceiptNoteItemId <= 0;
        }
        public bool HasFactorsChanged()
        {
            return Math.Abs((double)PreFactors - Factors) > MedConstants.Esp
                || (PreRetailUnitID != RetailUnitId)
                || (PreUnitID != UnitId);
        }
        public bool HasInitQuantityChanged()
        {
            return (double)Math.Abs(PreInitQuantity - InitQuantity) > MedConstants.Esp;
        }
        public int? InitReceiptNoteItemId { get; set; }
        #endregion
    }
}
