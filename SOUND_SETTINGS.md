# إعدادات الأصوات - Sound Settings

## تفعيل/تعطيل الأصوات

### الإعداد الافتراضي
الأصوات **مُفعَّلة** بشكل افتراضي الآن.

### ملفات الصوت
- **قطع الاتصال**: `Resources/Sounds/disconnect.wav`
- **عودة الاتصال**: `Resources/Sounds/reconnect.wav`

### كيفية تعديل الإعدادات

الإعدادات محفوظة في:
```
%LocalAppData%\NotifyMe\appsettings.json
```

لتعطيل الأصوات يدوياً، افتح الملف وغيّر:
```json
{
  "EnableSoundNotifications": false
}
```

### كيفية اختبار الأصوات

1. شغّل التطبيق
2. افصل الإنترنت (Disable network adapter)
   - يجب أن تسمع صوت `disconnect.wav`
   - سترى إشعار: "Internet connection lost"
3. أعد تشغيل الإنترنت (Enable network adapter)
   - يجب أن تسمع صوت `reconnect.wav`
   - سترى إشعار: "Internet connection restored"

### استبدال الأصوات

يمكنك استبدال الملفات الصوتية بملفات WAV خاصة بك:
1. اذهب إلى `NotifyMe.UI/Resources/Sounds/`
2. استبدل `disconnect.wav` و `reconnect.wav` بملفاتك
3. أعد بناء المشروع: `dotnet build`

## التحديثات الأخيرة

✅ تم تفعيل الأصوات بشكل افتراضي
✅ تم إضافة ملفات الصوت إلى المشروع
✅ الملفات تُنسخ تلقائياً إلى مجلد Build

---

## Sound Settings

### Default Setting
Sounds are now **enabled** by default.

### Sound Files
- **Disconnect**: `Resources/Sounds/disconnect.wav`
- **Reconnect**: `Resources/Sounds/reconnect.wav`

### How to Test Sounds

1. Run the application
2. Disconnect internet (Disable network adapter)
   - You should hear `disconnect.wav`
   - Toast notification: "Internet connection lost"
3. Reconnect internet (Enable network adapter)
   - You should hear `reconnect.wav`
   - Toast notification: "Internet connection restored"

### Customizing Sounds

Replace the WAV files in `NotifyMe.UI/Resources/Sounds/` with your own sounds and rebuild the project.
