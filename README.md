# DzentoPOS2

نظام نقاط بيع (POS) لسطح مكتب Windows مبني على **.NET 8 WinForms** بطبقات Clean Architecture.

---

## البنية

| المشروع | الدور |
|---|---|
| `DzentoPOS2` | واجهة المستخدم (WinForms) + الثيم + الإعدادات + الماسح + الخدمات المحلية |
| `DzentoPOS2.App` | طبقة التطبيق (خدمات البيع/الزبائن/الموردين/التقارير) و`RegionalContext` |
| `DzentoPOS2.Core` | الكيانات ومنطق المجال (Sales, Inventory, Identity, Partners) |
| `DzentoPOS2.Data` | EF Core + SQLite + الترحيلات (Migrations) + المستودعات |
| `DzentoPOS2.Shared` | عقود الـDTO + `RegionalCatalog` / `RegionalContext` |
| `DzentoPOS2.Localization` | الموارد `AppStrings` (ar / en / fr) + `LanguageService` |
| `EZControls` | مكتبة عناصر واجهة مخصّصة (قوائم، حقول، رسوم) |

## التشغيل

```bash
dotnet build DzentoPOS2.sln
cd DzentoPOS2/bin/Debug/net8.0-windows
./DzentoPOS2.WinForms.exe
```

> **مهم:** مسار قاعدة البيانات أصبح ثابتًا بجوار الملف التنفيذي (`Database/DzentoPOS.db`)، بصرف النظر عن مجلد تشغيل البرنامج — فيعمل التشغيل من Visual Studio أو من `bin` أو عبر اختصار بدون اختلاف. يُنشئ المُهيّئ حساب `admin` بكلمة المرور الافتراضية `admin`، ويضمن تطابق كلمة مروره مع القيمة الافتراضية عند كل إقلاع (يُعاد الضبط تلقائيًا فقط إذا لم تعد مطابقة).

## البناء

```bash
dotnet build DzentoPOS2.sln -t:Rebuild -v:m -nologo   # 0 أخطاء
```

## الإعدادات

تُحفظ في `user-settings.json` بجوار الملف التنفيذي: اللغة، المظهر، اسم المتجر، الشعار، العملة/الدولة/المنطقة الزمنية، الطابعة، عرض الإيصال، الكاميرا.
كلمات المرور المُحفظة (اختياري) تُخزَّن في **Windows Credential Manager** ولا تُكتب في ملف الإعدادات.

## التوثيق

- [`DESIGN_SYSTEM.md`](DESIGN_SYSTEM.md) — قواعد التصميم والهوية البصرية (Apple Design Restraint).
- [`docs/design-system-plan.md`](docs/design-system-plan.md) — خطة نظام التصميم وحالة التنفيذ.
- [`Todos.md`](Todos.md) — قائمة المهام الحالية مع أدلة الإنجاز.
- [`CHANGELOG.md`](CHANGELOG.md) — سجل التغييرات.

## الاختبارات

لا توجد اختبارات آلية بعد (مجلدا الاختبارات فارغان). التحقق يتم عبر البناء النظيف + مِضيف فحص آلي يقيس الواجهة عند 375 / 768 / 1440 بكسل.
