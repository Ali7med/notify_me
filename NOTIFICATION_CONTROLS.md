# تحكم في الإشعارات - Notifications Control

## نافذة الإعدادات - Settings Window

تم إضافة خيارات التحكم في الإشعارات في نافذة الإعدادات!

### الوصول إلى الإعدادات

1. **من System Tray**: اضغط بالزر الأيمن على أيقونة NotifyMe → اختر "Settings"
2. **من النافذة العائمة**: اضغط على أيقونة الإعدادات ⚙️

### الخيارات المتوفرة

#### 📢 Enable Toast Notifications
- **الوصف**: تفعيل/إيقاف الإشعارات المنبثقة (Windows Toast Notifications)
- **الافتراضي**: ✅ مُفعَّل
- **عند التعطيل**: لن تظهر إشعارات عند قطع/عودة الاتصال

#### 🔊 Enable Sound Notifications
- **الوصف**: تفعيل/إيقاف الأصوات عند تغيير حالة الاتصال
- **الافتراضي**: ✅ مُفعَّل
- **عند التعطيل**: لن تُشغَّل أصوات عند قطع/عودة الاتصال

### السيناريوهات المختلفة

| الإشعارات | الأصوات | النتيجة |
|-----------|---------|---------|
| ✅ مُفعَّل | ✅ مُفعَّل | إشعار + صوت |
| ✅ مُفعَّل | ❌ معطّل | إشعار فقط (بدون صوت) |
| ❌ معطّل | ✅ مُفعَّل | صوت فقط (بدون إشعار) |
| ❌ معطّل | ❌ معطّل | لا إشعارات ولا أصوات |

### الإعدادات الأخرى

#### 🎨 Appearance
- **Widget Opacity**: شفافية النافذة العائمة (10% - 100%)
- **Theme**: Glass (حديث وشفاف) أو Classic (صلب)

#### 🌐 Network
- **Ping IP Address**: عنوان IP لاختبار الاتصال (الافتراضي: 8.8.8.8)

#### ⚙️ Advanced
- **Update Interval**: معدل تحديث البيانات (1-10 ثانية)
- **High Traffic Threshold**: عتبة الحركة العالية بالميجابايت/ثانية

### حفظ التغييرات

1. قم بتعديل الإعدادات المطلوبة
2. اضغط على **"Save Changes"**
3. ستُحفظ الإعدادات في:
   ```
   %LocalAppData%\NotifyMe\appsettings.json
   ```

### ملاحظات مهمة

- ✅ التغييرات تُحفظ فوراً عند الضغط على "Save"
- ✅ التطبيق يقرأ الإعدادات تلقائياً عند كل تشغيل
- ✅ يمكن تعديل ملف JSON مباشرة إذا لزم الأمر
- ⚠️ بعض التغييرات (مثل الشفافية والثيم) يتم تطبيقها مباشرة
- ⚠️ تغييرات الإشعارات تؤثر على الأحداث الجديدة فقط

---

## Notifications Control

### Settings Window Access

1. **From System Tray**: Right-click NotifyMe icon → Select "Settings"
2. **From Floating Widget**: Click settings icon ⚙️

### Available Options

#### 📢 Enable Toast Notifications
- **Description**: Toggle Windows Toast Notifications on connection changes
- **Default**: ✅ Enabled
- **When disabled**: No toast popups will appear

#### 🔊 Enable Sound Notifications
- **Description**: Toggle sound alerts on connection changes
- **Default**: ✅ Enabled
- **When disabled**: No sounds will play

### How to Test

1. Open Settings window
2. Toggle the checkboxes as desired
3. Click "Save Changes"
4. Test by disconnecting/reconnecting your internet
5. Observe the behavior based on your settings

### Settings File Location

```
%LocalAppData%\NotifyMe\appsettings.json
```

Example content:
```json
{
  "Opacity": 0.8,
  "Theme": "Glass",
  "IsTransparent": true,
  "PingHost": "8.8.8.8",
  "UpdateIntervalSeconds": 1,
  "HighTrafficThresholdMBps": 5.0,
  "EnableSoundNotifications": true,
  "EnableToastNotifications": true
}
```
