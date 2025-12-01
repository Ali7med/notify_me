# NotifyMe - سجل التقدم والإنجازات

## 📅 **التاريخ**: 23 نوفمبر 2025

---

## 🎯 **نظرة عامة على المشروع**

**NotifyMe** هو تطبيق مراقبة شبكة متقدم مبني بـ .NET 10 و WPF، يوفر مراقبة في الوقت الفعلي لحالة الاتصال بالإنترنت واستهلاك الشبكة مع واجهة مستخدم احترافية.

---

## ✅ **المراحل المكتملة**

### **المرحلة 1: الأيقونة العائمة** ✅ (مكتمل 100%)
**التاريخ**: نوفمبر 2025

#### الميزات المنفذة:
- ✅ نافذة WPF شفافة قابلة للسحب على سطح المكتب
- ✅ تصميم "Dynamic Pill" (كبسولة أفقية) بدلاً من الدائرة
- ✅ تصميم "Bank UI" احترافي مع عرض البيانات بشكل منظم
- ✅ تحديث تلقائي للبيانات كل ثانية (قابل للتخصيص)
- ✅ تغيير اللون حسب حالة الاتصال:
  - 🔴 أحمر: قطع الاتصال
  - 🔵 أزرق: اتصال نشط مع بيانات
  - ⚪ رمادي: اتصال بدون بيانات
- ✅ عرض سرعات التحميل والرفع في الوقت الفعلي
- ✅ عرض Ping latency مع تلوين حسب السرعة
- ✅ تأثيرات Hover وانيميشن عند التمرير

#### الملفات المتأثرة:
- `NotifyMe.UI/FloatingIconWindow.xaml`
- `NotifyMe.UI/FloatingIconWindow.xaml.cs`

---

### **المرحلة 2: دمج مع الصينية** ✅ (مكتمل 100%)
**التاريخ**: نوفمبر 2025

#### الميزات المنفذة:
- ✅ أيقونة في System Tray مع Tooltip ديناميكي
- ✅ نقر مزدوج لفتح النافذة الرئيسية
- ✅ قائمة سياق (Context Menu) محسّنة بتصميم داكن
- ✅ خيارات القائمة:
  - Show Statistics (النافذة الرئيسية)
  - Show Widget (الأيقونة العائمة)
  - View History (سجل البيانات) 🆕
  - Settings (الإعدادات)
  - Exit (الخروج)

#### الملفات المتأثرة:
- `NotifyMe.UI/App.xaml`
- `NotifyMe.UI/App.xaml.cs`

---

### **المرحلة 3: الإعدادات المتقدمة** ✅ (مكتمل 100%)
**التاريخ**: 23 نوفمبر 2025

#### الميزات المنفذة:

##### **إعدادات المظهر:**
- ✅ التحكم في شفافية الـ Widget (10%-100%)
- ✅ اختيار Theme (Glass/Classic)

##### **إعدادات الشبكة:**
- ✅ تخصيص Ping IP Address (افتراضي: 8.8.8.8)
- ✅ التحقق من صحة IP قبل الحفظ

##### **الإعدادات المتقدمة:**
- ✅ فترة التحديث (Update Interval): 1-10 ثواني
- ✅ حد استهلاك الشبكة العالي (High Traffic Threshold)
- ✅ تفعيل/تعطيل الإشعارات الصوتية

##### **تحسينات واجهة الإعدادات:**
- ✅ تصميم داكن احترافي
- ✅ ScrollViewer للتمرير عبر جميع الإعدادات
- ✅ حجم النافذة: 650x550 بكسل
- ✅ حفظ فوري للإعدادات مع معاينة مباشرة

#### الملفات المتأثرة:
- `NotifyMe.Models/UserSettings.cs`
- `NotifyMe.UI/SettingsWindow.xaml`
- `NotifyMe.UI/SettingsWindow.xaml.cs`
- `NotifyMe.Core/Services/SettingsService.cs`

---

### **المرحلة 4: تسجيل البيانات التاريخية** ✅ (مكتمل 95%)
**التاريخ**: 23 نوفمبر 2025

#### الميزات المنفذة:

