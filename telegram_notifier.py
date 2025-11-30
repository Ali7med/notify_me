#!/usr/bin/env python3
"""
Telegram Notification Bot for NotifyMe Updates
Simple script to send notifications to Telegram when tasks are completed
"""

import requests
import sys
from datetime import datetime

class TelegramNotifier:
    def __init__(self):
        self.bot_token = "8319172203:AAE99ScoWSG7PqYXzvdMiShncd-9nR-fKgM"
        self.base_url = f"https://api.telegram.org/bot{self.bot_token}"
        
        # قائمة المستخدمين الذين سيتلقون الإشعارات
        self.chat_ids = [
            "563390643",  # يمكن إضافة المزيد هنا
        ]
    
    def send_message(self, message, parse_mode="HTML"):
        """إرسال رسالة إلى جميع المستخدمين المحددين"""
        results = []
        
        for chat_id in self.chat_ids:
            url = f"{self.base_url}/sendMessage"
            payload = {
                "chat_id": chat_id,
                "text": message,
                "parse_mode": parse_mode
            }
            
            try:
                response = requests.post(url, json=payload, timeout=10)
                if response.status_code == 200:
                    results.append((chat_id, True, "Success"))
                    print(f"✅ تم إرسال الرسالة إلى {chat_id}")
                else:
                    results.append((chat_id, False, response.text))
                    print(f"❌ فشل إرسال الرسالة إلى {chat_id}: {response.text}")
            except Exception as e:
                results.append((chat_id, False, str(e)))
                print(f"❌ خطأ في إرسال الرسالة إلى {chat_id}: {str(e)}")
        
        return results
    
    def send_task_completed(self, task_name, details=""):
        """إرسال إشعار بإكمال مهمة"""
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        
        message = f"""
🎉 <b>مهمة مكتملة - NotifyMe</b>

📋 <b>المهمة:</b> {task_name}

⏰ <b>الوقت:</b> {timestamp}
"""
        
        if details:
            message += f"\n📝 <b>التفاصيل:</b>\n{details}"
        
        message += "\n\n✨ <i>تم الإكمال بنجاح!</i>"
        
        return self.send_message(message)
    
    def send_phase_completed(self, phase_name, tasks_completed):
        """إرسال إشعار بإكمال مرحلة كاملة"""
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        
        message = f"""
🏆 <b>مرحلة مكتملة - NotifyMe</b>

🎯 <b>المرحلة:</b> {phase_name}

✅ <b>المهام المكتملة:</b> {tasks_completed}

⏰ <b>الوقت:</b> {timestamp}

🎊 <i>مبروك! تم إكمال المرحلة بنجاح</i>
"""
        
        return self.send_message(message)
    
    def send_custom_message(self, title, message_text):
        """إرسال رسالة مخصصة"""
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        
        message = f"""
📢 <b>{title}</b>

{message_text}

⏰ {timestamp}
"""
        
        return self.send_message(message)
    
    def add_chat_id(self, new_chat_id):
        """إضافة رقم مستخدم جديد"""
        if new_chat_id not in self.chat_ids:
            self.chat_ids.append(new_chat_id)
            print(f"✅ تمت إضافة الرقم: {new_chat_id}")
        else:
            print(f"ℹ️ الرقم {new_chat_id} موجود بالفعل")


def main():
    """استخدام البوت من سطر الأوامر"""
    notifier = TelegramNotifier()
    
    if len(sys.argv) < 2:
        print("""
استخدام البوت:
    
    # إرسال إشعار بإكمال مهمة
    python telegram_notifier.py task "اسم المهمة" ["تفاصيل اختيارية"]
    
    # إرسال إشعار بإكمال مرحلة
    python telegram_notifier.py phase "اسم المرحلة" عدد_المهام
    
    # إرسال رسالة مخصصة
    python telegram_notifier.py custom "العنوان" "الرسالة"
    
    # اختبار البوت
    python telegram_notifier.py test

أمثلة:
    python telegram_notifier.py task "إصلاح نافذة الإعدادات"
    python telegram_notifier.py phase "المرحلة الأولى" 5
    python telegram_notifier.py custom "تحديث" "تم إضافة ميزة جديدة"
    python telegram_notifier.py test
        """)
        return
    
    command = sys.argv[1].lower()
    
    if command == "task":
        if len(sys.argv) < 3:
            print("❌ يجب تحديد اسم المهمة")
            return
        task_name = sys.argv[2]
        details = sys.argv[3] if len(sys.argv) > 3 else ""
        notifier.send_task_completed(task_name, details)
    
    elif command == "phase":
        if len(sys.argv) < 4:
            print("❌ يجب تحديد اسم المرحلة وعدد المهام")
            return
        phase_name = sys.argv[2]
        tasks_count = sys.argv[3]
        notifier.send_phase_completed(phase_name, tasks_count)
    
    elif command == "custom":
        if len(sys.argv) < 4:
            print("❌ يجب تحديد العنوان والرسالة")
            return
        title = sys.argv[2]
        message = sys.argv[3]
        notifier.send_custom_message(title, message)
    
    elif command == "test":
        print("🧪 اختبار البوت...")
        notifier.send_custom_message(
            "🧪 اختبار البوت",
            "هذه رسالة اختبارية للتأكد من عمل البوت بشكل صحيح ✅"
        )
    
    else:
        print(f"❌ أمر غير معروف: {command}")


if __name__ == "__main__":
    main()
