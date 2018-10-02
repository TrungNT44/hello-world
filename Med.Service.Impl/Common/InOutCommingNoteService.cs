using System.Collections.Generic;
using System.Linq;
using App.Common.Data;
using Med.Entity.Registration;
using Med.Entity;
using Med.Repository.Factory;
using Med.Service.Base;
using Med.Service.Common;
using Med.ServiceModel.Drug;
using Med.ServiceModel.Common;
using System;
using Med.ServiceModel.Report;
using System.Threading.Tasks;
using App.Common.FaultHandling;
using App.Configuration;
using App.Common.Helpers;
using App.Common.Base;
using Med.Common;
using Med.Common.Enums;
using Med.ServiceModel.InOutComming;
using Med.DbContext;
using App.Common.DI;
using App.Common.Extensions;
using Med.Service.Receipt;
using App.Constants.Enums;

namespace Med.Service.Impl.Common
{
    public class InOutCommingNoteService: MedBaseService, IInOutCommingNoteService
    {
        #region Fields
        #endregion

        #region Constants
        private static readonly int EmptyNoteId = -1;
        private static readonly int AllNoteIds = 0;
        #endregion

        #region Interface Implementation  
        public long GetInOutCommingNoteNumber(string drugStoreCode, int noteTypeId)
        {
            var notes = _dataFilterService.GetValidInOutCommingNotes(drugStoreCode)
                .Where(i => i.LoaiPhieu == noteTypeId);
            var maxNumber = notes.Any() ? notes.Max(i => i.SoPhieu) : 0;

            return maxNumber + 1;
        }

        private int[] GetReceiverNoteId(int? inOutComingNoteId)
        {
            var receiverRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, InOutPaymentReceiverNote>>();
            int[] receiverrNoteIds = new int[] { };
            if (inOutComingNoteId > 0)
            {
                receiverrNoteIds = receiverRepo.GetAll().Where(i => i.InOutCommingNoteId == inOutComingNoteId && !i.IsDeleted)
                    .Select(i => i.ReceiverNoteId).ToArray();
            }

            return receiverrNoteIds;
        }