##### **قاعدة البيانات:**
- ✅ SQLite لتخزين البيانات محلياً
- ✅ موقع قاعدة البيانات: `%LocalAppData%\NotifyMe\network_logs.db`
- ✅ جدول `NetworkLogs` مع الحقول:
  - Timestamp (التاريخ والوقت)
  - IsConnected (حالة الاتصال)
  - DownloadSpeedBps (سرعة التحميل)
  - UploadSpeedBps (سرعة الرفع)
  - Latency (زمن الاستجابة)

##### **خدمة التسجيل (DataLogger):**
- ✅ تسجيل تلقائي لجميع البيانات
- ✅ استرجاع السجلات حسب نطاق زمني
- ✅ حذف السجلات القديمة (أكثر من 30 يوم)
- ✅ معالجة الأخطاء لضمان عدم تعطل التطبيق

##### **نافذة History (السجل):**
- ✅ واجهة احترافية مع DataGrid
- ✅ عرض البيانات بشكل منظم:
  - الوقت والتاريخ
  - حالة الاتصال
  - سرعات التحميل والرفع (منسقة)
  - Ping (مع عرض TIMEOUT للأخطاء)
- ✅ فلاتر زمنية:
  - آخر ساعة
  - آخر 24 ساعة
  - آخر 7 أيام
  - آخر 30 يوم
  - جميع السجلات
- ✅ زر Refresh لتحديث البيانات
- ✅ زر لحذف السجلات القديمة
- ✅ عداد السجلات المعروضة

#### الملفات المتأثرة:
- `NotifyMe.Models/NetworkLog.cs` (جديد)
- `NotifyMe.Core/Services/DataLogger.cs` (جديد)
- `NotifyMe.UI/HistoryWindow.xaml` (جديد)
- `NotifyMe.UI/HistoryWindow.xaml.cs` (جديد)
- `NotifyMe.UI/App.xaml.cs` (تحديث)

---

## 🔧 **الإصلاحات الهامة**

### **إصلاح 1: NullReferenceException عند بدء التشغيل**
**التاريخ**: 23 نوفمبر 2025
**المشكلة**: التطبيق يتعطل فوراً عند التشغيل
**السبب**: محاولة الوصول إلى `_timer` قبل تهيئته في `ApplySettings`
**الحل**: إضافة فحص null قبل تحديث `_timer.Interval`

### **إصلاح 2: Ping IP لا يتم تطبيقه**
**التاريخ**: 23 نوفمبر 2025
**المشكلة**: تغيير Ping IP في الإعدادات لا يؤثر
**الحل**: تحديث ترتيب حفظ الإعدادات وتطبيق `PingHost` فوراً

### **إصلاح 3: عدم ظهور TIMEOUT عند فشل Ping**
**التاريخ**: 23 نوفمبر 2025
**المشكلة**: عدم وجود إشارة بصرية عند فشل Ping
**الحل**: إرسال قيمة -1 عند الفشل وعرض "TIMEOUT" باللون الأحمر

---

## 🎨 **التصميم والواجهة**

