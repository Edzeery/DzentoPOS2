# خطة نظام التصميم (Design System Plan)

**التاريخ:** 2026-10-20
**النطاق:** توحيد نظام التصميم، إصلاح الوضع الداكن، بطاقات KPI، الحقول والقوائم والأزرار، التوافق مع أحجام الشاشات، وتحسين الأداء.

---

## تشخيص الجذر (Root Cause)

| المشكلة | السبب الجذري | الدليل |
|---|---|---|
| البطاقات "شبه صحيحة" في الوضع الداكن | `AppColors` قيم `static readonly` (Light فقط)، بينما `ThemeManager` يملك لوحة منفصلة (داكن = أزرق `0,120,212`) | `AppColors.cs`، `ThemeManager.SetMode` |
| التبديل للوضع الداكن لا يعيد تلوين النماذج المفتوحة | `SettingsForm.SaveSettings` يستدعي `SetMode` فقط، ولا شيء يعيد التطبيق على النموذج المُستضاف | `SettingsForm.cs:149`, `NavigationService.cs:90` |
| بطاقات/رسوم بيانية بيضاء في الداكن | `Color.White` مُثبّت في `KpiCard`, `UIHelper.WrapInCard`, `ChartsModule` | بحث `Color.White` (>100 نتيجة) |
| منطق ثيم معطوب | `if (!(control is Button))` داخل فرع `Button` دائماً false | `ThemeManager.cs:180` |
| تصميم غير متجاوب | إحداثيات مطلقة `Location/Size` ثابتة في الحوارات والنماذج | `ProductEditDialog`, `SettingsForm`, ... |

---

## المرحلة 1 — توحيد الثيم والوضع الداكن (الأساس)

**الهدف:** مصدر واحد للحقيقة للألوان يعمل في الوضعين الفاتح والداكن، مع إعادة تطبيق فورية عند التبديل.

1. تحويل `AppColors` من `static readonly` إلى **خصائص ديناميكية** (`public static Color X => ...`) تعتمد على `ThemeManager.IsDark`، مع الحفاظ على أسماء الأعضاء الحالية (لا كسر للمرجعيات).
2. توسيع `ThemeManager` ليملك اللوحة الكاملة الدلالية (Surfaces, Borders, Inputs, Grids, التدرجات) للوضعين، وجعل `AppColors` يقرأ منها (طبقة رقيقة فوق ThemeManager) — إزالة التعارض بين اللوحتين.
3. إضافة حدث `ThemeChanged` عام يُطلق عند `SetMode`، وإعادة تطبيق الثيم على النموذج النشط فوراً (subscription في `NavigationService` + `MainForm` shell).
4. إصلاح `ThemeManager.ApplyToControl` (المنطق المعطوب) أو استبداله بمعالج ثيم موحّد يعتمد على `AppColors`.
5. ربط `ChangeBrightness`/`Hover`/`Pressed` بالوضع الحالي.

**المعيار:** تبديل الوضع الداكن/الفاتح من الإعدادات يلوّن كل الصفحات المفتوحة فوراً بشكل صحيح.

## المرحلة 2 — إعادة تصميم البطاقات (Dashboard + Reports)

1. `KpiCard`: استبدال كل `Color.White` بـ `AppColors.Surface`، النصوص بـ `AppColors.Ink`/`InkMuted`، والحدود/الظل بالوضع الحالي.
2. `UIHelper.WrapInCard`: خلفية `AppColors.Surface` وحدود `AppColors.Border`.
3. `ChartsModule`: خلفيات مناطق الرسم والوسائل (Legends) تتبع الثيم، ألوان السلاسل من `AppColors.KpiPalette`، وإعادة الرسم عند تغيّر الثيم.
4. `InsightsModule` + `DataGridViewStyleHelper`: أنماط الشبكات تتبع الثيم (ApplyLightStyle → ApplyStyled تدرك الوضع).
5. توحيد تصميم بطاقة KPI بين Dashboard وReports (تكبير/تناسق الخط، الأيقونة، الشريط اللوني).

**المعيار:** البطاقات تبدو متطابقة وصحيحة تماماً في الوضعين.

## المرحلة 3 — مراجعة الحقول والقوائم والأزرار

1. إنشاء/توحيد أنماط الحقول: `TextBox`, `ComboBox`, `NumericUpDown`, `CheckBox` (ارتفاع موحد، حدود `AppColors.Border`، تركيز `AppColors.Primary`، خلفية `AppColors.Surface`).
2. مراجعة كل الحوارات (`ProductEditDialog`, `CustomerEditDialog`, `UserEditDialog`, `RoleEditDialog`, `ExpenseEditDialog`, `PurchaseEditDialog`, `SettingsForm`) لتطبيق الأنماط الموحدة.
3. توحيد الأزرار عبر `ButtonFactory` في كل الصفحات (بما تبقّى على `Button` مباشر).
4. توحيد الشبكات (DataGridView) عبر مطبّع واحد يدرك الثيم.
5. مراجعة المسافات والخطوط وفق مقياس `AppStyle`.