        public ReceiverDebtInfo GetReceiverDebtInfo(string drugStoreCode, int receiverId, int noteTypeId, int? inOutComingNoteId = null)
        {
            var result = new ReceiverDebtInfo()
            {
                DebtAmount = 0,
                DebtNotes = new List<ReceiverDebtNote>()
            };
            var debtNotes = new List<DebtNoteInfo>();
            var inOutNoteRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuThuChi>>();
            var receiverRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, InOutPaymentReceiverNote>>();
            var receiverNoteTypeId = (int)NoteInOutType.Delivery;
            var receiverrNoteIds = GetReceiverNoteId(inOutComingNoteId);
            
            if (noteTypeId == (int)InOutCommingType.Incomming)
            {
                var notes = _dataFilterService.GetValidDeliveryNotes(drugStoreCode, null, new int[] { (int)NoteInOutType.Delivery });
                if (receiverrNoteIds.Length > 0)
                {
                    notes = notes.Where(i => receiverrNoteIds.Contains(i.MaPhieuXuat));
                }
                else
                {
                    notes = notes.Where(i => i.IsDebt == true);
                    if (receiverId > 0)
                    {
                        notes = notes.Where(i => i.KhachHang_MaKhachHang == receiverId);
                    }
                }
               
                debtNotes = notes.Select(i => new DebtNoteInfo()
                {
                    NoteId = i.MaPhieuXuat,
                    NoteNumber = i.SoPhieuXuat,
                    NoteDate = i.NgayXuat.Value,
                    DebtAmount = (double)(i.TongTien - i.DaTra)
                }).ToList();
            }
            else if (noteTypeId == (int)InOutCommingType.Outcomming)
            {
                receiverNoteTypeId = (int)NoteInOutType.Receipt;
                var notes = _dataFilterService.GetValidReceiptNotes(drugStoreCode, null, new int[] { (int)NoteInOutType.Receipt });
                if (receiverrNoteIds.Length > 0)
                {
                    notes = notes.Where(i => receiverrNoteIds.Contains(i.MaPhieuNhap));
                }
                else
                {
                    notes = notes.Where(i => i.IsDebt == true);
                    if (receiverId > 0)
                    {
                        notes = notes.Where(i => i.NhaCungCap_MaNhaCungCap == receiverId);
                    }
                }
               
                debtNotes = notes.Select(i => new DebtNoteInfo()
                {
                    NoteId = i.MaPhieuNhap,
                    NoteNumber = i.SoPhieuNhap,
                    NoteDate = i.NgayNhap.Value,
                    DebtAmount = (double)(i.TongTien - i.DaTra)
                }).ToList();
            }
            if (inOutComingNoteId <= 0)
            {
                var receiverNoteIds = debtNotes.Select(i => i.NoteId).Distinct().ToList();

                var payments = (from r in receiverRepo.GetAll()
                                join io in inOutNoteRepo.GetAll() on r.InOutCommingNoteId equals io.MaPhieu
                                where r.ReceiverNoteTypeId == receiverNoteTypeId && !r.IsDeleted && receiverNoteIds.Contains(r.ReceiverNoteId)
                                select new
                                {
                                    r.ReceiverNoteId,
                                    PaymentAmount = (double)io.Amount
                                }).ToList().GroupBy(i => i.ReceiverNoteId).ToDictionary(i => i.Key, i => i.Sum(ii => ii.PaymentAmount));

                debtNotes.ForEach(i =>
                {
                    if (payments.ContainsKey(i.NoteId))
                    {
                        i.DebtAmount -= payments[i.NoteId];
                        i.PaymentAmount = payments[i.NoteId];
                    }
                });
                debtNotes = debtNotes.Where(i => i.DebtAmount > MedConstants.EspDebtAmount).ToList();
            }
            else if (receiverId <= 0)
            {
                var paymentAmount = (double)inOutNoteRepo.GetAll().Where(i => i.MaPhieu == inOutComingNoteId).Sum(i => i.Amount);
                debtNotes.ForEach(i =>
                {
                    i.DebtAmount = paymentAmount; ;
                    i.PaymentAmount = paymentAmount;
                });
            }

            result.DebtAmount = (double)debtNotes.Sum(i => i.DebtAmount);
            //result.DebtNotes.Add(new ReceiverDebtNote()
            //{
            //    NoteId = EmptyNoteId,
            //    NoteInfo = "--Phiếu trống--",
            //    DebtAmount = 0
            //});
            var firstItem = new ReceiverDebtNote()
            {
                NoteId = AllNoteIds,
                NoteInfo = "--Tất cả--",
                DebtAmount = result.DebtAmount
            };
            result.DebtNotes.Add(firstItem);
            var firstItemInfo = string.Empty;
            debtNotes.ForEach(i =>
            {
                var debtItem = new ReceiverDebtNote()
                {
                    NoteId = i.NoteId,
                    NoteInfo = string.Format("{0:dd/MM/yyyy} - {1}", i.NoteDate, i.NoteNumber),
                    DebtAmount = (double)i.DebtAmount
                };
                result.DebtNotes.Add(debtItem);
                if (string.IsNullOrEmpty(firstItemInfo))
                {
                    firstItemInfo = debtItem.NoteInfo;
                }
                else
                {
                    firstItemInfo = string.Format("{0}; {1}", firstItemInfo, debtItem.NoteInfo);
                }
            });
            if (receiverrNoteIds.Length > 1) // For all
            {
                firstItem.NoteInfo = firstItemInfo;
            }

            return result;
        }
        public InOutcommingNoteModel GetInOutcommingNoteModel(string drugStoreCode, int currentUserId,  
            int? noteId, int? noteTypeId, int? taskMode)
        {
            var result = new InOutcommingNoteModel()
            {
                NoteId = noteId ?? 0,
                CreatedById = currentUserId,
                TaskMode = (int)TaskMode.Create,
                NoteDate = DateTime.Now
            };
            if (taskMode.HasValue)
            {
                result.TaskMode = taskMode.Value;
            }
          
            if (noteId > 0) // Edit mode
            {
                var note = _dataFilterService.GetValidInOutCommingNotes(drugStoreCode)
                    .Where(i => i.MaPhieu == noteId)
                    .FirstOrDefault();
                var createdById = note.CreatedBy_UserId.Value;
                result.CreatedById = createdById;
                var createdByName = _dataFilterService.GetValidUsers(drugStoreCode)
                    .Where(i => i.UserId == createdById).Select(i => i.TenDayDu).FirstOrDefault();
                
                result.CreatedByName = createdByName;
                result.Description = note.DienGiai;
                result.PaymentAmount = (double)note.Amount;
                result.ReceiverId = noteTypeId == (int)InOutCommingType.Incomming ?  (note.KhachHang_MaKhachHang ?? 0) : (note.NhaCungCap_MaNhaCungCap ?? 0);
                result.NoteDate = note.NgayTao;
                result.NoteNumber = note.SoPhieu;
                result.NoteTypeId = note.LoaiPhieu;
                var receiverNoteIds = GetReceiverNoteId(noteId);
                if (receiverNoteIds.Length > 0)
                {
                    result.ReceiverNoteId = (receiverNoteIds.Length > 1) ? AllNoteIds : receiverNoteIds[0];
                }
            }
            else
            {
                result.NoteDate = DateTime.Now.AddDays(-1);
                result.NoteTypeId = noteTypeId ?? (int)InOutCommingType.Incomming;
                result.NoteNumber = GetInOutCommingNoteNumber(drugStoreCode, result.NoteTypeId);               
            }

            return result;
        }

