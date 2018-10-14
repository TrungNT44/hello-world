using App.Common.Base;
using App.Common.DI;
using Med.Service.Admin;
using Med.Service.Background;
using Med.Service.Common;
using Med.Service.Delivery;
using Med.Service.Drug;
using Med.Service.Impl.Admin;
using Med.Service.Impl.Background;
using Med.Service.Impl.Common;
using Med.Service.Impl.Delivery;
using Med.Service.Impl.Drug;
using Med.Service.Impl.Log;
using Med.Service.Impl.Notifications;
using Med.Service.Impl.Receipt;
using Med.Service.Impl.Recruitment;
using Med.Service.Impl.Registration;
using Med.Service.Impl.Report;
using Med.Service.Impl.System;
using Med.Service.Impl.Utilities;
using Med.Service.Log;
using Med.Service.Notifications;
using Med.Service.Receipt;
using Med.Service.Recruitment;
using Med.Service.Registration;
using Med.Service.Report;
using Med.Service.System;
using Med.Service.Utilities;

namespace Med.Service.Impl
{
    public class Bootstrap : App.Common.Tasks.BaseTask<IBaseContainer>, IBootstrapper
    {
        public Bootstrap():base(AppBase.Instance.AppType)
        {

        }
        public override void Execute(IBaseContainer context)
        {
            base.Execute(context);     
        }

        public override void RegisterForWeb(IBaseContainer context)
        {
            context.RegisterHybridPerWebRequestPerThread<IUserService, UserService>();
            context.RegisterHybridPerWebRequestPerThread<IReportService, ReportService>();
            context.RegisterHybridPerWebRequestPerThread<IDeliveryNoteService, DeliveryNoteService>();
            context.RegisterHybridPerWebRequestPerThread<IReceiptNoteService, ReceiptNoteService>();
            context.RegisterHybridPerWebRequestPerThread<ICommonService, CommonService>();
            context.RegisterHybridPerWebRequestPerThread<IDataFilterService, DataFilterService>();
            context.RegisterHybridPerWebRequestPerThread<IDrugManagementService, DrugManagementService>();
            context.RegisterHybridPerWebRequestPerThread<IReportGenDataService, ReportGenDataService>();
            context.RegisterHybridPerWebRequestPerThread<IDrugStoreService, DrugStoreService>();
            context.RegisterHybridPerWebRequestPerThread<IRevenueDrugSynthesisReportService, RevenueDrugSynthesisReportService>();
            context.RegisterHybridPerWebRequestPerThread<ISynthesisReportService, SynthesisReportService>();
            context.RegisterHybridPerWebRequestPerThread<ICustomizeReportService, CustomizeReportService>();
            context.RegisterHybridPerWebRequestPerThread<IDrugWarehouseReportService, DrugWarehouseReportService>();
            context.RegisterHybridPerWebRequestPerThread<IUtilitiesService, UtilitiesService>();
            context.RegisterHybridPerWebRequestPerThread<ISystemService, SystemService>();
            context.RegisterHybridPerWebRequestPerThread<IRecruitService, RecruitService>();
            context.RegisterHybridPerWebRequestPerThread<INotificationBaseService, NotificationBaseService>();
            context.RegisterHybridPerWebRequestPerThread<INotificationService, NotificationService>();
            context.RegisterHybridPerWebRequestPerThread<IInOutCommingNoteService, InOutCommingNoteService>();
            context.RegisterHybridPerWebRequestPerThread<IAuditLogService, AuditLogService>();
            context.RegisterHybridPerWebRequestPerThread<IAdminService, AdminService>();
            context.RegisterHybridPerWebRequestPerThread<ITransactionReportService, TransactionReportService>();
            context.RegisterHybridPerWebRequestPerThread<IBackgroundService, BackgroundService>();
            context.RegisterHybridPerWebRequestPerThread<IInventoryService, InventoryService>();
            context.RegisterHybridPerWebRequestPerThread<IReportHelperService, ReportHelperService>();
            context.RegisterHybridPerWebRequestPerThread<ICleanUpService, CleanUpService>();
            context.RegisterHybridPerWebRequestPerThread<IInventoryAdjustmentService, InventoryAdjustmentService>();
            context.RegisterSingleton<IAppSettingService, AppSettingService>();
        }
        public override void RegisterForNonWeb(IBaseContainer context)
        {
            context.RegisterPerLifetimeScope<IUserService, UserService>();
            context.RegisterPerLifetimeScope<IReportService, ReportService>();
            context.RegisterPerLifetimeScope<IDeliveryNoteService, DeliveryNoteService>();
            context.RegisterPerLifetimeScope<IReceiptNoteService, ReceiptNoteService>();
            context.RegisterPerLifetimeScope<ICommonService, CommonService>();
            context.RegisterPerLifetimeScope<IDataFilterService, DataFilterService>();
            context.RegisterPerLifetimeScope<IDrugManagementService, DrugManagementService>();
            context.RegisterPerLifetimeScope<IReportGenDataService, ReportGenDataService>();
            context.RegisterPerLifetimeScope<IDrugStoreService, DrugStoreService>();
            context.RegisterPerLifetimeScope<IRevenueDrugSynthesisReportService, RevenueDrugSynthesisReportService>();
            context.RegisterPerLifetimeScope<ISynthesisReportService, SynthesisReportService>();
            context.RegisterPerLifetimeScope<ICustomizeReportService, CustomizeReportService>();
            context.RegisterPerLifetimeScope<IDrugWarehouseReportService, DrugWarehouseReportService>();
            context.RegisterPerLifetimeScope<IUtilitiesService, UtilitiesService>();
            context.RegisterPerLifetimeScope<ISystemService, SystemService>();
            context.RegisterPerLifetimeScope<IRecruitService, RecruitService>();
            context.RegisterPerLifetimeScope<IAuditLogService, AuditLogService>();
            context.RegisterPerLifetimeScope<INotificationBaseService, NotificationBaseService>();
            context.RegisterPerLifetimeScope<INotificationService, NotificationService>();
            context.RegisterPerLifetimeScope<IInOutCommingNoteService, InOutCommingNoteService>();
            context.RegisterPerLifetimeScope<IAdminService, AdminService>();
            context.RegisterPerLifetimeScope<ITransactionReportService, TransactionReportService>();
            context.RegisterPerLifetimeScope<IBackgroundService, BackgroundService>();
            context.RegisterPerLifetimeScope<IInventoryService, InventoryService>();
            context.RegisterPerLifetimeScope<IReportHelperService, ReportHelperService>();
            context.RegisterSingleton<ICleanUpService, CleanUpService>();
            context.RegisterPerLifetimeScope<IInventoryAdjustmentService, InventoryAdjustmentService>();            context.RegisterSingleton<IAppSettingService, AppSettingService>();
        }
    }
}
