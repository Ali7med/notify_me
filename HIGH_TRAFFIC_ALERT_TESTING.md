# دليل اختبار High Traffic Alert

## الهدف
اختبار ميزة التنبيه عند تجاوز حد الاستهلاك العالي.

---

## الخطوات

### 1. تفعيل Debug Output
قبل الاختبار، شغل التطبيق من Visual Studio أو استخدم:
```bash
dotnet run --project NotifyMe.UI/NotifyMe.UI.csproj
```

افتح **Output Window** في Visual Studio (View → Output) لمشاهدة Debug messages.

---

### 2. ضبط الإعدادات

1. **فتح Settings** → Advanced Tab
2. **High Traffic Alert Threshold**:
   - **القيمة**: ضع رقم صغير للاختبار (مثل: `0.1` أو `0.5`)
   - **الوحدة**: اختر من ComboBox:
     - `KB/s` - للاختبار السريع
     - `MB/s` - الافتراضي
     - `GB/s` - للسرعات العالية جداً
3. **Save Changes**

---

### 3. توليد Traffic
لتجاوز الحد، جرب أحد الطرق التالية:

#### الطريقة 1: تحميل ملف كبير
1. افتح المتصفح
2. حمّل ملف كبير (فيديو، ملف ISO، إلخ)
3. راقب Widget → Download speed

#### الطريقة 2: Speed Test
1. افتح [speedtest.net](https://speedtest.net)
2. اضغط "Go"
3. راقب سرعة التحميل

#### الطريقة 3: YouTube/Netflix
1. افتح فيديو بجودة 4K
2. راقب Traffic

---

### 4. مراقبة Debug Output

في Output Window، ابحث عن:

```
Traffic Check - Speed: [X.XX] MB/s, Threshold: [Y.YY] MB/s (Z.Z KB)
```

**إذا تجاوز الحد:**
```
THRESHOLD EXCEEDED! Time since last alert: [XXX.X] minutes
Showing high traffic alert...
ShowHighTrafficAlert called: Speed=X.XX MB/s, Unit=KB, Threshold=0.5
Displaying toast: X.XX KB/s
Toast shown successfully
```

**إذا كان Throttled (تم الإشعار مؤخراً):**
```
Skipping alert (throttled) - wait X.X more minutes
```

---

### 5. التحقق من Toast Notification

**يجب أن يظهر Toast بالمحتوى التالي:**

```
⚠️ High Traffic Alert
Traffic exceeded 0.5 KB/s
Current speed: X.XX KB/s
```

---

## المشاكل المحتملة وحلولها

### المشكلة 1: لا يظهر Toast
**الأسباب:**
1. **Toast Notifications معطلة** - Settings → Notifications → تفعيل "Show Toast Notifications"
2. **إعدادات Windows** - تحقق من Settings → System → Notifications → NotifyMe.UI.exe مفعّل
3. **Throttling** - انتظر 5 دقائق من آخر إشعار

**الحل:**
- افحص Debug Output لمعرفة السبب بالضبط
- إذا كان "Toast notifications are disabled" → فعّل من Settings
- إذا كان "Skipping alert (throttled)" → انتظر 5 دقائق

---

### المشكلة 2: Debug Output لا يظهر شيء
**السبب:** السرعة لم تتجاوز الحد

**التحقق:**
```
Traffic Check - Speed: 0.50 MB/s, Threshold: 5.00 MB/s (5.0 MB)
```
- إذا `Speed < Threshold` → لن يظهر إشعار
- **الحل**: اخفض الـ Threshold أو زد السرعة

---

### المشكلة 3: الحساب خطأ
**مثال:**
- Settings: `20 KB/s`
- Actual speed: `0.02 MB/s` (= 20 KB/s)
- Debug: `Threshold: 0.02 MB/s`

**التحقق:**
- 1 MB = 1024 KB
- 1 GB = 1024 MB
- `20 KB/s = 20/1024 MB/s = 0.0195 MB/s`

---

## اختبار شامل

### Test Case 1: KB/s
1. Settings: `0.5 KB/s`
2. حمّل أي شيء (حتى فتح موقع)
3. **توقع**: إشعار فوراً

### Test Case 2: MB/s
1. Settings: `1.0 MB/s`
2. حمّل ملف متوسط
3. **توقع**: إشعار عند `>1.0 MB/s`

### Test Case 3: GB/s
1. Settings: `0.001 GB/s` (= 1 MB/s)
2. حمّل ملف كبير
3. **توقع**: إشعار عند `>1.0 MB/s`

### Test Case 4: Throttling
1. أظهر إشعار مرة
2. حاول مرة أخرى فوراً
3. **توقع**: "Skipping alert (throttled)"
4. انتظر 5 دقائق
5. حاول مرة أخرى
6. **توقع**: إشعار يظهر

---

## ملاحظات مهمة

### Throttling (منع الإزعاج)
- **مدة الانتظار**: 5 دقائق
- **السبب**: تجنب spam من الإشعارات المتكررة
- **يمكن تعديلها** في `App.xaml.cs` سطر:
  ```csharp
  if (timeSinceLastAlert >= 5) // غير 5 للمدة المطلوبة بالدقائق
  ```

### دقة الحساب
- الحسابات دقيقة ل `F2` (رقمين بعد الفاصلة)
- التحويلات: `1 MB = 1024 KB`
- السرعة من `BytesPerSecond` → `MB/s`: `//(1024*1024)`

### إعادة تعيين Throttling
لإعادة تعيين وإظهار الإشعار فوراً:
1. أعد تشغيل التطبيق
2. أو انتظر 5 دقائق

---

**جاهز للاختبار!** 🧪