        public int SaveInOutCommingNote(string drugStoreCode, int currentUserId, InOutcommingNoteModel model)
        {
            var retVal = 0;
            var noteDate = model.NoteDate.WithCurrentTime();

            var inOutCommingRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuThuChi>>();
            var receiverRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, InOutPaymentReceiverNote>>();
            var deliveryRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>();
            var receiptRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
            if (model.NoteId > 0) // Edit mode
            {
                inOutCommingRepo.UpdateMany(i => i.MaPhieu == model.NoteId, i => new PhieuThuChi()
                {
                    ModifiedBy_UserId = currentUserId,
                    Modified = DateTime.Now,
                    UserProfile_UserId = currentUserId,
                    DienGiai = model.Description,
                    Amount = (decimal)model.PaymentAmount,
                    NgayTao = noteDate
                });
                retVal = model.NoteId;
            }
            else // New mode
            {                
                var newNote = new PhieuThuChi()
                {
                    Active = true,
                    Created = DateTime.Now,
                    DienGiai = model.Description,
                    Amount = (decimal)model.PaymentAmount,
                    LoaiPhieu = model.NoteTypeId,
                    NhaThuoc_MaNhaThuoc = drugStoreCode,
                    CreatedBy_UserId = currentUserId,
                    SoPhieu = (int)model.NoteNumber,
                    NgayTao = noteDate,
                    UserProfile_UserId = currentUserId
                };
                if (model.NoteTypeId == (int)InOutCommingType.Incomming)
                {
                    newNote.KhachHang_MaKhachHang = model.ReceiverId;
                }
                else
                {
                    newNote.NhaCungCap_MaNhaCungCap = model.ReceiverId;
                }
                inOutCommingRepo.Add(newNote);
                inOutCommingRepo.Commit();
                retVal = newNote.MaPhieu;

                if (model.ReceiverNoteId == AllNoteIds && model.ReceiverNoteIds != null && model.ReceiverNoteIds.Length > 0) // All notes
                {
                    var paymentNotes = model.ReceiverNoteIds.Select(i => new InOutPaymentReceiverNote()
                    {
                        DrugStoreCode = drugStoreCode,
                        InOutCommingNoteId = newNote.MaPhieu,
                        IsDeleted = false,
                        ReceiverNoteId = i,
                        ReceiverNoteTypeId = model.NoteTypeId == (int)InOutCommingType.Incomming ?
                            (int)NoteInOutType.Delivery : (int)NoteInOutType.Receipt,
                        CreatedDateTime = DateTime.Now
                    }).ToList();
                    receiverRepo.InsertMany(paymentNotes);
                    if (model.NoteTypeId == (int)InOutCommingType.Incomming)
                    {
                        deliveryRepo.UpdateMany(i => model.ReceiverNoteIds.Contains(i.MaPhieuXuat), i => new PhieuXuat()
                        {
                            IsDebt = false
                        });
                    }
                    else
                    {
                        receiptRepo.UpdateMany(i => model.ReceiverNoteIds.Contains(i.MaPhieuNhap), i => new PhieuNhap()
                        {
                            IsDebt = false
                        });
                    }
                }
                else if (model.ReceiverNoteId > 0) // Specific note
                {
                    receiverRepo.Add(new InOutPaymentReceiverNote()
                    {
                        DrugStoreCode = drugStoreCode,
                        InOutCommingNoteId = newNote.MaPhieu,
                        IsDeleted = false,
                        ReceiverNoteId = model.ReceiverNoteId,
                        ReceiverNoteTypeId = model.NoteTypeId == (int)InOutCommingType.Incomming ?
                            (int)NoteInOutType.Delivery : (int)NoteInOutType.Receipt,
                        CreatedDateTime = DateTime.Now
                    });
                    receiptRepo.Commit();
                    if (model.NoteTypeId == (int)InOutCommingType.Incomming)
                    {
                        deliveryRepo.UpdateMany(i => model.ReceiverNoteId == i.MaPhieuXuat && model.PaymentAmountWithEsp >= (double)(i.TongTien - i.DaTra) ,
                            i => new PhieuXuat()
                            {
                                IsDebt = false
                            });
                    }
                    else
                    {
                        receiptRepo.UpdateMany(i => model.ReceiverNoteId == i.MaPhieuNhap && model.PaymentAmountWithEsp >= (double)(i.TongTien - i.DaTra) , 
                            i => new PhieuNhap()
                            {
                                IsDebt = false
                            });
                    }
                }
            }

            return retVal;
        }

