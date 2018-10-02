using System;
using System.Collections.Generic;
using App.Common.Data;
using Med.Entity.Registration;
using Med.ServiceModel.Registration;
using Med.Entity;
using Med.ServiceModel.Report;
using Med.ServiceModel.Common;
using Med.ServiceModel.System;
using Med.Common;

namespace Med.Service.System
{
    public interface IAppSettingService
    {
        string GetSettingStringValue(string settingKey, string defaultValue = null);
        int GetSettingIntValue(string settingKey, int defaultValue = 0);
        float GetSettingFloatValue(string settingKey, float defaultValue = 0);
        DateTime GetSettingDateTimeValue(string settingKey, DateTime defaultValue = default(DateTime));
        ApplicationSetting[] GetAppSettings();
        bool UpdateAppSettings(ApplicationSetting[] settings);
    }
}
