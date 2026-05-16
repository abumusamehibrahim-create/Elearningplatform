# منصة تعليمية - ASP.NET Core 8

## خطوات التشغيل على Visual Studio 2022

### 1. المتطلبات
- Visual Studio 2022 (أي إصدار)
- .NET 8 SDK
- SQL Server أو LocalDB (مدمج مع VS)

### 2. فتح المشروع
1. افتح VS2022
2. File > Open > Project/Solution
3. اختر ملف `ELearningPlatform.csproj`

### 3. إعداد قاعدة البيانات
افتح **Package Manager Console** (Tools > NuGet > Package Manager Console) وشغّل:
```
Add-Migration InitialCreate
Update-Database
```

### 4. تشغيل المشروع
اضغط **F5** أو زر التشغيل الأخضر.

---

## بيانات الدخول الافتراضية
| المستخدم | كلمة المرور | الدور |
|---------|------------|-------|
| admin | Admin@123456 | مدير |

---

## إعداد Stripe (للدفع الحقيقي)
1. سجّل في https://stripe.com
2. من لوحة التحكم، انسخ **Publishable Key** و **Secret Key**
3. ضعهما في `appsettings.json`:
```json
"Stripe": {
  "PublishableKey": "pk_test_...",
  "SecretKey": "sk_test_..."
}
```
4. للاختبار بدون Stripe (وضع التجربة): اترك القيم كما هي وسيعمل المشروع بدون دفع حقيقي.

### بطاقة الاختبار
- رقم البطاقة: `4242 4242 4242 4242`
- تاريخ الانتهاء: أي تاريخ مستقبلي (مثل 12/34)
- CVV: أي 3 أرقام (مثل 123)

---

## إعداد البريد الإلكتروني (Gmail)
1. فعّل **2-Step Verification** في حساب Google
2. اذهب إلى: Google Account > Security > App Passwords
3. أنشئ App Password وضعه في `appsettings.json`:
```json
"Email": {
  "Host": "smtp.gmail.com",
  "Port": "587",
  "User": "your@gmail.com",
  "Password": "xxxx-xxxx-xxxx-xxxx"
}
```

---

## هيكل المشروع
```
ELearningPlatform/
├── Controllers/
│   ├── HomeController.cs       - الصفحة الرئيسية وتفاصيل الكورس
│   ├── AccountController.cs    - تسجيل الدخول والخروج
│   ├── PaymentController.cs    - الدفع عبر Stripe + إنشاء الحساب
│   ├── VideoController.cs      - عرض وبث الفيديوهات (محمي)
│   └── AdminController.cs      - لوحة تحكم المدير
├── Models/
│   └── Models.cs               - ApplicationUser, Course, Video, Payment
├── Data/
│   └── ApplicationDbContext.cs - EF Core DbContext
├── Services/
│   ├── EmailService.cs         - إرسال البريد الإلكتروني
│   └── VideoAccessService.cs   - التحقق من صلاحية الوصول للفيديو
├── Views/
│   ├── Home/                   - الرئيسية + تفاصيل الكورس
│   ├── Account/                - تسجيل الدخول + الملف الشخصي
│   ├── Payment/                - صفحة الدفع + صفحة النجاح
│   ├── Video/                  - قائمة الفيديوهات + مشغل الفيديو
│   ├── Admin/                  - لوحة التحكم الكاملة
│   └── Shared/                 - _Layout + _AdminLayout
├── ProtectedVideos/            - مجلد الفيديوهات (خارج wwwroot)
├── Program.cs                  - إعدادات التطبيق + Seed البيانات
└── appsettings.json            - الإعدادات
```

---

## مسار عمل المنصة
1. الطالب يتصفح الكورسات من الصفحة الرئيسية
2. ينقر "اشترك الآن" ويُوجَّه لصفحة الدفع
3. يدفع عبر Stripe (أو وضع الاختبار)
4. يتم إنشاء حساب Username + Password تلقائياً
5. تُرسَل البيانات للبريد الإلكتروني + تظهر على الشاشة
6. الطالب يسجل دخوله ويشاهد الفيديوهات
7. الفيديوهات تُبثّ بأمان عبر ASP.NET (لا يمكن تحميلها)
