using App.Common.Data;
using Med.DbContext;
using App.Common.DI;
using Med.Service.Base;
using global::System;
using Med.Entity;
using global::System.Linq;
using App.Common.Helpers;
using Med.Service.Background;
using Med.Service.Report;
using Med.Service.Utilities;
using Med.Service.Drug;
using Med.Entity.Report;
using Med.Entity.Drug;
using Med.Common.Enums;

namespace Med.Service.Impl.Background
{
    public class BackgroundService : MedBaseService, IBackgroundService
    {  
        public void MakeAffectedChangesRelatedDeliveryNotes(string drugStoreID, params int[] noteIds)
        {
            LogHelper.Debug("Make affected changed related to delivery notes: {0}.", string.Join(",", noteIds));

            var noteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var drugIds = noteItemRepo.GetAll().Where(i => noteIds.Contains(i.PhieuXuat_MaPhieuXuat.Value)
                && i.IsModified && i.NhaThuoc_MaNhaThuoc == drugStoreID)
                .Select(i => i.Thuoc_ThuocId.Value).ToArray();
            if (!drugIds.Any()) return;

            // Generate inventory for drugs.
            var invService = IoC.Container.Resolve<IInventoryService>();
            invService.TryToGenerateInventory4Drugs(drugStoreID, false, false, true, drugIds);

            // Make affected changes
            var rtpHelperService = IoC.Container.Resolve<IReportHelperService>();
            rtpHelperService.TryMakeAffectedChangesByDeliveryNotes(drugStoreID, noteIds);
        }
        public void MakeAffectedChangesRelatedReceiptNotes(string drugStoreID, params int[] noteIds)
        {
            LogHelper.Debug("Make affected changed related to receipt notes: {0}.", string.Join(",", noteIds));
            var noteRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
            var noteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var noteItems = (from ni in noteItemRepo.GetAll()
                             join n in noteRepo.GetAll() on ni.PhieuNhap_MaPhieuNhap equals n.MaPhieuNhap
                             where noteIds.Contains(n.MaPhieuNhap)
                                 && ni.IsModified && n.NhaThuoc_MaNhaThuoc == drugStoreID
                             select new
                             {
                                 DrugID = ni.Thuoc_ThuocId.Value,
                                 NoteTypeID = n.LoaiXuatNhap_MaLoaiXuatNhap.Value,
                                 ni.RetailOutPrice,
                                 ni.RetailPrice
                             }).ToList();
            if (!noteItems.Any()) return;
            var receiptNoteItems = noteItems.Where(i => i.NoteTypeID == (int)NoteInOutType.Receipt).ToList();
            var dsSettings = IoC.Container.Resolve<IUtilitiesService>().GetDrugStoreSetting(drugStoreID);
            if (dsSettings != null && dsSettings.AutoUpdateInOutPriceOnNote && receiptNoteItems.Any())
            {
                var drugPrices = receiptNoteItems.GroupBy(i => i.DrugID).Select(i => new Thuoc()
                {
                    ThuocId = i.Key,
                    GiaNhap = (Decimal)i.First().RetailPrice,
                    GiaBanLe = (Decimal)i.First().RetailOutPrice,
                }).ToList();
                var drugRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>();
                drugRepo.UpdateMany(drugPrices, i => i.GiaNhap, i => i.GiaBanLe);
            }
            var drugIds = noteItems.Select(i => i.DrugID).Distinct().ToArray();

            // Generate inventory for drugs.
            var invService = IoC.Container.Resolve<IInventoryService>();
            invService.TryToGenerateInventory4Drugs(drugStoreID, false, false, true, drugIds);

            // Make affected changes
            var rtpHelperService = IoC.Container.Resolve<IReportHelperService>();
            rtpHelperService.TryToMakeAffectedChangesByReceiptNotes(drugStoreID, noteIds);
        }

        public void MakeAffectedChangesByUpdatedDrugs(string drugStoreID, params int[] drugIds)
        {
            var rtpHelperService = IoC.Container.Resolve<IReportHelperService>();
            rtpHelperService.MakeAffectedChangesByUpdatedDrugs(drugStoreID, drugIds);
        }

        public void DeleteForeverDrugs(string drugStoreID, params int[] drugIds)
        {
            if (drugIds == null || !drugIds.Any()) return;

            var rNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var dNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var invNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuKiemKeChiTiet>>();
            var sumPeriodNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, TongKetKyChiTiet>>();
            var sumPeriodNoteRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, TongKetKy>>();
            var invRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Inventory>>();
            var drugMappingRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, DrugMapping>>();
            var drugRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Thuoc>>();
       
            var priceRefRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReceiptDrugPriceRef>>();
            var reduceRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReduceNoteItem>>();
            var snapshotRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, DeliveryNoteItemSnapshotInfo>>();

            using (var trans = TransactionScopeHelper.CreateReadCommittedForWrite())
            {
                priceRefRepo.Delete(i => i.DrugStoreCode == drugStoreID
                   && drugIds.Contains(i.DrugId));
                reduceRepo.Delete(i => i.DrugStoreCode == drugStoreID
                   && drugIds.Contains(i.DrugID));
                snapshotRepo.Delete(i => i.DrugStoreCode == drugStoreID
                   && drugIds.Contains(i.DrugId));

                rNoteItemRepo.Delete(i => i.NhaThuoc_MaNhaThuoc == drugStoreID
                    && drugIds.Contains(i.Thuoc_ThuocId.Value));
                dNoteItemRepo.Delete(i => i.NhaThuoc_MaNhaThuoc == drugStoreID
                   && drugIds.Contains(i.Thuoc_ThuocId.Value));
                invNoteItemRepo.Delete(i => drugIds.Contains(i.Thuoc_ThuocId.Value));
                sumPeriodNoteItemRepo.Delete(i => i.NhaThuoc_MaNhaThuoc == drugStoreID
                   && drugIds.Contains(i.Thuoc_ThuocId.Value));
                sumPeriodNoteRepo.Delete(i => i.NhaThuoc_MaNhaThuoc == drugStoreID
                   && drugIds.Contains(i.Thuoc_ThuocId.Value));
                invRepo.Delete(i => i.DrugStoreID == drugStoreID
                   && drugIds.Contains(i.DrugID));
                drugMappingRepo.Delete(i => i.DrugStoreID == drugStoreID
                   && drugIds.Contains(i.SlaveDrugID));
                drugRepo.Delete(i => i.NhaThuoc_MaNhaThuoc == drugStoreID
                   && drugIds.Contains(i.ThuocId));

                trans.Complete();
            }
        }
    }
}
