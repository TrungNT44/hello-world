using System;
using System.Dynamic;
using System.Linq;
using App.Common;
using App.Common.Extensions;
using App.Common.Helpers;
using App.Common.Data;
using App.Common.Validation;
using Med.Common.Enums;
using Med.DbContext;
using Med.Entity.Registration;
using App.Common.DI;
using System.Collections.Generic;
using Med.Repository.Factory;
using Med.Service.Base;
using Med.Service.Delivery;
using Med.Service.Receipt;
using Med.Service.Report;
using Med.ServiceModel.Common;
using Med.ServiceModel.Registration;
using Med.Service.Registration;
using Med.Entity;
using Med.Repository.Registration;
using Med.ServiceModel.Report;
using Med.ServiceModel.Response;
using Med.Common;
using App.Common.FaultHandling;
using Castle.Core.Internal;
using Med.ServiceModel.Drug;
using Med.Service.Drug;

namespace Med.Service.Impl.Report
{
    public class ReportService : MedBaseService, IReportService
    {
        #region Fields
        #endregion

        #region Interface Implementation
        public InOutCommingValueSummary GetInOutCommingValueSumary(string drugStoreCode, FilterObject filter = null)
        {
            var result = new InOutCommingValueSummary();
                     
            var deliveryNotes = _dataFilterService.GetValidDeliveryNotes(drugStoreCode, filter, new int[] { (int)NoteInOutType.Delivery });
            var returnedToSupplyerNotes = _dataFilterService.GetValidDeliveryNotes(drugStoreCode, filter, new int[] { (int)NoteInOutType.ReturnToSupplier });
            var receiptNotes = _dataFilterService.GetValidReceiptNotes(drugStoreCode, filter, new int[] { (int)NoteInOutType.Receipt });
            var returnedFromCustomerNotes = _dataFilterService.GetValidReceiptNotes(drugStoreCode, filter, new int[] { (int)NoteInOutType.ReturnFromCustomer });
            var inOutNoteStatues = new int[]
            {
                (int) InOutCommingType.Incomming, 
                (int) InOutCommingType.OtherIncomming, 
                (int) InOutCommingType.Outcomming,
                (int) InOutCommingType.OtherOutcomming,
                (int) InOutCommingType.BusinessCosts
            };
            var inOutCommingNotes = _dataFilterService.GetValidInOutCommingNotes(drugStoreCode, filter, inOutNoteStatues).Select(i => new
            {
                NoteType = i.LoaiPhieu,
                Amount = (double)i.Amount
            }).ToList();

            double incommingTotal = 0.0, otherIncommingTotal = 0.0, outcommingTotal = 0.0, otherOutcommingTotal = 0.0, bussCostsTotal = 0.0;
            if (inOutCommingNotes.Any())
            {
                incommingTotal = inOutCommingNotes.Where(i => i.NoteType == (int) InOutCommingType.Incomming).Sum(i => i.Amount);
                otherIncommingTotal = inOutCommingNotes.Where(i => i.NoteType == (int)InOutCommingType.OtherIncomming).Sum(i => i.Amount);
                outcommingTotal = inOutCommingNotes.Where(i => i.NoteType == (int)InOutCommingType.Outcomming).Sum(i => i.Amount);
                otherOutcommingTotal = inOutCommingNotes.Where(i => i.NoteType == (int)InOutCommingType.OtherOutcomming).Sum(i => i.Amount);
                bussCostsTotal = inOutCommingNotes.Where(i => i.NoteType == (int) InOutCommingType.BusinessCosts).Sum(i => i.Amount);
            }
            var customerDueFromDeliveryNotes = 0.0;
            var supplyerDueFromReceiptNotes = 0.0;
            if (deliveryNotes.Any())
            {
                // In comming value - Tổng thu
                result.DeliveryIncomingTotal = (double)deliveryNotes.Sum(i => i.DaTra) + incommingTotal;

                // Out due values - Tổng nợ xuất
                customerDueFromDeliveryNotes = (double)deliveryNotes.Sum(i => (i.TongTien - i.DaTra));
            }
            if (receiptNotes.Any())
            {
                // Out comming value - Tổng chi
                result.ReceiptOutcomingTotal = (double)receiptNotes.Sum(i => i.DaTra) + outcommingTotal;

                // In due values - Tổng nợ nhập
                supplyerDueFromReceiptNotes = (double)receiptNotes.Sum(i => (i.TongTien - i.DaTra)); 
            }
            
            result.OtherIncommingTotal = otherIncommingTotal;
            result.OtherOutcomingTotal = otherOutcommingTotal;
            result.BusinessCostsTotal = bussCostsTotal;

            var returnedFromCustomerValue = 0; // (double)returnedFromCustomerNotes.Select(i => i.TongTien).ToList().Sum();
            result.DeliveryDueTotal = customerDueFromDeliveryNotes - (incommingTotal + returnedFromCustomerValue);            
            var returnedToSupplyerValue = 0; // (double)returnedToSupplyerNotes.Select(i => i.TongTien).ToList().Sum();
            result.ReceiptDueTotal = supplyerDueFromReceiptNotes - (outcommingTotal + returnedToSupplyerValue);

            return result;
        }
        public Dictionary<int, DrugWarehouseSynthesis> GetDrugWarehouseSyntheises(string drugStoreCode, FilterObject filter = null)
        {
            LogHelper.StartExeMethodMeasurement();
            Dictionary<int, DrugWarehouseSynthesis> drugWarehouseSyntheises = null;
            DateTime? fromDate = null;
            DateTime? toDate = null;
            int[] drugIds = null;
            if (filter != null)
            {
                fromDate = filter.FromDate;
                toDate = filter.ToDate;
                drugIds = filter.DrugIds;
            }
            var isFromDateValid = fromDate.HasValue && fromDate.Value > MedConstants.MinProductionDataDate;           
            var validDrugs = _dataFilterService.GetValidDrugs(drugStoreCode, filter, false);
            var isChild = false;
            var dsSession = GetDrugStoreSession();
            if (dsSession != null && dsSession.IsChildDrugStore)
            {
                isChild = true;
            }
            var drugItems = validDrugs.Select(i => new
            {
                DrugId = i.ThuocId,
                RetailQuantity = isChild ? 0 : (double)i.SoDuDauKy,
                RetailPrice = (double)i.GiaDauKy,
                IsActivated = i.HoatDong,
                LimitQuantityWarning = i.GioiHan ?? 0
            }).ToDictionary(i => i.DrugId, i => i);
            drugWarehouseSyntheises = drugItems.Values.Select(i => new DrugWarehouseSynthesis()
            {
                DrugId = i.DrugId,
                FirstInventoryValue = i.RetailQuantity * i.RetailPrice,
                InitReceiptQuantity = i.RetailQuantity,
                InitReceiptValue = i.RetailQuantity * i.RetailPrice,
                InitRetailPrice = i.RetailPrice,
                LastReceiptRetailPrice = i.RetailPrice,
                IsActivated = i.IsActivated,
                LimitQuantityWarning = i.LimitQuantityWarning
            }).ToDictionary(i => i.DrugId, i => i);


            FilterObject newFilter = null;
            if (filter != null)
            {
                newFilter = filter.DeepCopy();
                newFilter.FromDate = null;
            }
          
            var hasFromDate = filter != null && filter.FromDate.HasValue && filter.FromDate.Value > MedConstants.MinProductionDataDate;
            List<NoteItemQuantity> receiptItems = null;
            List<NoteItemQuantity> deliveryItems = null;
            var invService = IoC.Container.Resolve<IInventoryService>();           
            invService.GetNoteItemQuantities(drugStoreCode, out receiptItems, out deliveryItems, newFilter);            

            var validReceiptStatus = new int[] { (int)NoteInOutType.Receipt, (int)NoteInOutType.InventoryAdjustment };
            var validDeliveryStatus = new int[] { (int)NoteInOutType.Delivery, (int)NoteInOutType.InventoryAdjustment };
            var validReceiptItems = receiptItems.Where(i => validReceiptStatus.Contains(i.NoteType)).ToList();
            var validDeliveryItems = deliveryItems.Where(i => validDeliveryStatus.Contains(i.NoteType)).ToList();
            var returnedFromCustomerItems = receiptItems.Where(i => i.NoteType == (int)NoteInOutType.ReturnFromCustomer).ToList();
            var returnedToSupplyerItems = deliveryItems.Where(i => i.NoteType == (int)NoteInOutType.ReturnToSupplier).ToList();

            var receiptItemsByDrugsDict = validReceiptItems.GroupBy(i => i.DrugId).ToDictionary(i => i.Key, i => i.ToList());
            var deliveryItemsByDrugsDict = validDeliveryItems.GroupBy(i => i.DrugId).ToDictionary(i => i.Key, i => i.ToList());          
            var returnedFromCustomerItemsByDrugsDict = returnedFromCustomerItems.GroupBy(i => i.DrugId)
                .ToDictionary(i => i.Key, i => i.Sum(ii => ii.RetailQuantity));
            var returnedToSupplyerItemsByDrugs = returnedToSupplyerItems.GroupBy(i => i.DrugId)
                .ToDictionary(i => i.Key, i => i.Sum(ii => ii.RetailQuantity));
        
            drugItems.AsParallel().ForAll(d =>
            {
                var deliveryQuantity = 0.0;
                var receiptQuantity = 0.0;
                var returnedFromCustomerQuantity = 0.0;
                var returnedToSupplyerQuantity = 0.0;
                var drugId = d.Key;
                var drugWarehouse = drugWarehouseSyntheises[drugId];

                List<NoteItemQuantity> receiptItemsByDrug = null;
                List<NoteItemQuantity> deliveryItemsByDrug = null;
                if (receiptItemsByDrugsDict.ContainsKey(drugId))
                {
                    receiptItemsByDrug = receiptItemsByDrugsDict[drugId]
                        .OrderByDescending(i => i.NoteDate).ToList();
                    if (receiptItemsByDrug.Any())
                    {
                        drugWarehouse.LastReceiptRetailPrice = receiptItemsByDrug.First().RetailPrice;
                    }
                    receiptQuantity = receiptItemsByDrug.Sum(i => i.RetailQuantity);
                }
                if (deliveryItemsByDrugsDict.ContainsKey(drugId))
                {
                    deliveryItemsByDrug = deliveryItemsByDrugsDict[drugId]
                        .OrderByDescending(i => i.NoteDate).ToList();
                    deliveryQuantity = deliveryItemsByDrug.Sum(i => i.RetailQuantity);
                }
                if (returnedFromCustomerItemsByDrugsDict.ContainsKey(drugId))
                {
                    returnedFromCustomerQuantity = returnedFromCustomerItemsByDrugsDict[drugId];
                }
                if (returnedToSupplyerItemsByDrugs.ContainsKey(drugId))
                {
                    returnedToSupplyerQuantity = returnedToSupplyerItemsByDrugs[drugId];
                }
                
                drugWarehouse.LastDeliveryQuantity = (deliveryQuantity - returnedFromCustomerQuantity);
                drugWarehouse.LastReceiptQuantity = (receiptQuantity - returnedToSupplyerQuantity);
                var inventoryQuantity = drugWarehouse.LastInventoryQuantity - drugWarehouse.InitReceiptQuantity;
                var inventoryValue = 0.0;
                if (inventoryQuantity > MedConstants.EspQuantity)
                {
                    if (receiptItemsByDrug != null)
                    {
                        var quantity = inventoryQuantity;
                        while (quantity > MedConstants.EspQuantity && receiptItemsByDrug.Count > 0)
                        {
                            var usedQuantity = Math.Min(quantity, receiptItemsByDrug[0].RetailQuantity);
                            quantity -= usedQuantity;
                            inventoryValue += usedQuantity * receiptItemsByDrug[0].RetailPrice;
                            receiptItemsByDrug.RemoveAt(0);
                        }
                        if (quantity > MedConstants.EspQuantity)
                        {
                            inventoryValue += quantity * drugWarehouse.InitRetailPrice;
                        }
                        inventoryValue += drugWarehouse.InitReceiptValue;
                    }                   
                }
                else
                {
                    inventoryValue = drugWarehouse.LastInventoryQuantity * drugWarehouse.InitRetailPrice;
                }
                drugWarehouse.LastInventoryValue = inventoryValue;
            });

            if (hasFromDate)
            {
                validReceiptItems = validReceiptItems.Where(i => i.NoteDate < fromDate).ToList();
                validDeliveryItems = validDeliveryItems.Where(i => i.NoteDate < fromDate).ToList();
                receiptItemsByDrugsDict = validReceiptItems.GroupBy(i => i.DrugId).ToDictionary(i => i.Key, i => i.ToList());
                deliveryItemsByDrugsDict = validDeliveryItems.GroupBy(i => i.DrugId).ToDictionary(i => i.Key, i => i.ToList());

                returnedFromCustomerItems = returnedFromCustomerItems.Where(i => i.NoteDate < fromDate).ToList();
                returnedToSupplyerItems = returnedToSupplyerItems.Where(i => i.NoteDate < fromDate).ToList();
                returnedFromCustomerItemsByDrugsDict = returnedFromCustomerItems.GroupBy(i => i.DrugId)
                    .ToDictionary(i => i.Key, i => i.Sum(ii => ii.RetailQuantity));
                returnedToSupplyerItemsByDrugs = returnedToSupplyerItems.GroupBy(i => i.DrugId)
                    .ToDictionary(i => i.Key, i => i.Sum(ii => ii.RetailQuantity));
                drugItems.AsParallel().ForAll(d =>
                {
                    var deliveryQuantity = 0.0;
                    var receiptQuantity = 0.0;
                    var returnedFromCustomerQuantity = 0.0;
                    var returnedToSupplyerQuantity = 0.0;
                    var drugId = d.Key;
                    var drugWarehouse = drugWarehouseSyntheises[drugId];
                    List<NoteItemQuantity> receiptItemsByDrug = null;
                    List<NoteItemQuantity> deliveryItemsByDrug = null;
                    if (receiptItemsByDrugsDict.ContainsKey(drugId))
                    {
                        receiptItemsByDrug = receiptItemsByDrugsDict[drugId]
                            .OrderByDescending(i => i.NoteDate).ToList();                       
                        receiptQuantity = receiptItemsByDrug.Sum(i => i.RetailQuantity);
                    }
                    if (deliveryItemsByDrugsDict.ContainsKey(drugId))
                    {
                        deliveryItemsByDrug = deliveryItemsByDrugsDict[drugId]
                            .OrderByDescending(i => i.NoteDate).ToList();
                        deliveryQuantity = deliveryItemsByDrug.Sum(i => i.RetailQuantity);
                    }
                    if (returnedFromCustomerItemsByDrugsDict.ContainsKey(drugId))
                    {
                        returnedFromCustomerQuantity = returnedFromCustomerItemsByDrugsDict[drugId];
                    }
                    if (returnedToSupplyerItemsByDrugs.ContainsKey(drugId))
                    {
                        returnedToSupplyerQuantity = returnedToSupplyerItemsByDrugs[drugId];
                    }

                    drugWarehouse.FirstDeliveryQuantity = (deliveryQuantity - returnedFromCustomerQuantity);
                    drugWarehouse.FirstReceiptQuantity = (receiptQuantity - returnedToSupplyerQuantity);
                    var inventoryQuantity = drugWarehouse.FirstInventoryQuantity - drugWarehouse.InitReceiptQuantity;
                    var inventoryValue = 0.0;
                    if (inventoryQuantity > MedConstants.EspQuantity)
                    {
                        if (receiptItemsByDrug != null)
                        {
                            var quantity = inventoryQuantity;
                            while (quantity > MedConstants.EspQuantity && receiptItemsByDrug.Count > 0)
                            {
                                var usedQuantity = Math.Min(quantity, receiptItemsByDrug[0].RetailQuantity);
                                quantity -= usedQuantity;
                                inventoryValue += usedQuantity * receiptItemsByDrug[0].RetailPrice;
                                receiptItemsByDrug.RemoveAt(0);
                            }
                            if (quantity > MedConstants.EspQuantity)
                            {
                                inventoryValue += quantity * drugWarehouse.InitRetailPrice;
                            }
                            inventoryValue += drugWarehouse.InitReceiptValue;
                        }
                      
                    }
                    else 
                    {
                        inventoryValue = drugWarehouse.LastInventoryQuantity * drugWarehouse.InitRetailPrice;
                    }
                    drugWarehouse.FirstInventoryValue = inventoryValue;
                });
            }
            var deliveryInPeriodItemIds = deliveryItems.Where(i => i.NoteType != (int)NoteInOutType.ReturnToSupplier
                   && i.NoteDate >= fromDate && i.NoteDate <= toDate).Select(i => i.NoteItemId).ToArray();
            var deliveryService = IoC.Container.Resolve<IDeliveryNoteService>();
            var drugDeliveryInPeriodValues = deliveryService.GetDrugDeliveryTotalValues(drugStoreCode, deliveryInPeriodItemIds);
            var receiptAfterToDateItemsByDrugsDict = receiptItems.Where(i => validReceiptStatus.Contains(i.NoteType) && i.NoteDate > fromDate)
                .GroupBy(i => i.DrugId).ToDictionary(i => i.Key, i => i.ToList());
            drugItems.ForEach(d =>
            {
                var drugId = d.Key;
                var drugWarehouse = drugWarehouseSyntheises[drugId];             
                List< NoteItemQuantity> receiptItemsByDrug = null;
                if (receiptAfterToDateItemsByDrugsDict.ContainsKey(drugId))
                {
                    receiptItemsByDrug = receiptAfterToDateItemsByDrugsDict[drugId].OrderBy(i => i.NoteDate).ToList();
                }
                
                var quantity = drugWarehouse.ReceiptInventoryQuantityInPeriod;
                var inventoryValue = 0.0;               
                while (quantity > MedConstants.EspQuantity && receiptItemsByDrug != null && receiptItemsByDrug.Count > 0)
                {
                    var usedQuantity = Math.Min(quantity, receiptItemsByDrug[0].RetailQuantity);
                    quantity -= usedQuantity;
                    inventoryValue += usedQuantity * receiptItemsByDrug[0].RetailPrice;
                    receiptItemsByDrug.RemoveAt(0);
                }
                var lastPrice = drugWarehouse.LastReceiptRetailPrice;
                if (lastPrice < MedConstants.EspQuantity)
                {
                    lastPrice = drugWarehouse.InitRetailPrice;
                }
                if (quantity > MedConstants.EspQuantity)
                {                   
                    inventoryValue += quantity * lastPrice;
                }
                drugWarehouse.ReceiptInventoryValueInPeriod = inventoryValue;

                if (drugDeliveryInPeriodValues.ContainsKey(drugId))
                {
                    var deliveryQuantityInPeriod = drugDeliveryInPeriodValues[drugId].QuantityTotal;
                    var deliveryValueInPeriod = drugDeliveryInPeriodValues[drugId].ValueTotal;
                    var diffQuantity = drugWarehouse.DeliveryInventoryQuantityInPeriod - deliveryQuantityInPeriod;
                    if (diffQuantity > MedConstants.EspQuantity)
                    {
                        deliveryValueInPeriod +=  diffQuantity * lastPrice;
                    }
                    
                    drugWarehouse.DeliveryInventoryValueInPeriod = deliveryValueInPeriod;
                }
                if (drugWarehouse.LastInventoryQuantity < MedConstants.EspQuantity)
                {
                    drugWarehouse.LastInventoryValue = 0;
                }
            });
            drugWarehouseSyntheises = drugWarehouseSyntheises.Where(i => i.Value.IsActivated || (!i.Value.IsActivated && i.Value.LastInventoryQuantity > 0))
                .ToDictionary(i => i.Key, i => i.Value);
            LogHelper.StopExeMethodMeasurement();

            return drugWarehouseSyntheises;
        }
        public GroupFilterData GetFilterItems(string drugStoreCode, ItemFilterType filterType, int? currentItemId = null, bool optionAllItems = false)
        {
            var result = new GroupFilterData();
            List<GroupFilterItem> groupItems = null;
            List<GroupFilterItem> nameItems = null;
            FilterObject filter = null; 
           
            switch (filterType)
            {
                case ItemFilterType.DrugGoup:
                    groupItems = _dataFilterService.GetValidDrugGroups(drugStoreCode).Select(i => new 
                    {
                        ItemId = i.MaNhomThuoc,
                        ItemName = i.TenNhomThuoc
                    }).ToList().Select(i => new GroupFilterItem()
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName
                    }).ToList();
                    groupItems.Insert(0, new GroupFilterItem()
                    {
                        ItemId = MedConstants.ConsultDrugGroup,
                        ItemName = "HÀNG TƯ VẤN"
                    });

                    break;

                case ItemFilterType.CustomerGroup:
                    groupItems = _dataFilterService.GetValidCustomerGroups(drugStoreCode).Select(i => new
                    {
                        ItemId = i.MaNhomKhachHang,
                        ItemName = i.TenNhomKhachHang
                    }).ToList().Select(i => new GroupFilterItem()
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName
                    }).ToList();
                    nameItems = _dataFilterService.GetValidCustomers(drugStoreCode).Select(i => new
                    {
                        ItemId = i.MaKhachHang,
                        ItemName = i.TenKhachHang
                    }).ToList().Select(i => new GroupFilterItem()
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName
                    }).ToList();
                   
