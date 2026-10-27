# DzentoPOS2

نظام نقاط بيع (POS) لسطح مكتب Windows مبني على **.NET 10 WinForms** بطبقات Clean Architecture.

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

## المتطلبات

- **.NET 10 SDK** — يُختار الإصدار عبر [`global.json`](global.json): `10.0.403` مع `rollForward: latestFeature`.
- **.NET 10 Desktop Runtime** — مطلوب لتشغيل التطبيق على أجهزة المستخدمين النهائية (`Microsoft.WindowsDesktop.App` + `Microsoft.NETCore.App`، x64). يبقى شرطًا قائمًا حتى يُضمِّن المُثبِّت الإطار داخل حزمة التوزيع.
- للتحقق من التثبيت: `dotnet --list-sdks` و`dotnet --list-runtimes`.

## التشغيل

```bash
dotnet build DzentoPOS2.sln
cd DzentoPOS2/bin/Debug/net10.0-windows
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
- [`docs/net10-package-audit.md`](docs/net10-package-audit.md) — تدقيق حزم .NET 10 (تراخيص / ثغرات / إهمال).
- [`docs/net10-breaking-changes.md`](docs/net10-breaking-changes.md) — تغييرات .NET 10 وEF Core 10 المؤثّرة على المشروع.
- [`docs/net10-verification.md`](docs/net10-verification.md) — التحقق الكامل من البناء والاختبارات والترحيلات.
- [`docs/net10-smoke-run.md`](docs/net10-smoke-run.md) — تشغيل مسار الإقلاع على نسخ من قواعد بيانات حقيقية.
- [`Todos.md`](Todos.md) — قائمة المهام الحالية مع أدلة الإنجاز.
- [`CHANGELOG.md`](CHANGELOG.md) — سجل التغييرات.

## الاختبارات

```bash
dotnet test DzentoPOS2.sln
```

- `DzentoPOS2.UnitTests`: **84** اختبارًا.
- `DzentoPOS2.IntegrationTests`: **41** اختبارًا (قاعدة بيانات SQLite مؤقتة + كل الترحيلات + مسارات الترقية القديمة).

البناء النظيف (`dotnet build DzentoPOS2.sln -t:Rebuild`): **0 أخطاء / 21 تحذيرًا**، وكلها `NU1701` لحزم موجّهة إلى .NET Framework فقط (`AForge*` / `OpenTK*` / `SkiaSharp.Views.WindowsForms`) — نفس خط الأساس قبل الترقية.