**المعيار:** كل الحقول والقوائم والأزرار متسقة بصرياً في كل الصفحات والوضعين.

## المرحلة 4 — التوافق مع أحجام الشاشات

1. تحويل الحوارات ذات الإحداثيات المطلقة إلى `TableLayoutPanel` مع `Dock`/`Anchor`.
2. `MinimumSize` لكل النماذج/الحوارات.
3. التأكد من تجاوب شبكات KPI والرسوم على العرض الضيق/الواسع.
4. دعم RTL/LTR بشكل صحيح مع التخطيط الجديد.
5. مراجعة `PerMonitorV2` مع التخطيط الجديد (لا قصّ للعناصر).

**المعيار:** لا قصّ ولا تداخل في أي حجم شاشة معقول أو تكبير DPI.

## المرحلة 5 — الأداء والتحقق

1. تقليل إعادة الرسم (SuspendLayout/ResumeLayout، تجنب إنشاء خطوط جديدة متكررة، تخزين مؤقت للفرش).
2. التأكد من أن `GetSummaryAsync` ما زال SQL-side ولا يحمّل جداول كاملة.
3. بناء كامل (`-t:Rebuild`) + تشغيل واختبار الدخان.
4. تحديث `CHANGELOG.md`.
5. تحديث المهام المكتملة.

**المعيار:** 0 أخطاء، 0 تحذيرات C#، تشغيل ناجح، تبديل الثيم فوري وسلس.

---

## الملفات الرئيسية المتأثرة

- `Themes/AppColors.cs`, `Themes/ThemeManager.cs`, `Themes/AppStyle.cs`, `Themes/ButtonFactory.cs`
- `Controls/KpiCard.cs`, `Controls/LoadingOverlay.cs`
- `Modules/Dashboard/UIHelper.cs`, `ChartsModule.cs`, `InsightsModule.cs`, `KpiCardsModule.cs`, `RecentTransactionsModule.cs`
- `Helpers/DataGridViewStyleHelper.cs`, `Helpers/FormLocalizationHelper.cs`
- `Forms/**` (كل النماذج والحوارات)
- `Navigation/NavigationService.cs`, `Forms/Layout/MainForm.cs` + Designer
- `CHANGELOG.md`

---

## حالة الإنجاز (2026-10-21)

### المرحلة 1 — توحيد الثيم والوضع الداكن ✅
1. `AppColors` أصبح خصائص ديناميكية تعتمد على `ThemeManager.IsDark` مع الحفاظ على أسماء الأعضاء.
2. `ThemeManager` يملك اللوحة الدلالية الكاملة للوضعين و`AppColors` طبقة رقيقة فوقه.
3. حدث `ThemeChanged` عام + `ApplyShellTheme()` في `MainForm` (الشريط الجانبي، الشريط العلوي، شريط الحالة، أزرار القائمة، الهيدر) + `NavigationService` يعيد تطبيق الثيم على الصفحة المُستضافة.
4. `ApplyToControl` أُصلح (يتخطى عناصر `IThemeAware` ولا يدخل في أبنائها).
5. `ChangeBrightness`/`Hover`/`Pressed` مربوطة بالوضع الحالي.
6. **إصلاح حرج**: المُنشئ الساكن لـ`ThemeManager` كان يستدعي `SetMode(Light)` فيحفظ الملف قبل أن يقرأه `LoadTheme()` (فيُمحى التفضيل المحفوظ عند كل تشغيل). صار `SetMode(mode, save)` مع `save: false` في المُنشئ الساكن.

### المرحلة 2 — البطاقات (Dashboard + Reports) ✅
- `KpiCard` / `UIHelper.WrapInCard` / `ChartsModule` تستخدم `AppColors` (لا `Color.White` ثابت).
- تصميم بطاقة KPI موحّد بين Dashboard وReports، ولوحة ألوان ديناميكية `AppColors.KpiPalette`.
- إصلاح قصّ العنوان: `AutoEllipsis` + `UpdateLayout()`، وارتفاع لوحة KPI في `ReportsForm` = 288 بصفوف 132.

### المرحلة 3 — الحقول والقوائم والأزرار ✅
- كل الأزرار عبر `ButtonFactory` (لا `new Button` مباشر في الكود).
- `AppMessageBox` أُعيدت كتابته بالكامل على `AppColors` + `ButtonTone`.
- كل حوارات التحرير بلا ألوان فاتحة ثابتة، والشبكات عبر `DataGridViewStyleHelper` الذي يدرك الثيم.

### المرحلة 4 — التوافق مع الشاشات ✅ (جزئياً: DPI/RTL يدوي)
- `PerMonitorV2` مفعّل، و`MinimumSize` على `MainForm`.
- RTL/LTR: التخطيط ينعكس فعلياً (الشريط الجانبي ينتقل لليمين في العربية) وتم التحقق منه أثناء التشغيل.