        protected class TransitDeliveryNoteItem
        {
            public int DrugId { get; set; }
            public int UnitId { get; set; }
            public double Quantiy { get; set; }
            public double Price { get; set; }
            public double Discount { get; set; }
            public string Batch { get; set; }
            public DateTime Expdate { get; set; }
            public double Total { get; set; }
        }
        public int TransitWarehouse(string drugStoreCode, string targetDrugStoreCode, int deliveryNoteId, int userId)
        {
            int newNoteId = 0;

            var deliverNote = _dataFilterService.GetValidDeliveryNotes(drugStoreCode)
                .Where(i => i.MaPhieuXuat == deliveryNoteId)
                .Select(i => new { NoteDate = i.Created, NoteNumber = i.SoPhieuXuat }).FirstOrDefault();
            var deliveryItems = _dataFilterService.GetValidDeliveryNoteItems(drugStoreCode)
                .Where(i => i.NoteId == deliveryNoteId)
                .Select(i => new TransitDeliveryNoteItem()
                {
                    DrugId = i.DrugId,
                    UnitId = i.UnitId,
                    Quantiy = i.Quantity,
                    Price = i.Price,
                    Discount = i.Discount
                }).ToList();

            var drugIds = deliveryItems.Select(i => i.DrugId).Distinct().ToList();
            var drugPrices = _dataFilterService.GetValidDrugs(drugStoreCode)
                .Where(i => drugIds.Contains(i.ThuocId))
                .Select(i => new
                {
                    DrugId = i.ThuocId,
                    OutRetailPrice = (double)i.GiaBanLe,
                    InRetailPrice = (double)i.GiaNhap,
                    RetailUnitId = i.DonViXuatLe_MaDonViTinh.Value,
                    Factors = i.HeSo
                }).ToList().ToDictionary(i => i.DrugId, i => i);
            deliveryItems.ForEach(i =>
            {
                var drug = drugPrices[i.DrugId];
                var price = drug.InRetailPrice;
                var factors = 1.0;
                if (i.UnitId != drug.RetailUnitId )
                {
                    factors = drug.Factors;
                }
                i.Price = price * factors;
                i.Total = i.Price * i.Quantiy;
            });

            var receiptNoteService = IoC.Container.Resolve<IReceiptNoteService>();
            var receiptNoteRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
            var drugStoreRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaThuoc>>();
            var receiptNoteItemRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhapChiTiet>>();
            var comService = IoC.Container.Resolve<ICommonService>();
            var supplierName = drugStoreRepo.GetAll().Where(i => i.MaNhaThuoc == drugStoreCode)
                .Select(i => i.TenNhaThuoc).FirstOrDefault();           
            var supplierId = comService.CreateUniqueInternalSupplier(targetDrugStoreCode, supplierName, userId);

            var noteTotal = deliveryItems.Sum(i => i.Total);
            var newNote = new PhieuNhap()
            {
                Active = true,
                Created = DateTime.Now,
                CreatedBy_UserId = userId,
                DaTra = (Decimal)noteTotal,
                DienGiai = string.Format("Phiếu nhập chuyển kho từ nhà thuốc: {0}. Có số phiếu: {1}", supplierName, deliverNote.NoteNumber),
                NgayNhap = deliverNote.NoteDate,
                LoaiXuatNhap_MaLoaiXuatNhap = (int)NoteInOutType.Receipt,
                TongTien = (Decimal)noteTotal,
                UserProfile_UserId = userId,
                NhaThuoc_MaNhaThuoc = targetDrugStoreCode,
                SoPhieuNhap = receiptNoteService.GetNewReceiptNoteNumber(targetDrugStoreCode),
                NhaCungCap_MaNhaCungCap = supplierId
            };
            receiptNoteRepo.Add(newNote);
            receiptNoteRepo.Commit();
            newNoteId = newNote.MaPhieuNhap;
            if (newNoteId < 1) return newNoteId;
           

            var targetDeliveryItems = deliveryItems.Select(i => new PhieuNhapChiTiet()
            {
                ChietKhau = (Decimal)i.Discount,
                DonViTinh_MaDonViTinh = i.UnitId,
                GiaNhap = (Decimal)i.Price,
                IsModified = true,
                NhaThuoc_MaNhaThuoc = targetDrugStoreCode,
                PhieuNhap_MaPhieuNhap = newNote.MaPhieuNhap,
                SoLuong = (Decimal)i.Quantiy,
                Thuoc_ThuocId = i.DrugId
            }).ToList();
            receiptNoteItemRepo.InsertMany(targetDeliveryItems);

            var whTransitRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, WarehouseTransition>>();
            whTransitRepo.Add(new WarehouseTransition()
            {
                DeliveryNoteId = deliveryNoteId,
                ReceiptNoteId = newNote.MaPhieuNhap,
                RecordStatusId = (int)RecordStatus.Activated,
                CreatedDateTime = DateTime.Now
            });
            whTransitRepo.Commit();

            return newNoteId;
        }