                    break;

                case ItemFilterType.SupplyerGroup:
                    groupItems = _dataFilterService.GetValidSupplyerGroups(drugStoreCode).Select(i => new
                    {
                        ItemId = i.MaNhomNhaCungCap,
                        ItemName = i.TenNhomNhaCungCap
                    }).ToList().Select(i => new GroupFilterItem()
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName
                    }).ToList();
                    nameItems = _dataFilterService.GetValidSupplyers(drugStoreCode).Select(i => new
                    {
                        ItemId = i.MaNhaCungCap,
                        ItemName = i.TenNhaCungCap
                    }).ToList().Select(i => new GroupFilterItem()
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName
                    }).ToList();
                    break;

                case ItemFilterType.Staff:
                    if (currentItemId.HasValue)
                    {
                        filter = new FilterObject()
                        {
                            StaffIds = new int[] { currentItemId.Value }
                        };
                    }
                    nameItems = _dataFilterService.GetStaffsHasDeliveryNotes(drugStoreCode, filter).Select(i => new
                    {
                        ItemId = i.StaffId,
                        ItemName = i.StaffName
                    }).ToList().Select(i => new GroupFilterItem()
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName
                    }).ToList();
                    
                    break;

                case ItemFilterType.Doctor:
                    nameItems = _dataFilterService.GetValidDoctors(drugStoreCode).Select(i => new
                    {
                        ItemId = i.MaBacSy,
                        ItemName = i.TenBacSy
                    }).ToList().Select(i => new GroupFilterItem()
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName
                    }).ToList();
                    break;

                case ItemFilterType.Customer:
                    nameItems = _dataFilterService.GetValidCustomers(drugStoreCode)
                        .Where(i => i.CustomerTypeId != (int)CustomerType.InventoryAdjustment).Select(i => new
                    {
                        ItemId = i.MaKhachHang,
                        ItemName = i.TenKhachHang,
                        ItemTypeId = i.CustomerTypeId ?? 0
                    }).ToList().Select(i => new GroupFilterItem()
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName,
                        ItemTypeId = i.ItemTypeId
                    }).ToList();
                    groupItems = _dataFilterService.GetValidCustomerGroups(drugStoreCode).Select(i => new
                    {
                        ItemId = i.MaNhomKhachHang,
                        ItemName = i.TenNhomKhachHang
                    }).ToList().Select(i => new GroupFilterItem()
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName
                    }).ToList();
                    break;
                case ItemFilterType.Supplyer:
                    nameItems = _dataFilterService.GetValidSupplyers(drugStoreCode)
                        .Where(i => i.SupplierTypeId != (int)SupplierType.InventoryAdjustment).Select(i => new
                    {
                        ItemId = i.MaNhaCungCap,
                        ItemName = i.TenNhaCungCap,
                        ItemTypeId = i.SupplierTypeId ?? 0
                    }).ToList().Select(i => new GroupFilterItem()
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName,
                        ItemTypeId = i.ItemTypeId
                    }).ToList();
                    groupItems = _dataFilterService.GetValidSupplyerGroups(drugStoreCode).Select(i => new
                    {
                        ItemId = i.MaNhomNhaCungCap,
                        ItemName = i.TenNhomNhaCungCap
                    }).ToList().Select(i => new GroupFilterItem()
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName
                    }).ToList();
                    break;
            }
            if (groupItems == null)
            {
                groupItems = new List<GroupFilterItem>();
            }
            if (!currentItemId.HasValue)
            {
                groupItems.Insert(0, new GroupFilterItem()
                {
                    ItemId = 0,
                    ItemName = MedConstants.FilterByAllText
                });
            }
            if (nameItems == null)
            {
                nameItems = new List<GroupFilterItem>();
            }
            if (optionAllItems)
            {
                nameItems.Insert(0, new GroupFilterItem()
                {
                    ItemId = 0,
                    ItemName = MedConstants.FilterByAllText
                });
            }

            result.GroupItems = groupItems;
            result.NameItems = nameItems;

            return result;
        }
        public class ReportBaseNoteItem: IEquatable<ReportBaseNoteItem>
        {
            public int OwnerId { get; set; }
            public int NoteId { get; set; }

            public bool Equals(ReportBaseNoteItem other)
            {
                if (OwnerId == other.OwnerId && NoteId == other.NoteId)
                    return true;

                return false;
            }

            public override int GetHashCode()
            {     
                return OwnerId.GetHashCode() ^ NoteId.GetHashCode(); 
            }
            public bool ReturnedItem { get; set; }
        }
        protected class ReportDeliveryItem: ReportBaseNoteItem
        {
            public double TotalAmount { get; set; }
            public double PaidAmount { get; set; }
            public bool IsDebt { get; set; }
        }
        public class ReportBaseNoteItemEqualityComparer : IEqualityComparer<ReportBaseNoteItem>
        {
            public bool Equals(ReportBaseNoteItem x, ReportBaseNoteItem y)
            {
                return x.OwnerId.Equals(y.OwnerId) &&
                       x.NoteId.Equals(y.NoteId);
            }

            public int GetHashCode(ReportBaseNoteItem obj)
            {
                return obj.GetHashCode();
            }
        }
        public ReportByResponse GetReportByData(string drugStoreCode, FilterObject filter)
        {
            if (filter.ReportByTypeId == ReportByType.ByGoodsByStaff || filter.ReportByTypeId == ReportByType.ByGoodsByDoctor)
                return GetReportByGoods(drugStoreCode, filter);

            GenerateReportData(drugStoreCode);
            var result = new ReportByResponse();
            var reportByItems = new List<ReportByBaseItem>();
            var deliveryNotes = new List<ReportDeliveryItem>();
            var deliveryItemsGroups = new List<IGrouping<ReportBaseNoteItem, BaseNoteItemInfo>>();
            var deliveryItemsGroupsByOwners = new List<IGrouping<int?, BaseNoteItemInfo>>();
            var deliveryNotesDict = new Dictionary<int, ReportDeliveryItem>();
            var receiptNotesFromCustomerDict = new Dictionary<int, ReportDeliveryItem>();
            var incommingNoteAmountDict = new Dictionary<int?, double>();
            var returnedNotesFromCustomers = new List<ReportDeliveryItem>();
            var noteItemsReturnFromCustomers = new List<ReceiptNoteItemInfo>();
            var totalCount = 0;
            var deliveryService = IoC.Container.Resolve<IDeliveryNoteService>();
            var deliveryStatuses = new int[] { (int)NoteInOutType.Delivery };
            using (var trans = TransactionScopeHelper.CreateReadUncommitted())
            {
                var deliveryNotesQable = _dataFilterService.GetValidDeliveryNotes(drugStoreCode, filter, deliveryStatuses);
                if (!deliveryNotesQable.Any())
                {
                    result.PagingResultModel = new PagingResultModel<ReportByBaseItem>(reportByItems, totalCount);
                    return result;
                }
                var receiptNoteStatuses = new int[] { (int)NoteInOutType.ReturnFromCustomer };
                var receiptNotesFromCustomerQable = _dataFilterService.GetValidReceiptNotes(drugStoreCode, filter, receiptNoteStatuses)
                    .Where(i => i.KhachHang_MaKhachHang > 0);

                if (filter.HasStaffIds || filter.HasCustomerIds || filter.HasDoctorIds || filter.HasSupplyerIds)
                {
                    totalCount = deliveryNotesQable.Count();
                    var deliveryNoteCands = deliveryNotesQable.OrderByDescending(i => i.NgayXuat)
                        .ToPagedQueryable(filter.PageIndex, filter.PageSize, totalCount).Select(i => new ReportDeliveryItem()
                        {
                            PaidAmount = (double)i.DaTra,
                            TotalAmount = (double)i.TongTien,
                            NoteId = i.MaPhieuXuat
                        }).ToList();
                    var deliveryNoteCandIds = deliveryNoteCands.Select(i => i.NoteId).ToArray();
                    filter.DeliveryNoteIds = deliveryNoteCandIds;

                    deliveryNotesDict = deliveryNoteCands.ToDictionary(i => i.NoteId, i => i);

                    // Returned items from customers
                    var receiptNoteService = IoC.Container.Resolve<IReceiptNoteService>();
                    noteItemsReturnFromCustomers = receiptNoteService.GetReceiptNoteItems(drugStoreCode, filter,
                        new int[] { (int)NoteInOutType.ReturnFromCustomer });
                    var receiptNoteIds = noteItemsReturnFromCustomers.Select(i => i.NoteId).Distinct().ToList();
                    receiptNotesFromCustomerDict = _dataFilterService.GetValidReceiptNotes(drugStoreCode)
                        .Where(i => receiptNoteIds.Contains(i.MaPhieuNhap))
                        .Select(i => new ReportDeliveryItem()
                        {
                            PaidAmount = (double)i.DaTra,
                            TotalAmount = (double)i.TongTien,
                            NoteId = i.MaPhieuNhap
                        }).ToDictionary(i => i.NoteId, i => i);
                }

                var deliveryItemCands = deliveryService.GetDeliveryNoteItems(drugStoreCode, filter, deliveryStatuses);
                var deliveryItems = deliveryItemCands.Cast<BaseNoteItemInfo>().ToList();
                var noteIds = deliveryItems.Select(i => i.NoteId).Distinct().ToArray();
                switch (filter.ReportByTypeId)
                {
                    case ReportByType.ByStaff:
                        if (filter.HasStaffIds)
                        {
                            deliveryItems.AddRange(noteItemsReturnFromCustomers.ToArray());
                            deliveryItemsGroups = deliveryItems.GroupBy(i => new ReportBaseNoteItem()
                            { OwnerId = i.StaffId.Value, NoteId = i.NoteId }).DistinctBy(i => i.Key).ToList();
                        }
                        else
                        {
                            returnedNotesFromCustomers = receiptNotesFromCustomerQable.Select(i => new ReportDeliveryItem()
                            {
                                PaidAmount = (double)i.DaTra,
                                TotalAmount = (double)i.TongTien,
                                OwnerId = i.CreatedBy_UserId.Value,
                                ReturnedItem = true
                            }).ToList();
                            deliveryItemsGroupsByOwners = deliveryItems.GroupBy(i => i.StaffId).ToList();
                            deliveryNotes = deliveryNotesQable.Where(i => noteIds.Contains(i.MaPhieuXuat))
                                .Select(i => new ReportDeliveryItem()
                                {
                                    PaidAmount = (double)i.DaTra,
                                    TotalAmount = (double)i.TongTien,
                                    OwnerId = i.CreatedBy_UserId.Value
                                }).ToList();
                        }
                        break;
                    case ReportByType.ByCustomer:
                        if (filter.HasCustomerIds)
                        {
                            deliveryItems.AddRange(noteItemsReturnFromCustomers.ToArray());
                            deliveryItemsGroups = deliveryItems.GroupBy(i => new ReportBaseNoteItem()
                            { OwnerId = i.CustomerId.Value, NoteId = i.NoteId }).Distinct().ToList();
                        }
                        else
                        {
                            returnedNotesFromCustomers = receiptNotesFromCustomerQable.Select(i => new ReportDeliveryItem()
                            {
                                PaidAmount = (double)i.DaTra,
                                TotalAmount = (double)i.TongTien,
                                OwnerId = i.KhachHang_MaKhachHang.Value,
                                ReturnedItem = true
                            }).ToList();
                            deliveryItemsGroupsByOwners = deliveryItems.GroupBy(i => i.CustomerId).ToList();
                            deliveryNotes = deliveryNotesQable.Where(i => noteIds.Contains(i.MaPhieuXuat) && i.KhachHang_MaKhachHang > 0)
                                .Select(i => new ReportDeliveryItem()
                                {
                                    PaidAmount = (double)i.DaTra,
                                    TotalAmount = (double)i.TongTien,
                                    OwnerId = i.KhachHang_MaKhachHang.Value,
                                    IsDebt = i.IsDebt ?? false
                                }).ToList();
                            var incomingNotes = _dataFilterService.GetValidInOutCommingNotes(drugStoreCode,
                                filter, new int[] { (int)InOutCommingType.Incomming });
                            incommingNoteAmountDict = incomingNotes.Select(i => new { CustomerId = i.KhachHang_MaKhachHang, i.Amount }).ToList()
                                .GroupBy(i => i.CustomerId).ToDictionary(i => i.Key, i => (double)i.Sum(ii => ii.Amount));
                        }
                        break;
                    case ReportByType.ByDoctor:
                        if (filter.HasDoctorIds)
                        {
                            deliveryItemsGroups = deliveryItems.Where(i => i.DoctorId > 0).GroupBy(i => new ReportBaseNoteItem()
                            { OwnerId = i.DoctorId.Value, NoteId = i.NoteId }).Distinct().ToList();
                        }
                        else
                        {
                            deliveryItemsGroupsByOwners = deliveryItems.Where(i => i.DoctorId > 0).GroupBy(i => i.DoctorId).ToList();
                            deliveryNotes = deliveryNotesQable.Where(i => noteIds.Contains(i.MaPhieuXuat) && i.BacSy_MaBacSy > 0)
                                .Select(i => new ReportDeliveryItem()
                                {
                                    PaidAmount = (double)i.DaTra,
                                    TotalAmount = (double)i.TongTien,
                                    OwnerId = i.BacSy_MaBacSy.Value
                                }).ToList();
                        }
                        break;
                    case ReportByType.BySupplyer:
                        if (filter.HasSupplyerIds)
                        {
                            deliveryItemsGroups = deliveryItems.GroupBy(i => new ReportBaseNoteItem()
                            { OwnerId = i.SupplyerId.Value, NoteId = i.NoteId }).Distinct().ToList();
                        }
                        else
                        {
                            deliveryItemsGroupsByOwners = deliveryItems.GroupBy(i => i.SupplyerId).ToList();
                            deliveryNotes = deliveryNotesQable.Where(i => noteIds.Contains(i.MaPhieuXuat))
                                .Select(i => new ReportDeliveryItem()
                                {
                                    PaidAmount = (double)i.DaTra,
                                    TotalAmount = (double)i.TongTien,
                                    OwnerId = i.NhaCungCap_MaNhaCungCap.Value
                                }).ToList();
                        }
                        break;
                    default:
                        break;
                }

                var order = filter.PageIndex * filter.PageSize;
                deliveryItemsGroups.ForEach(i =>
                {
                    var firstItem = i.FirstOrDefault();
                    var isDebt = i.Any(di => di.IsDebt);
                    var rptItem = new ReportByBaseItem()
                    {
                        ItemId = firstItem.NoteId,
                        ItemDate = firstItem.NoteDate,
                        ItemName = firstItem.CustomerName,
                        ItemNumber = firstItem.NoteNumber.ToString(),
                        Revenue = i.Sum(ii => ii.Revenue),
                        ReturnedItem = (firstItem.NoteType == (int)NoteInOutType.ReturnFromCustomer
                            || firstItem.NoteType == (int)NoteInOutType.ReturnToSupplier),
                    };
                    ReportDeliveryItem deliveryItem = null;
                    if (rptItem.ReturnedItem)
                    {
                        rptItem.ItemName = string.Format("{0} (Khách hàng trả lại)", rptItem.ItemName);
                        if (receiptNotesFromCustomerDict.ContainsKey(i.Key.NoteId))
                        {
                            deliveryItem = receiptNotesFromCustomerDict[i.Key.NoteId];
                            rptItem.PaidAmount = 0;
                            rptItem.TotalAmount = -deliveryItem.TotalAmount;
                            rptItem.DebtAmount = 0;
                        }
                    }
                    else if (deliveryNotesDict.ContainsKey(i.Key.NoteId))
                    {
                        deliveryItem = deliveryNotesDict[i.Key.NoteId];
                        rptItem.PaidAmount = deliveryItem.PaidAmount;
                        rptItem.TotalAmount = deliveryItem.TotalAmount;
                        rptItem.DebtAmount = rptItem.TotalAmount - rptItem.PaidAmount;
                        if (rptItem.DebtAmount < 0)
                        {
                            rptItem.DebtAmount = 0;
                        }
                        rptItem.IsDebt = isDebt && rptItem.DebtAmount > MedConstants.EspDebtAmount;
                    }

                    reportByItems.Add(rptItem);
                });
                if (reportByItems.Any())
                {
                    reportByItems = reportByItems.OrderByDescending(i => i.ItemDate).ToList();
                    reportByItems.ForEach(i =>
                    {
                        order++;
                        i.Order = order;
                    });
                }

                deliveryNotes.AddRange(returnedNotesFromCustomers.ToArray());
                deliveryItemsGroupsByOwners.ForEach(i =>
                {
                    order++;
                    var firstItem = i.FirstOrDefault();
                    var itemName = string.Empty;
                    var laterPaidAmount = 0.0;
                    switch (filter.ReportByTypeId)
                    {
                        case ReportByType.ByStaff:
                            itemName = firstItem.StaffName;
                            break;
                        case ReportByType.ByDoctor:
                            itemName = firstItem.DoctorName;
                            break;
                        case ReportByType.ByCustomer:
                            itemName = firstItem.CustomerName;
                            if (incommingNoteAmountDict.ContainsKey(firstItem.CustomerId))
                            {
                                laterPaidAmount = incommingNoteAmountDict[firstItem.CustomerId];
                            }
                            break;
                    }

                    var ownerDeliveryNotes = deliveryNotes.Where(ii => ii.OwnerId == i.Key).ToList();
                    var rptItem = new ReportByBaseItem()
                    {
                        Order = order,
                        ItemDate = null,
                        ItemName = itemName,
                        ItemNumber = string.Empty,
                        Revenue = i.Sum(ii => ii.Revenue),
                        LaterPaidAmount = laterPaidAmount,
                        PaidAmount = (double)ownerDeliveryNotes.Where(d => !d.ReturnedItem).Sum(ii => ii.PaidAmount),
                        TotalAmount = (double)ownerDeliveryNotes.Where(d => !d.ReturnedItem).Sum(ii => ii.TotalAmount),
                        ReturnedAmount = (double)ownerDeliveryNotes.Where(d => d.ReturnedItem).Sum(ii => ii.TotalAmount),
                        IsDebt = ownerDeliveryNotes.Any(d => d.IsDebt)
                    };

                    rptItem.DebtAmount = rptItem.TotalAmount - rptItem.ReturnedAmount - rptItem.PaidAmount - rptItem.LaterPaidAmount;
                    if (rptItem.DebtAmount < 0)
                    {
                        rptItem.DebtAmount = 0;
                    }
                    if (rptItem.TotalAmount < 0)
                    {
                        rptItem.TotalAmount = 0;
                    }
                    rptItem.IsDebt = rptItem.IsDebt && rptItem.DebtAmount > MedConstants.EspDebtAmount;
                    reportByItems.Add(rptItem);
                });
                if (filter.ReportByTypeId == ReportByType.ByCustomer && !filter.HasCustomerIds)
                {
                    reportByItems = reportByItems.Where(i => i.TotalAmount >= filter.MinValue).ToList();
                }
                if (deliveryItemsGroupsByOwners.Any())
                {
                    totalCount = reportByItems.Count();
                }
            } 
            
            result.TotalAmount = reportByItems.Sum(i => i.TotalAmount - i.ReturnedAmount);
            result.TotalRevenue = reportByItems.Sum(i => i.Revenue);
            result.TotalPaidAmount = reportByItems.Sum(i => i.PaidAmount);
            result.TotalLaterPaidAmount = reportByItems.Sum(i => i.LaterPaidAmount);
            result.TotalDebtAmount = reportByItems.Sum(i => i.DebtAmount);
            result.PagingResultModel = new PagingResultModel<ReportByBaseItem>(reportByItems, totalCount);

            return result;
        }
        private ReportByResponse GetReportByGoods(string drugStoreCode, FilterObject filter)
        {
            GenerateReportData(drugStoreCode);
            var result = new ReportByResponse();
            var reportByItems = new List<ReportByBaseItem>();
            var totalCount = 0;
            var deliveryStatuses = new int[] { (int)NoteInOutType.Delivery };
            using (var trans = TransactionScopeHelper.CreateReadUncommitted())
            {
                var deliveryCands = _dataFilterService.GetValidDeliveryItems(drugStoreCode, filter, deliveryStatuses);
                var deliveryDrugs = deliveryCands.Select(i => i.Thuoc_ThuocId).Distinct();
                totalCount = deliveryDrugs.Count();
                if (totalCount < 1)
                {
                    result.PagingResultModel = new PagingResultModel<ReportByBaseItem>(reportByItems, totalCount);
                    return result;
                }

                var drugIds = deliveryDrugs.OrderBy(i => i)
                    .ToPagedQueryable(filter.PageIndex, filter.PageSize, totalCount).ToList();
                var drugUnitRep = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, DonViTinh>>();
                var drugQueryable = (from dr in _dataFilterService.GetValidDrugs(drugStoreCode, null, false)
                                     join u in drugUnitRep.GetAll() on dr.DonViXuatLe_MaDonViTinh equals u.MaDonViTinh
                                     where drugIds.Contains(dr.ThuocId)
                                     select new
                                     {
                                         DrugId = dr.ThuocId,
                                         DrugCode = dr.MaThuoc,
                                         DrugName = dr.TenThuoc,
                                         DrugRetailUnitName = u.TenDonViTinh,
                                         DrugGroupId = dr.NhomThuoc_MaNhomThuoc
                                     });

                var drugs = drugQueryable.ToDictionary(i => i.DrugId, i => i);
                filter.DrugIds = drugIds.Select(i => i.Value).ToArray();
                var deliveryService = IoC.Container.Resolve<IDeliveryNoteService>();

                var deliveryItems = deliveryService.GetDeliveryNoteItems(drugStoreCode, filter, deliveryStatuses);
                var deliveryItemsByDrugs = deliveryItems.GroupBy(i => i.DrugId).ToDictionary(i => i.Key, i => i.ToList());
                var order = filter.PageIndex * filter.PageSize;
                if (filter.HasStaffIds)
                {
                    var deliveryDrugIds = deliveryItemsByDrugs.Select(i => i.Key).ToList();
                    drugs = drugs.Where(i => deliveryDrugIds.Contains(i.Key)).ToDictionary(i => i.Key, i => i.Value);
                }
                var deliveryItemIds = deliveryItems.Select(i => i.NoteItemId).Distinct().ToList();
                var reduceNoteItems = _dataFilterService.GetValidReduceNoteItems(drugStoreCode)
                    .Where(i => i.NoteTypeId == (int)NoteInOutType.ReturnFromCustomer && deliveryItemIds.Contains(i.ReduceNoteItemId))
                    .GroupBy(i => i.ReduceNoteItemId).ToDictionary(i => i.Key, i => i.Sum(ii => ii.ReduceQuantity));

                drugs.ForEach(i =>
                {
                    var drug = drugs[i.Key];
                    var rptItem = new ReportByGoodsItem()
                    {
                        Order = ++order,
                        ItemName = drug.DrugName,
                        ItemNumber = drug.DrugCode,
                        DrugUnit = drug.DrugRetailUnitName
                    };
                    if (deliveryItemsByDrugs.ContainsKey(i.Key))
                    {
                        var items = deliveryItemsByDrugs[i.Key];
                        rptItem.Quantity = items.Sum(ii => ii.FinalRetailQuantity);
                        rptItem.TotalAmount = items.Sum(ii => ii.FinalRetailAmount);
                        rptItem.Revenue = items.Sum(ii => ii.Revenue);
                        var deliveryItemIdsByDrug = items.Select(di => di.NoteItemId).ToList();
                        rptItem.ReturnedQuantity = reduceNoteItems.Where(r => deliveryItemIdsByDrug.Contains(r.Key)).Sum(r => r.Value);
                        if (rptItem.ReturnedQuantity > MedConstants.EspQuantity)
                        {
                            rptItem.ReturnedItem = true;
                        }
                    }

                    reportByItems.Add(rptItem);
                });
            }
            
            result.TotalAmount = reportByItems.Sum(i => i.TotalAmount);
            result.TotalRevenue = reportByItems.Sum(i => i.Revenue);
            result.PagingResultModel = new PagingResultModel<ReportByBaseItem>(reportByItems, totalCount);

            return result;
        }
        #endregion

        #region Private Methods
        private void ProcessReturnedFromCustomers(List<DeliveryNoteItemInfo> deliveryItems,
            List<ReceiptNoteItemInfo> receiptItems)
        {
            var returnedItems = receiptItems.Where(i => i.NoteType == (int) NoteInOutType.ReturnFromCustomer)
                .GroupBy(i => i.DrugId);
            var candidates = deliveryItems.Where(i => i.NoteType != (int)NoteInOutType.ReturnToSupplier)
                .GroupBy(i => i.DrugId).ToDictionary(i => i.Key, i => i.ToList());
            returnedItems.AsParallel().ForAll(cand =>
            {
                if (candidates.ContainsKey(cand.Key))
                {
                    ProcessReturnedItemsByDrug(candidates[cand.Key].Select(i => (BaseNoteItemInfo)i).ToList()
                        , (List<BaseNoteItemInfo>)cand.Select(i => (BaseNoteItemInfo)i).ToList());
                }
            });
        }

        private void ProcessReturnedToSupplyers(List<DeliveryNoteItemInfo> deliveryItems,
           List<ReceiptNoteItemInfo> receiptItems)
        {
            var returnedItems = deliveryItems.Where(i => i.NoteType == (int) NoteInOutType.ReturnToSupplier)
                .GroupBy(i => i.DrugId);
            var candidates = receiptItems.Where(i => i.NoteType != (int)NoteInOutType.ReturnFromCustomer)
                .GroupBy(i => i.DrugId).ToDictionary(i => i.Key, i => i.ToList());
            returnedItems.AsParallel().ForAll(cand =>
            {
                if (candidates.ContainsKey(cand.Key))
                {
                    ProcessReturnedItemsByDrug(candidates[cand.Key].Select(i => (BaseNoteItemInfo)i).ToList()
                        , (List<BaseNoteItemInfo>)cand.Select(i => (BaseNoteItemInfo)i).ToList());
                }
            });
        }

        private void ProcessReturnedItemsByDrug(List<BaseNoteItemInfo> noteItemsByDrug,
            List<BaseNoteItemInfo> returnedItems)
        {
            var totalReturnedQuantity = returnedItems.Sum(i => i.RetailQuantity);
            if (totalReturnedQuantity < MedConstants.EspQuantity) return;

            var maxReturnedDate = returnedItems.Max(i => i.NoteDate);
            var candidates = noteItemsByDrug.Where(i => i.NoteDate <= maxReturnedDate)
                .OrderByDescending(i => i.NoteDate).ThenByDescending(i => i.RetailQuantity).ToList();
            var quantity = 0.0;
            while (quantity < totalReturnedQuantity && candidates.Count > 0)
            {
                var firstItem = candidates.First();
                candidates.Remove(firstItem);
                var remainQuantity = totalReturnedQuantity - quantity;
                var usedQuantity = Math.Min(firstItem.RetailQuantity, remainQuantity);
                firstItem.RetailQuantity -= usedQuantity;
                quantity += usedQuantity;
            }
        }
        #endregion
    }
}
