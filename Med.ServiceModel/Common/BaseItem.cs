using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Common
{
    public class BaseItem
    {
        /// <summary>
        /// Item order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Date item
        /// </summary>
        public DateTime? ItemDate { get; set; }

        /// <summary>
        /// Item number
        /// </summary>
        public string ItemNumber { get; set; }

        /// <summary>
        /// Item Id
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Item name
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// Item Code
        /// </summary>
        public string ItemCode { get; set; }
    }
}
