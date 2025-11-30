# دليل استخدام بوت التليجرام 📱

## 🚀 البدء السريع

### 1. تثبيت المتطلبات
```bash
pip install -r requirements.txt
```

أو مباشرة:
```bash
pip install requests
```

### 2. اختبار البوت
```bash
python telegram_notifier.py test
```

## 📋 الاستخدام

### إرسال إشعار بإكمال مهمة
```bash
python telegram_notifier.py task "اسم المهمة"
```

مثال:
```bash
python telegram_notifier.py task "إصلاح تصميم نافذة الإعدادات"
```

مع تفاصيل:
```bash
python telegram_notifier.py task "إصلاح نافذة الإعدادات" "تم إعادة تصميم جميع التبويبات"
```

### إرسال إشعار بإكمال مرحلة
```bash
python telegram_notifier.py phase "اسم المرحلة" عدد_المهام
```

مثال:
```bash
python telegram_notifier.py phase "المرحلة الأولى - التحسينات الأساسية" 8
```

### إرسال رسالة مخصصة
```bash
python telegram_notifier.py custom "العنوان" "نص الرسالة"
```

مثال:
```bash
python telegram_notifier.py custom "تحديث مهم" "تم إضافة ميزة الإشعارات الذكية"
```

## ⚙️ الإعدادات

### إضافة مستخدمين جدد

افتح ملف `telegram_notifier.py` وعدّل القائمة `chat_ids`:

```python
self.chat_ids = [
    "563390643",    # المستخدم الأول
    "123456789",    # مستخدم جديد
    "987654321",    # مستخدم آخر
]
```

### معلومات البوت
- **توكن البوت:** 8319172203:AAE99ScoWSG7PqYXzvdMiShncd-9nR-fKgM
- **المستخدمون الحاليون:** 563390643

## 💡 أمثلة عملية

### مثال 1: إشعار بإكمال تبويب
```bash
python telegram_notifier.py task "تصميم تبويب Appearance" "تم استخدام Grid بدلاً من StackPanel"
```

### مثال 2: إشعار بإكمال مجموعة تبويبات
```bash
python telegram_notifier.py phase "تحديث نافذة الإعدادات" 4
```

### مثال 3: إشعار بإصلاح bug
```bash
python telegram_notifier.py custom "🐛 Bug Fix" "تم إصلاح مشكلة التداخل في التصميم"
```

### مثال 4: إشعار ببداية عمل
```bash
python telegram_notifier.py custom "🚀 بدء العمل" "بدأت العمل على المرحلة الثانية"
```

## 🔧 استخدام من كود Python

يمكنك أيضاً استخدام البوت مباشرة من كود Python:

```python
from telegram_notifier import TelegramNotifier

# إنشاء كائن البوت
notifier = TelegramNotifier()

# إرسال إشعار بمهمة
notifier.send_task_completed(
    task_name="إصلاح التصميم",
    details="تم تحسين جميع المسافات والهوامش"
)

# إرسال إشعار بمرحلة
notifier.send_phase_completed(
    phase_name="المرحلة الأولى",
    tasks_completed=10
)

# إرسال رسالة مخصصة
notifier.send_custom_message(
    title="تحديث مهم",
    message_text="تم الانتهاء من جميع التحديثات المطلوبة"
)

# إضافة مستخدم جديد
notifier.add_chat_id("123456789")
```

## 📝 ملاحظات

1. تأكد من اتصالك بالإنترنت قبل إرسال الإشعارات
2. يمكن إضافة أي عدد من المستخدمين
3. الرسائل تدعم تنسيق HTML
4. يتم إرسال الإشعار لجميع المستخدمين في القائمة

## 🎨 تخصيص الرسائل

يمكنك تعديل قوالب الرسائل في ملف `telegram_notifier.py` حسب تفضيلاتك.

---

**استمتع بالإشعارات الفورية! 🎉**