### المرحلة 5 — الأداء والتحقق ✅
- `dotnet build DzentoPOS2.sln -t:Rebuild -v:m -nologo`: **0 أخطاء**، والتحذيرات `NU1701` الباقية (15) من حزم OpenTK/SkiaSharp وليست من الكود.
- اختبار دخان: Splash → Login → MainForm بلا انهيار، واستعادة الثيم من `theme.config`.
- **تبديل ثيم حيّ**: من `SettingsForm` (وضع داكن → فاتح) يُعاد تلوين الـshell فوراً، و`theme.config` يُحدَّث (تحقّق بقراءة بكسلات `PrintWindow`: داكن `#090E16`/`#111827`، فاتح `#002731`/`#F5F7FA`).

### ملاحظات مفتوحة
- لا توجد اختبارات آلية (`DzentoPOS2.UnitTests` و`DzentoPOS2.IntegrationTests` ملفات فارغة) — التحقق يدوي.
- سلوك الدخول التلقائي في `LoginForm` (تعبئة `admin`/`Admin@123` والدخول تلقائياً بعد ~7–10 ثوانٍ) يحتاج تأكيداً من المستخدم: سلوك مقصود أم عيب.
- `AppSettings.DarkMode` في `user-settings.json` مرآة فقط؛ المصدر المعتمد عند التشغيل هو `ThemeManager`/`theme.config`.

---

## المرحلة 6 — صفحة الإعدادات والهوية الإقليمية ومسح الباركود (2026-10-21)

**الهدف:** تحويل صفحة الإعدادات إلى واجهة بطاقات متجاوبة تغطي هوية المتجر والإقليم والطباعة والباركود، وربطها فعلياً بالواجهة (الشعار، العملة، المنطقة الزمنية).

### ما تم تنفيذه

1. **صفحة الإعدادات (بطاقات):** `SettingsForm` أُعيدت كتابتها بالكامل — ست بطاقات (هوية المتجر، الإقليم، المظهر، الطباعة، الباركود، حول) + شريط سفلي (حفظ/استرجاع)، وكل الحقول قوائم `ThemedComboBox` مربوطة بالبيانات.
2. **نموذج الإعدادات:** `AppSettings` + `ICurrentSettingsService`/`CurrentSettingsService` + `IUserPreferencesService`/`UserPreferencesService` (ملف `user-settings.json` واحد للتفضيلات وإعدادات المتجر).
3. **الهوية:** `StoreBranding` (استيراد/حذف/تحميل الشعار) + اسم المتجر في الشريط الجانبي (`MainForm`) ولوحة الدخول (`LoginForm`).
4. **الإقليم:** `RegionalCatalog` (عملات/دول/مناطق زمنية) + `RegionalContext` (مصدر واحد للعملة والدولة والمنطقة الزمنية) + `FormatMoney` في `ProductsForm`/`SalesForm` + حدود اليوم/الشهر في `DashboardService` (التخزين يبقى UTC).
5. **الطباعة:** `PrinterService` (الطابعات، مقاسات الورق، الاتجاه، عرض الإيصال الحرّ، طباعة تجريبية) مع تكيّف كامل مع واجهة `System.Drawing.Printing` المختصرة في .NET 8.
6. **الباركود:** `CameraDeviceEnumerator` + `BarcodeCameraService` + `BarcodeScannerOverlay` + زر مسح في `SalesForm` (مطابقة SKU).
7. **إصلاح القائمة المنسدلة:** `EZComboBox` صار يرسم **كل** الخيارات داخل قائمة قابلة للتمرير (`ToolStripControlHost` + لوحة `AutoScroll`) بدل بترها.

### المعيار والمطابقة
- بناء نظيف: `dotnet build DzentoPOS2.sln -t:Rebuild` ⇒ **0 أخطاء** (21 تحذير `NU1701` سابقة فقط).
- فحص تشغيلي آلي (مضيف console مؤقت referencing WinForms): كل القوائم ممتلئة، والقوائم الطويلة (42 دولة / 43 منطقة زمنية / 37 عملة) تعرض 6 صفوف مع شريط تمرير فعّال، وارتفاع القائمة محدود بـ192px، واتجاه القائمة `RTL=Yes` في العربية.
- `MainForm`: اسم المتجر ظاهر في الشريط الجانبي (تم التأكد من بكسلات نص أبيض في نطاق اللوحة) والشعار مرسوم.

### ملاحظات
- لا توجد كاميرا في بيئة التطوير ⇒ يُعرض "لم يتم العثور على كاميرا"؛ مسار الالتقاط وفك الترميز لم يُختبر على عتاد حقيقي.
- لا توجد طابعة في بيئة التطوير ⇒ قوائم الطابعة/مقاس الورق تعرض "الافتراضي" فقط، ومنطق العرض الأفقي (تبديل أبعاد الورقة) لم يُختبر على ورق حقيقي.