        public bool DeleteInOutCommingNote(string drugStoreCode, int noteId)
        {
            var retVal = false;
            var inOutCommingRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuThuChi>>();
            var receiverRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, InOutPaymentReceiverNote>>();
            var receiptNoteRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuNhap>>();
            var deliveryNoteRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, PhieuXuat>>();
            var receiverNotes = receiverRepo.GetAll().Where(i => i.InOutCommingNoteId == noteId)
                .Select(i => new { i.ReceiverNoteId, i.ReceiverNoteTypeId }).ToList();
            var receiptNoteIds = receiverNotes.Where(i => i.ReceiverNoteTypeId == (int)NoteInOutType.Receipt)
                .Select(i => i.ReceiverNoteId).ToList();
            var deliveryNoteIds = receiverNotes.Where(i => i.ReceiverNoteTypeId == (int)NoteInOutType.Delivery)
                .Select(i => i.ReceiverNoteId).ToList();
            try
            {
                //using (var tran = TransactionScopeHelper.CreateReadCommittedForWrite())
                {
                    inOutCommingRepo.Delete(i => i.MaPhieu == noteId);
                    receiverRepo.Delete(i => i.InOutCommingNoteId == noteId);
                    receiptNoteRepo.UpdateMany(i => receiptNoteIds.Contains(i.MaPhieuNhap), i => new PhieuNhap() { IsDebt = true });
                    deliveryNoteRepo.UpdateMany(i => deliveryNoteIds.Contains(i.MaPhieuXuat), i => new PhieuXuat() { IsDebt = true });

                    //tran.Complete();
                    retVal = true;
                }                
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this, string.Format("DeleteInOutCommingNote - NoteID: {0}", noteId));
            }

            return retVal;
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