### **نمط التصميم المعتمد:**
- **Dark Theme**: خلفيات داكنة (#1E1E1E, #2D2D2D)
- **Accent Color**: أزرق سماوي (#4CC2FF)
- **Typography**: Segoe UI Variable Display
- **تأثيرات**: DropShadow, Glassmorphism, Hover animations

### **الألوان المستخدمة:**
- 🔴 Red (#FF4C4C): قطع الاتصال / Ping عالي
- 🟡 Yellow (#FFD700): Ping متوسط
- 🟢 Green (#4CFF4C): Ping منخفض / اتصال جيد
- 🔵 Blue (#4CC2FF): اتصال نشط
- ⚪ Gray (#808080): اتصال خامل

---

## 📦 **التقنيات والمكتبات**

### **Framework:**
- .NET 10.0
- WPF (Windows Presentation Foundation)
- Target: `net10.0-windows10.0.26100.0`

### **NuGet Packages:**
- `Hardcodet.Wpf.TaskbarNotification` - System Tray Icon
- `Microsoft.Data.Sqlite` - قاعدة البيانات
- `System.Drawing.Common` - معالجة الصور

### **خدمات مخصصة:**
- `NetworkMonitor` - مراقبة حالة الاتصال والـ Ping
- `TrafficMonitor` - قياس استهلاك الشبكة
- `SettingsService` - إدارة الإعدادات (JSON)
- `DataLogger` - تسجيل البيانات (SQLite)
- `NotificationService` - الإشعارات

---

## 📁 **هيكل المشروع**

```
NotifyMe/
├── NotifyMe.Models/          # نماذج البيانات
│   ├── UserSettings.cs
│   ├── NetworkStats.cs
│   ├── NetworkLog.cs
│   └── ...
├── NotifyMe.Core/            # المنطق الأساسي
│   └── Services/
│       ├── NetworkMonitor.cs
│       ├── TrafficMonitor.cs
│       ├── SettingsService.cs
│       ├── DataLogger.cs
│       └── NotificationService.cs
├── NotifyMe.UI/              # واجهة المستخدم
│   ├── App.xaml/cs
│   ├── MainWindow.xaml/cs
│   ├── FloatingIconWindow.xaml/cs
│   ├── SettingsWindow.xaml/cs
│   ├── HistoryWindow.xaml/cs
│   └── Resources/
└── NotifyMe.Tests/           # الاختبارات
```

---

### **المرحلة 5: الإشعارات الصوتية** ✅ (مكتمل 100%)
**التاريخ**: 23 نوفمبر 2025

#### الميزات المنفذة:

##### **خدمة الصوت (SoundService):**
- ✅ تشغيل صوت عند قطع الاتصال
- ✅ تشغيل صوت عند استعادة الاتصال
- ✅ التحقق من إعداد `EnableSoundNotifications` قبل التشغيل
- ✅ معالجة الأخطاء لضمان عدم تعطل التطبيق

##### **ملفات الصوت:**
- ✅ تحميل `disconnect.wav` من Orange Free Sounds
- ✅ تحميل `reconnect.wav` من Orange Free Sounds
- ✅ موقع الملفات: `Resources/Sounds/`
- ✅ استخدام `System.Media.SoundPlayer` للتشغيل

##### **التكامل:**
- ✅ دمج SoundService في `App.xaml.cs`
- ✅ ربط مع حدث `ConnectionStateChanged`
- ✅ تطبيق إعداد الصوت من Settings
- ✅ إضافة مكتبة `System.Windows.Extensions`

#### الملفات المتأثرة:
- `NotifyMe.Core/Services/SoundService.cs` (جديد)
- `NotifyMe.UI/Resources/Sounds/disconnect.wav` (جديد)
- `NotifyMe.UI/Resources/Sounds/reconnect.wav` (جديد)
- `NotifyMe.UI/App.xaml.cs` (تحديث)
- `NotifyMe.Core/NotifyMe.Core.csproj` (تحديث)

---

### **المرحلة 6: تحكم في الإشعارات** ✅ (مكتمل 100%)
**التاريخ**: 24 نوفمبر 2025

#### الميزات المنفذة:

##### **إدارة الإشعارات:**
- ✅ إضافة `EnableToastNotifications` في UserSettings
- ✅ إضافة `EnableSoundNotifications` في UserSettings
- ✅ تحديث NotificationService لاستخدام SettingsService
- ✅ التحكم الكامل من Settings Window

##### **واجهة الإعدادات:**
- ✅ CheckBox لتفعيل/إيقاف Toast Notifications
- ✅ CheckBox لتفعيل/إيقاف Sound Notifications
- ✅ نصوص توضيحية لكل خيار
- ✅ حفظ الإعدادات في appsettings.json

#### السيناريوهات المدعومة:
- ✅ إشعارات + أصوات معاً
- ✅ إشعارات فقط (بدون أصوات)
- ✅ أصوات فقط (بدون إشعارات)
- ✅ تعطيل كامل

#### الملفات المتأثرة:
- `NotifyMe.Models/UserSettings.cs` (تحديث)
- `NotifyMe.UI/Services/NotificationService.cs` (تحديث)
- `NotifyMe.UI/SettingsWindow.xaml` (تحديث)
- `NotifyMe.UI/SettingsWindow.xaml.cs` (تحديث)
- `NotifyMe.UI/App.xaml.cs` (تحديث)

---

### **المرحلة 7: إعادة تصميم نافذة الإعدادات** ✅ (مكتمل 100%)
**التاريخ**: 24 نوفمبر 2025

#### الميزات المنفذة:

##### **التصميم بالتبويبات (TabControl):**
- ✅ 4 تبويبات منظمة:
  - 🎨 **Appearance**: شفافية وثيم
  - 🌐 **Network**: Ping IP مع Quick Select
  - 🔔 **Notifications**: Toast و Sound
  - ⚙️ **Advanced**: معدل التحديث وعتبة الحركة

##### **التحسينات البصرية:**
- ✅ Custom TabItem Style مع Hover Effects
- ✅ Custom CheckBox Style مع ✓ Mark
- ✅ أزرار Quick Select لـ DNS (Google, Cloudflare, OpenDNS)
- ✅ Tip Box في تبويب الإشعارات
- ✅ Footer منفصل مع زر Save Changes
- ✅ أيقونات في كل تبويب
- ✅ نصوص توضيحية شاملة

##### **المواصفات:**
- ✅ الحجم: 700x600 بكسل
- ✅ Border Radius: 12px
- ✅ Drop Shadow: Blur 30px
- ✅ ScrollViewer لكل تبويب

#### الوظائف الجديدة:
- ✅ `SetPingHost_Click` لاختيار DNS بسرعة
- ✅ تنظيم أفضل للإعدادات
- ✅ وصول أسرع للإعداد المطلوب

#### الملفات المتأثرة:
- `NotifyMe.UI/SettingsWindow.xaml` (إعادة تصميم كاملة)
- `NotifyMe.UI/SettingsWindow.xaml.cs` (إضافة SetPingHost_Click)

---

### **المرحلة 8: البدء التلقائي مع Windows** ✅ (مكتمل 100%)
**التاريخ**: 24 نوفمبر 2025

#### الميزات المنفذة:

##### **AutoStartService:**
- ✅ إدارة Windows Registry (HKEY_CURRENT_USER)
- ✅ `IsEnabled()` - التحقق من حالة Auto-Start
- ✅ `Enable()` - تفعيل البدء التلقائي
- ✅ `Disable()` - تعطيل البدء التلقائي
- ✅ معالجة أخطاء شاملة

##### **واجهة المستخدم:**
- ✅ CheckBox في تبويب Advanced: "🚀 Start with Windows"
- ✅ نص توضيحي: "Automatically launch NotifyMe when Windows starts"
- ✅ تصميم متناسق مع بقية الإعدادات

##### **المزامنة التلقائية:**
- ✅ قراءة حالة Registry عند بدء التطبيق
- ✅ مزامنة Settings مع الحالة الفعلية
- ✅ تحديث appsettings.json تلقائياً

##### **معالجة الأخطاء:**
- ✅ رسائل واضحة عند الفشل
- ✅ Revert التغييرات عند حدوث خطأ
- ✅ Debug logging لتتبع المشاكل

#### التقنية المستخدمة:
- **Registry**: `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
- **لا يحتاج Admin Rights**: استخدام HKCU بدلاً من HKLM
- **القيمة**: مسار كامل للـ exe محاط بعلامات اقتباس

#### الملفات المتأثرة:
- `NotifyMe.Core/Services/AutoStartService.cs` (جديد)
- `NotifyMe.Models/UserSettings.cs` (تحديث)
- `NotifyMe.UI/SettingsWindow.xaml` (تحديث)
- `NotifyMe.UI/SettingsWindow.xaml.cs` (تحديث)
- `NotifyMe.UI/App.xaml.cs` (تحديث)

---

### **المرحلة 9: نظام الإشعارات المخصص** ✅ (مكتمل 100%)
**التاريخ**: 24 نوفمبر 2025

#### الميزات المنفذة:

##### **Custom Notification Window:**
- ✅ نافذة WPF مخصصة بتصميم داكن (Dark Theme)
- ✅ دعم 4 مواقع: أعلى/أسفل يمين/يسار
- ✅ تأثيرات Slide In/Out
- ✅ إغلاق تلقائي بعد 5 ثواني
- ✅ أيقونة ونص واضح للحالة

##### **تكامل الإعدادات:**
- ✅ خيار للتبديل بين Windows Toast و Custom Notifications
- ✅ خيار لتحديد موقع الإشعار (يظهر فقط عند اختيار Custom)
- ✅ حفظ التفضيلات في UserSettings

##### **السيناريوهات المدعومة:**
- ✅ قطع الاتصال (أيقونة حمراء ❌)
- ✅ استعادة الاتصال (أيقونة خضراء ✅)
- ✅ تنبيه استهلاك عالي (أيقونة صفراء ⚠️)

#### الملفات المتأثرة:
- `NotifyMe.UI/Windows/CustomNotificationWindow.xaml` (جديد)
- `NotifyMe.UI/Windows/CustomNotificationWindow.xaml.cs` (جديد)
- `NotifyMe.UI/SettingsWindow.xaml` (تحديث)
- `NotifyMe.UI/SettingsWindow.xaml.cs` (تحديث)
- `NotifyMe.UI/Services/NotificationService.cs` (تحديث)
- `NotifyMe.Models/UserSettings.cs` (تحديث)

---

### **المرحلة 10: التحسينات الأساسية (Phase 1 Core Enhancement)** ✅ (مكتمل 100%)
**التاريخ**: 1 ديسمبر 2025

#### الميزات المنفذة:

##### **1. لوحة التحكم الرئيسية (Main Dashboard):**
- ✅ تصميم Material Design 5.x مع Dark Theme
- ✅ 4 بطاقات إحصائيات Real-time:
  - قوة الإشارة (Signal Strength)
  - سرعة التحميل (Download Speed)
  - سرعة الرفع (Upload Speed)
  - زمن الاستجابة (Latency)
- ✅ رسم بياني Live Charts للبيانات التاريخية
- ✅ قائمة جانبية للتنقل (Navigation Sidebar)
- ✅ تحديث تلقائي كل ثانية

##### **2. تتبع التطبيقات (Per-Application Tracking):**
- ✅ رصد فعلي للتطبيقات باستخدام P/Invoke و GetExtendedTcpTable
- ✅ عرض الاتصالات الشبكية لكل تطبيق
- ✅ تصنيف تلقائي (Browser, Gaming, Communication, System, etc.)
- ✅ DataGrid مع:
  - اسم التطبيق مع أيقونة
  - عدد الاتصالات
  - البيانات المرسلة / المستقبلة
  - آخر نشاط
  - التصنيف
- ✅ بحث وفرز متقدم (حسب الاسم، الاستهلاك، الاتصالات، النشاط)
- ✅ تحديث تلقائي كل 3 ثوان

##### **3. نافذة التحليلات (Analytics Dashboard):**
- ✅ واجهة Material Design شاملة
- ✅ 4 بطاقات إحصائيات summary:
  - Total Usage
  - Average Speed (↓ Download / ↑ Upload)
  - Peak Speed
  - Active Time
- ✅ 3 رسوم بيانية متقدمة:
  - **Usage Over Time**: Line Chart للاستهلاك عبر الزمن
  - **Upload vs Download**: Pie Chart للمقارنة
  - **Peak Hours Analysis**: Column Chart لأوقات الذروة
- ✅ فلترة حسب الفترة الزمنية:
  - Today
  - This Week
  - This Month
  - Custom Range
- ✅ حسابات ذكية من قاعدة البيانات

##### **4. تحسين نافذة التاريخ (Enhanced History Window):**
- ✅ إعادة تصميم كاملة بـ Material Design
- ✅ 4 بطاقات إحصائيات:
  - Total Records
  - Average Download Speed
  - Average Upload Speed
  - Average Ping
- ✅ DataGrid متقدم مع:
  - ألوان مخصصة للحالات
  - تنسيق احترافي للبيانات
  - Alternating row colors
- ✅ فلترة زمنية محسّنة:
  - Last Hour
  - Last 24 Hours
  - Last 7 Days
  - Last 30 Days
  - All Time
- ✅ تصدير إلى CSV ✨ جديد
- ✅ حذف جميع البيانات (Clear All History) ✨ جديد
- ✅ حذف السجلات القديمة (+30 يوم)
- ✅ تأكيد مزدوج للحماية

##### **5. تحسينات عامة:**
- ✅ توحيد الثيم عبر جميع النوافذ
- ✅ إصلاح null reference errors
- ✅ تحسين Async/Await patterns
- ✅ معالجة أخطاء شاملة

#### التقنيات الجديدة المستخدمة:
- **LiveChartsCore 2.0** - رسوم بيانية متقدمة
- **Material Design XAML 5.x** - تصميم Material Design
- **P/Invoke** (iphlpapi.dll) - GetExtendedTcpTable للـ Process Tracking
- **Async/Await** - برمجة غير متزامنة
- **ObservableCollection** - تحديث UI تلقائي

#### الملفات الجديدة:
- `NotifyMe.UI/MainWindow.xaml.cs` (تحديث كامل)
- `NotifyMe.UI/ApplicationsWindow.xaml` (جديد)
- `NotifyMe.UI/ApplicationsWindow.xaml.cs` (جديد)
- `NotifyMe.UI/AnalyticsWindow.xaml` (جديد)
- `NotifyMe.UI/AnalyticsWindow.xaml.cs` (جديد)
- `NotifyMe.Core/Services/ProcessMonitorService.cs` (جديد)

#### الملفات المحدثة:
- `NotifyMe.UI/App.xaml` (Dark Theme)
- `NotifyMe.UI/App.xaml.cs` (دعم النوافذ الجديدة)
- `NotifyMe.UI/MainWindow.xaml` (تصميم جديد كامل)
- `NotifyMe.UI/HistoryWindow.xaml` (Material Design)
- `NotifyMe.UI/HistoryWindow.xaml.cs` (CSV Export + Clear All)
- `NotifyMe.UI/FloatingIconWindow.xaml` (تحسينات الرسم البياني)

#### الإحصائيات:
- **عدد النوافذ الجديدة**: 2 (Applications, Analytics)
- **عدد الخدمات الجديدة**: 1 (ProcessMonitorService)
- **عدد الميزات**: 5 ميزات رئيسية
- **عدد الملفات**: 12+ ملف تم إنشاءه/تحديثه
- **الوقت المستغرق**: يوم واحد
- **عدد الأسطر**: 2000+ سطر جديد

---

## 🚀 **المراحل القادمة (من plan v1.md)

### **المرحلة 9: Do Not Disturb** ⏳
- [ ] إضافة جدول زمني في Settings
- [ ] تعطيل الإشعارات في أوقات محددة

### **المرحلة 7: تعدد اللغات** ⏳
- [ ] ملفات `.resx` للعربية
- [ ] ملفات `.resx` للإنجليزية
- [ ] آلية تبديل اللغة

### **المرحلة 8: تحسين الأداء** ⏳
- [ ] تحسين استهلاك الذاكرة
- [ ] تحسين استهلاك CPU
- [ ] استخدام `IHostedService` (اختياري)

---

## 📊 **الإحصائيات**

- **عدد الملفات المنشأة**: 30+
- **عدد الملفات المعدلة**: 50+
- **عدد الميزات المنفذة**: 75+
- **نسبة الإنجاز من Phase 1**: 100% ✅
- **عدد الإصلاحات الحرجة**: 5
- **عدد النوافذ**: 7 (Main, Floating, Settings, History, Applications, Analytics, Custom Notifications)
- **عدد الخدمات**: 8 (NetworkMonitor, TrafficMonitor, DataLogger, SettingsService, NotificationService, SoundService, AutoStartService, ProcessMonitorService)

---

## 🔗 **روابط مهمة**

- [plan v1.md](file:///d:/Apps/C#/NofiyMe/plan%20v1.md) - الخطة الأصلية
- [phase1_core_enhancement.md](file:///d:/Apps/C#/NofiyMe/phase1_core_enhancement.md) - خطة المرحلة الأولى
- [COMMANDS.md](file:///d:/Apps/C#/NofiyMe/COMMANDS.md) - تعليمات العمل

---

## 📝 **ملاحظات**

1. **الأداء**: التطبيق يعمل بسلاسة مع استهلاك منخفض للموارد
2. **الاستقرار**: تم إصلاح جميع الأخطاء الحرجة
3. **التوافق**: يعمل على Windows 10/11 مع .NET 8
4. **قاعدة البيانات**: تنظيف تلقائي للسجلات القديمة (+30 يوم)
5. **الإشعارات**: تحكم كامل في Toast و Sound و Custom Notifications
6. **واجهة الإعدادات**: تصميم جديد بالتبويبات أكثر تنظيماً
7. **Auto-Start**: بدء تلقائي مع Windows بدون Admin rights
8. **Material Design**: جميع النوافذ الآن تستخدم Material Design 5.x
9. **Process Tracking**: تتبع فعلي للتطبيقات باستخدام P/Invoke
10. **Analytics**: تحليلات متقدمة مع 3 رسوم بيانية

---

**آخر تحديث**: 1 ديسمبر 2025، 10:40
