using App.Common.Data;
using global::System.Collections.Generic;
using Med.Service.Log;
using Med.Entity.Log;
using Med.DbContext;
using App.Common.DI;
using Med.Service.Base;
using Med.ServiceModel.Log;
using global::System;
using Newtonsoft.Json;
using Med.Entity;
using global::System.Linq;
using App.Common.Helpers;
using Med.Common.Enums;

namespace Med.Service.Impl.Log
{
    public class AuditLogService : BaseService, IAuditLogService
    {
        public void Add(IList<Audit> audits)
        {
            if (audits.Count == 0) return;

            var auditRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbLogContext, Audit>>();
            if (audits.Count == 1)
            {
                auditRepo.Add(audits[0]);
                auditRepo.Commit();
            }
            else
            {
                auditRepo.InsertMany(audits, true);
            }
        }

        public void Add(Audit audit)
        {
            var auditRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbLogContext, Audit>>();
            auditRepo.Add(audit);
            auditRepo.Commit();
        }
        public void Add(HistoryBaseModel history, int hisEntityTypeId)
        {
            var auditRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbLogContext, AuditHistory>>();
            var audit = new AuditHistory()
            {
                DrugStoreCode = history.DrugStoreCode,
                CreatedDatetime = DateTime.Now,
                HisEntityTypeID = hisEntityTypeId,
                HisEntityID = history.EntityID,
                EntityContent = JsonConvert.SerializeObject(history),
                ActorID = history.ActorID
            };
            auditRepo.Add(audit);
            auditRepo.Commit();
        }
        public void LogReceiptNote(string drugStoreCode, int noteId, int? actorId)
        {
            var noteRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
            var noteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var note = noteRepo.TableAsNoTracking.Where(i => i.MaPhieuNhap == noteId).FirstOrDefault();
            if (note == null)
            {
                LogHelper.Debug("Don't exist note id: {0}", noteId);

                return;
            }

            var noteItems = noteItemRepo.TableAsNoTracking.Where(i => i.PhieuNhap_MaPhieuNhap == noteId && i.NhaThuoc_MaNhaThuoc == drugStoreCode)
                .ToList().Select(i => (BaseEntity)i);
            var hisModel = new NoteHistoryModel()
            {
                EntityID = noteId,
                DrugStoreCode = drugStoreCode,
                Note = note,
                NoteItems = noteItems,
                ActorID = actorId
            };
            Add(hisModel, (int)HistoryEntityType.Receipt);
        }
        public void LogDeliveryNote(string drugStoreCode, int noteId, int? actorId)
        {
            var noteRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>();
            var noteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuatChiTiet>>();
            var note = noteRepo.TableAsNoTracking.Where(i => i.MaPhieuXuat == noteId).FirstOrDefault();
            if (note == null)
            {
                LogHelper.Debug("Don't exist note id: {0}", noteId);

                return;
            }

            var noteItems = noteItemRepo.TableAsNoTracking.Where(i => i.PhieuXuat_MaPhieuXuat == noteId && i.NhaThuoc_MaNhaThuoc == drugStoreCode)
                .ToList().Select(i => (BaseEntity)i);
            var hisModel = new NoteHistoryModel()
            {
                EntityID = noteId,
                DrugStoreCode = drugStoreCode,
                Note = note,
                NoteItems = noteItems,
                ActorID = actorId
            };
            Add(hisModel, (int)HistoryEntityType.Delivery);
        }
    }
}
