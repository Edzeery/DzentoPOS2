# قائمة المهام — DzentoPOS2

**آخر تحديث:** 2026-10-27
**المرجع البصري:** [`DESIGN_SYSTEM.md`](DESIGN_SYSTEM.md) — [`docs/design-system-plan.md`](docs/design-system-plan.md)
**قاعدة القبول:** كل بند لا يُعتبر منجزًا إلا إذا تحقّق دليل قابل للقياس (بناء نظيف + فحص آلي عند 375/768/1440).

**المفتاح:** ⬜ لم يبدأ · 🟨 قيد التنفيذ · ✅ مكتمل ومُتحقَّق · ⏭️ مُستبدَل بمرحلة أحدث


| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| 0.1 | إنشاء `Todos.md` | ✅ | الملف موجود |
| 0.2 | إنشاء `DESIGN_SYSTEM.md` | ✅ | الملف موجود |
| 0.3 | تعبئة `README.md` (كان فارغًا) | ✅ | بنية المشروع + التشغيل + البناء |

## P1 — إصلاحات أساسية

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| 1.1 | رقم النسخة `1.0.0` بدون `+<sha>` | ✅ | فحص آلي `P1.1`: `Application.ProductVersion == "1.0.0"` |
| 1.2 | تطبيق اللغة المحفوظة عند الإقلاع | ✅ | فحص آلي `P1.2`: حفظ `fr` ⇒ `CurrentUICulture = "fr"` والنص يختلف عن الإنجليزي |
| 1.3 | عدم الكتابة فوق التفضيل غير المعروف | ✅ | فحص آلي `P1.3a`/`P1.3b`: `de-DE` يبقى كما هو في `user-settings.json` والقائمة لا تفرض أول عنصر |
| 1.4 | العملة الافتراضية `DZD` / رمز `دج` / `DZ` / `Africa/Algiers` | ✅ | فحص آلي `P1.4`/`P1.4b`/`P1.4c`/`P1.4d`: `AppSettings` + `RegionalContext` + `RegionalCatalog` (رمز `دج`) |
| 1.5 | عملة `LYD` الناقصة في الكتالوج | ✅ | فحص آلي `P1.5`: `RegionalCatalog.GetSymbol("LYD") = "د.ل"` |
| 1.6 | «تذكّرني»: اسم + كلمة مرور عبر Windows Credential Manager | ✅ | فحص آلي `P1.6`: دورة تخزين/قراءة/حذف ناجحة (`uihost-user` / `uihost-pass`) |
| 1.7 | حذف كلمة المرور المبدّبة من الكود | ✅ | فحص آلي `P1.7`: 0 ملف `.cs` يحتوي `Admin@123` |
| 1.8 | جملة حقوق النشر أسفل نموذج الدخول | ✅ | فحص آلي `P1.8`: `lblCopyRight` ابنٌ لجدول على مستوى النموذج (ليس داخل `pnlLeft`)، أسفل الحاوية |

## P2 — الشريط الجانبي

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| 2.1 | تمرير القائمة الجانبي عند صغر الشاشة | ✅ | فحص آلي `P2.1`/`P2.1b`: `AutoScroll == true` و14 زرًّا قابلًا للوصول عند 720px |
| 2.2 | كتلة معلومات المستخدم (اسم/دور/فرع) بدون قصّ | ✅ | فحص آلي `P2.2`/`P2.2b`: المسميات داخل حدود `pnlUserInfo` عند 220px و100px |
| 2.3 | انعكاس RTL لكتلة المستخدم | ✅ | فحص آلي `P2.3`: `RightToLeft == true` + `MiddleRight` + أفاتار `Dock = Right` |
| 2.4 | أفاتار المستخدم (بدل `IconChar.None`) | ✅ | فحص آلي `P2.4`: `iconPictureBox1.IconChar = User` |

## P3 — نقطة البيع

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| 3.1 | فتح «نقطة البيع» افتراضيًا بعد الدخول | ✅ | فحص آلي `P3.1`: `contentPanel` يحتضن `SalesForm` بعد الدخول |
| 3.2 | زر «+» بجانب قائمة الزبون يفتح نافذة إضافة زبون | ✅ | فحص آلي `P3.2`: زر «إضافة عميل» موجود، وإنشاء زبون يُدرجه في القائمة |
| 3.3 | إصلاح عيب نوع `SelectedValue` في قائمة الزبائن | ✅ | فحص آلي `P3.3`/`P3.3b`: كل العناصر `CustomerItem` والمعرّف يُقرأ من `SelectedItem` (`SelectedValue` فارغ) ولا سطر غير تعليقي في `SalesForm.cs` فيه `SelectedValue` |
| 3.4 | نافذة حاسبة + شروط إضافة النتيجة | ✅ | فحص آلي `P3.4`/`P3.4b`: `SalesCalculatorDialog` + 5 أهداف (سطر منتج / مبلغ حر / مدفوع / خصم / ضريبة) |
| 3.5 | حقل البحث عن المنتج وزر الباركود لا يختفيان تحت السلة | ✅ | فحص آلي `P3.5`: حقل البحث `{X=413,Y=37,478x36}` وزر الباركود `{X=891,Y=37,150x36}` غير مغطّيين بالسلة (`covered search=False barcode=False`) وداخل حدود النموذج |
| 3.6 | فصل المنتجات عن السلة بتخطيط جدولي بلا تداخل | ✅ | فحص آلي `P3.6`: المنتجات `{X=405,644x516}` والسلة `{X=7,392x516}` — لا تداخل، وكلاهما داخل الحدود (السبب الجذري: `Fill` يُرصّ أولًا ثم `Right` فوقه) |
| 3.7 | سلة مكدّسة أسفل المنتجات عند صغر الشاشة | ✅ | فحص آلي `P3.7`: صفحة بعرض 420px ⇒ المنتجات `{Y=29,390x278}` والسلة `{Y=313,390x341}` (مكدّسة) و0 تجاوز |

## P4 — صفحة الموردين

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| 4.1 | `SuppliersForm` (جدول + بحث + CRUD) | ✅ | `Forms/Suppliers/SuppliersForm.cs` + شبكة 7 أعمدة + بحث `SearchAsync` + إضافة/تعديل/حذف |
| 4.2 | `SupplierEditDialog` | ✅ | `Forms/Suppliers/SupplierEditDialog.cs` — تحقق من الاسم + حفظ/تعديل |
| 4.3 | تسجيل DI + زر جانبي | ✅ | `AddTransient<SuppliersForm>()` + `btnSuppliers` في الشريط الجانبي + `NavigateTo<SuppliersForm>` |
| 4.4 | عدد الاستعلامات عند فتح الصفحة | ✅ | استدعاء واحد لـ`GetAllAsync`/`SearchAsync` في `LoadDataAsync` |

## P5 — صفحة الإعدادات

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| 5.1 | بطاقات بارتفاع محسوب (لا انهيار) | ✅ | فحص آلي `P5.1`: 6 بطاقات، كلها `Height > 0` و`Visible` |
| 5.2 | شبكة متجاوبة (عمودان ← عمود واحد) | ✅ | فحص آلي `P5.2`: عرض بطاقة موحّد (278px عند 375، 671px عند 768، 663/664px عند 1440) و0 تجاوز |
| 5.3 | `NavigationService` لا يترك الصفحة فارغة عند الخطأ | ✅ | حل النموذج داخل `try/catch` قبل إتلاف السابق + `AppMessageBox.Error` |

## P6 — الماسح

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| 6.1 | `Dispose` لـ`VideoCaptureDevice` (تسريب COM) | ✅ | `Stop()` ينفّذ `SignalToStop`+`WaitForStop` ثم `(device as IDisposable)?.Dispose()` |
| 6.2 | ترجمة نافذة الماسح | ✅ | كل نصوص `BarcodeScannerOverlay` عبر `AppStrings` |
| 6.3 | حقل باركود في المنتج + migration | ✅ | `Barcode` في `Product`/`ProductDto`/Create/Update + migration `20261021192318_AddProductBarcode` + مطابقة الماسح في `SalesForm` |
| 6.4 | مسار المسح بالكاميرا لا ينهار (`ArgumentException` في `AddArc`) | ✅ | فحص آلي `P6.4`: تطبيق منطقة الزوايا على لوحة بارتفاع 0 و1×1 لا يرمي استثناءً، والمنطقة تُطبَّق على لوحة بأبعاد عادية |

## P7 — الترجمة

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| 7.1 | الملف الأساسي `AppStrings.resx` بالإنجليزية | ✅ | عدّ آلي: 0 قيمة عربية في الملف الأساسي (289 قيمة) |
| 7.2 | كل اللغات لها نفس المفاتيح | ✅ | 289 مفتاحًا في base/ar/en/fr، فرق = 0 |
| 7.3 | كل مفاتيح هذه الجولة في 4 ملفات + `Designer` | ✅ | مفاتيح الموردين (9) موجودة في 4 ملفات + `AppStrings.Designer.cs` |

## P8 — التحقق النهائي

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| 8.1 | قياس عند 375 / 768 / 1440 | ✅ | المِضيف الآلي: **40 ناجح / 0 فاشل** — صفحة الإعدادات والشريط والصفحة داخل الشفق: 0 تجاوز عند 375/768/1440، وصفحة البيع (بحث/باركود/سلة) بلا تداخل |
| 8.2 | بناء نظيف 0 أخطاء | ✅ | `dotnet build DzentoPOS2.sln -t:Rebuild -v:m -nologo` ⇒ `Build succeeded, 0 Error(s)` (21 تحذيرًا `NU1701` سابقًا) |
| 8.3 | تحديث `CHANGELOG.md` | ✅ | قسم `## [Unreleased] — 2026-10-22 (settings layout + point of sale fixes)` + بند «إصلاح تخطيط نقطة البيع» |

## P9 — الوضع الداكن (التبويبات وشارات المؤشرات)

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| 9.1 | عدم بقاء أي سطح فاتح عند التحويل للداكن | ✅ | فحص آلي `DM.1`: 12 صفحة، 0 سطح فاتح (شمل شارات `KpiCard`) |
| 9.2 | وضوح النصوص على السطح الداكن | ✅ | فحص آلي `DM.2`: 44 تسمية ظاهرة في لوحة التحكم، 0 بلون أمامي داكن |
| 9.3 | شريط التبويبات داكن فعليًا (OwnerDraw) | ✅ | فحص آلي `DM.3`: بكسل الترويسة/الشريط/المحتوى = `(17,24,39)` في Sales/Products/Reports عبر `TabsStyler` |
| 9.4 | بناء نظيف + إجمالي الفحوص | ✅ | `Build succeeded, 0 Error(s)` + المِضيف: **91 ناجح / 0 فاشل** (P1–P8 + DM.1–DM.3 + A.1–A.4 + B.1–B.4 + C.1–C.2 + D.1–D.3) |

## A — الإصلاحات العاجلة (الموافق عليها: A أولاً ثم B)

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| A.1 | منتج لا يُتابع عند التعديل/الحذف (`DbContext` مشترك) | ✅ | `ProductService.UpdateAsync/DeleteAsync` و`ExpenseService.UpdateAsync/DeleteAsync` يقرأان الكيان متتبّعًا من `_context` (مع `Include(Category, Unit)` للتعديل) بدلاً من `GetByIdAsync` بلا متابعة + `Update` 분리ًّا. فحص `A1.1`: إنشاء منتج ثم تعديل مرتين على السياق نفسه ⇒ الاسم النهائي `uihost-product-2` (قبل الإصلاح: استثناء "instance with the same key is already being tracked") |
| A.2 | بطاقات الإعدادات تقصّ الحقول عند 375/768/1440 وبتغيير اللغة | ✅ | `SettingsCard` تقيس عناوين الحقول عبر `TextRenderer` وتحدّث أنماط صفوف stack (العناوين + الحقول الكاملة) في كل تغيير عرض/لغة. فحص `A2.1`: 0 حقل مقصوص عند 375/768/1440 بالعربية والفرنسية والإنجليزية |
| A.3 | زرّا «الرئيسية» و«المبيعات» مُفعَّلان معًا بعد الدخول | ✅ | `MainForm_Load` يستدعي `ApplyShellTheme()` بعد `NavigateTo<SalesForm>(BtnSales)`؛ الألوان تُحدّد من المصدر الوحيد `_activeSidebarButton`. فحص `A3.1`: 9 أزرار جانبية، زر واحد بلون الأساس (`BtnSales`) |
| A.4 | أعمدة «التصنيف» و«الوحدة» فارغة في شبكة المنتجات | ✅ | `ProductRepository.GetAllAsync/FindAsync` carry `Include(Category).Include(Unit).AsNoTracking()`. فحص `A4.1`: `CategoryName`/`UnitName` غير فارغَين عبر `IProductService.GetAllAsync` |
| A.5 | التحقق النهائي للحزمة A | ✅ | `dotnet build DzentoPOS2.sln -t:Rebuild`: 0 أخطاء. المِبض الآلي: **61 ناجح / 0 فاشل** |

## B — التصنيفات والوحدات (مكتملة)

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| B.1 | خدمات التصنيفات والوحدات (CRUD + التحقق من الإسم المكرر) | ✅ | `IProductCategoryService`/`ProductCategoryService` و`IProductUnitService`/`ProductUnitService` (GetAll, GetById, Search, ExistsByName, GetBaseUnits, Create, Update, Delete). التعديل/الحذف يقرأان الكيان متتبّعًا من `_context` (نفس عيب A.1). الوحدات تتحقق أيضًا من فريدية `Symbol` (فهرس فريد في القاعدة). فحوص `B.1.1–B.1.4` (إنشاء/تعديل/ظهور في GetAll/حذف) و`B.2.2–B.2.4` و`B.3.1` (اسم مكرر ⇒ `InvalidOperationException`) |
| B.2 | نماذج إضافة/تعديل التصنيف والوحدة في الشريط الجانبي | ✅ | `ProductCategoriesForm`/`ProductCategoryEditDialog` و`ProductUnitsForm`/`ProductUnitEditDialog` + زرّا `btnCategories`/`btnUnits` في `MainForm` (أيقونتا `Tags`/`Ruler`). فحوص `B.4.2` (الزرّان ظاهران ونصّهما مترجم) و`B.4.3`/`B.4.4` (النقر يفتح النموذج الصحيح) |
| B.3 | تسجيل النماذج في DI + ترجمة العناوين | ✅ | تسجيل `ProductCategoriesForm`/`ProductUnitsForm` في `Infrastructure\DependencyInjection.cs`؛ 12 مفتاحًا في `AppStrings` + ar/en/fr + خصائص `AppStrings.Designer.cs`. فحص `B.4.1` (حلّ النموذجين من DI) |
| B.4 | فحوصات CRUD آلية (إضافة/تعديل/حذف + تكرار الاسم) | ✅ | المِضيف: **10/10** لقسم B — `B.1.1–B.1.4`, `B.2.1–B.2.4`, `B.3.1`, `B.4.1–B.4.4` |

## C — صندوق النقود اليومية (مكتملة — مُستبدَلة بـ E3)

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| C.1 | جلسة صندوق يومية (`CashSession`) مربوطة بحركات الصندوق — مُستبدَلة بـ E3 | ⏭️ | كيان `CashSession` + `CashSessionStatus` + `CashSessionConfiguration` + migration `20261023082755_AddCashSessions` + `ICashSessionRepository`/`CashSessionRepository` + `ICashSessionService`/`CashSessionService` (فتح جلسة واحدة لكل صندوق، إغلاق يحسب المتوقع من حركات الصندوق بإشارة `MovementType` ويضبط `ExpectedBalance`/`Difference`/`Status` و`Cashbox.CurrentBalance`) + `CashboxSeeder` في `DbInitializer`. فحوص `C.1.0–C.1.6`: فتح 1000 ثم حركتا بيع 500/شراء 200 ثم إغلاق 1350 ⇒ المتوقع 1300 والفرق 50 والرصيد 1350، ومنع فتح جلسة ثانية. **استُبدلت بنموذج E3 المربوط بالجلسة والملخّص الموحَّد** |
| C.2 | نموذج افتتاح/إغلاق الصندوق مع الرصيد الافتتاحي والختامي — مُستبدَلة بـ E3 | ⏭️ | `CashSessionForm` (صندوق + رصيد افتتاحي + فتح + رصيد ختامي + إغلاق + شبكة الجلسات + ملصق حالة) + زر `btnCashSessions` في `MainForm` + تسجيل DI + 14 مفتاح ترجمة (ar/en/fr + `AppStrings.Designer.cs`). فحوص `C.2.1–C.2.5`: الحل من DI، عناصر النموذج، حالة الأزرار، ظهور الزر ونصّه المترجم، والنقر يفتح النموذج. **استُبدل بـ `CashSessionForm` الجديد (لوحة KPI) و`CashSessionOpenDialog` في F3** |

## D — الأداء ومراجعة هوية Apple/DPI (مكتملة)

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| D.1 | خفض استعلامات لوحة التحكم (~25 متسلسلة) | ✅ | `DashboardService`: تجميع KPI لكل جدول في استعلام واحد (`GroupBy(_ => 1)` + `Sum(شرط ? قيمة : null)`)، ودمج قراءة المبيعات اليومية/الشهرية، ودمج أسماء التصنيفات في استعلام التوزيع. فحص `D.1.1` عبر `DbCommandInterceptor`: **19 أمرًا** (الهدف ≤ 22) + `D.1.2`: 30 نقطة اتجاه |
| D.3 | مراجعة هوية Apple ودقة DPI للشاشات عالية الكثافة | ✅ | لا أصول/هوية Apple في الحل (Windows-only)، و`ApplicationHighDpiMode=PerMonitorV2` في `DzentoPOS2.WinForms.csproj`. فحص `D.3.1`: `AutoScaleMode=Font` و`DeviceDpi=96`، و`D.3.2`: وجود `PerMonitorV2` |

## E — صحة المبيعات والمخزون والصندوق (مكتملة)

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| E1 | صحة إجماليات الفاتورة والتفصيل وترقيم المستندات | ✅ | `InvoiceCalculator` (تقريب كل سطر ثم المجموع، `AwayFromZero`) + `DocumentNumberService` (تسلسل يومي). دليل: `InvoiceCalculatorTests` (وحدة) و`SaleServiceE1Tests.SavedInvoiceTotalsMatchCalculator` (تكامل) — الإجماليات المحفوظة = مخرجات الحاسبة |
| E2 | خصم المخزون المتتبَّع واسترجاعه عند الإلغاء | ✅ | `StockService` (حركات `SaleOut`/`VoidIn` + منع الرصيد السالب) و`SaleService.VoidAsync` يعيد الكمية للمخزون. دليل: `SaleServiceE2Tests` (خصم + دفتر حركات + استرجاع عند الإلغاء + منع الرصيد السالب) و`BusinessClockTests` |
| E3 | ربط حركات الصندوق والفواتير بالجلسة + قيود FK + فهرس الجلسة المفتوحة الفريد | ✅ | migration `20261023140849_E3_CashSessionLinking` (FK restrict: `CashTransactions`/`SalesInvoices` → `CashSessions`) و`20261027164243_E3_UniqueOpenSessionIndex` (`UX_CashSessions_OpenPerCashbox` فهرس فريد مفلتر `"Status" = 0 AND "IsDeleted" = 0`). دليل: `SaleServiceE3Tests` (منها انحدار عيب FK عند إتمام البيع) و`MigrationSmokeTests` و`LegacySchemaRepairTests` |

## F — استكمال دورة البيع (مكتملة)

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| F1 | تحصين إقلاع المتجر وإصلاح المخطط القديم وتوطين الأخطاء | ✅ | `LegacySchemaRepair` يُصلح مخطط SQLite القديم قبل الترحيلات، والإقلاع يهيّئ الفرع/المستودع/الصندوق مرة واحدة، و`ExceptionFormatter`/`ExceptionLogger` (رسائل محلية + سجل ملفات). دليل: `LegacySchemaRepairTests` و`MigrationSmokeTests` و`ExceptionFormatterTests` و`ExceptionLoggerTests` |
| F2 | أوضاع الخصم (نسبة/مبلغ) وصحة الدفع (يشمل Part 0/F2b) | ✅ | commit `40bae45`: دعم النسبة والمبلغ، الدفع المطبَّق/الباقي/الصرف، واشتراط زبون للبيع الآجل. **Part 0 (F2b)**: سقف الخصم يُطبَّق على الوضعين معًا عبر النسبة الفعلية `DiscountAmount/Subtotal*100`، وتهيئة `sales.max_discount_percent` عند التنفيذ (`AppSettingKeys` + `AppSettingSeeder`)، وتوطين أخطاء المجال عبر `CodedDomainException`/`DomainErrorCodes`/`ExceptionFormatter.LocalizeCode`، وتثبيت `_numPaid.Maximum = 99,999,999`. دليل: `SaleServiceF2Tests` (`Discount_ceiling_*` للوضعين + parity) و`ReceiptLocalizationTests` (0 نص إنجليزي مكتوب صراحةً في قالب الطباعة) |
| F3 | لوحة تحكم الصندوق + الإجماليات الحية + الفرض | ✅ | `CashSessionOpenDialog` (نموذج أول: التخطيط في `.Designer.cs` والمنطق في `.cs`) و`CashSessionForm` مُعاد بناؤه (8 بطاقات `KpiCard`، تدفّق 4→2→1، RTL، وضع داكن، تنسيق عبر `RegionalContext`)، و`ISalesEventPublisher` مفرد يُطلق `SaleCompleted`/`SaleVoided`/`CashMovementAdded` **بعد الـcommit** + شريحة في شريط الحالة، و`GetSessionSummaryAsync` مصدر وحيد يستخدمه `CloseSessionAsync` (لا إغلاق مرتين)، وربط المصروفات بالصندوق. دليل: `CashSessionE3Tests` (11) و`CashSessionPolicyTests` (3، اكتشاف يوم العمل السابق بالتوقيت المحلي) — الوحدة 79 والتكامل 37، بناء نظيف (0 أخطاء / 21 تحذير `NU1701`)
| F3b | ترقية آمنة لفهرس الجلسة المفتوحة الفريد + توطين لواحق KPI المختصرة | ✅ | `LegacySchemaRepair` يغلق الجلسات المفتوحة المكرّرة لكل صندوق قبل `MigrateAsync` داخل معاملة واحدة وبشكل مُكرَّر الأمان (idempotent): يُبقي الأحدث بـ`OpenedAtUtc` ويغلق الباقي بـ`Status = Closed` و`ClosedAtUtc = max(OpenedAtUtc, آخر حركة/فاتورة مرتبطة)` مع الحفاظ على الأرصدة وإلحاق الملاحظة «Auto-closed by upgrade (duplicate open session)» وتسجيل معرّفات الجلسة/الصندوق، دون حذف أي صف أو لمس الفواتير/الحركات. والترحيل `20261027164243_E3_UniqueOpenSessionIndex` أصبح كامل الأمان عند الترقية: `DROP INDEX IF EXISTS "IX_CashSessions_CashboxId"` و`CREATE UNIQUE INDEX IF NOT EXISTS "UX_CashSessions_OpenPerCashbox"`، و`Down()` متماثل (`DROP INDEX IF EXISTS` + `CREATE INDEX IF NOT EXISTS`)، فلا ينهار الإقلاع سواء كان الفهرس مفقودًا أو موجودًا مسبقًا. وتوطين لواحق `KpiValueFormatter.Compact` عبر `KpiCompactSuffixes` ومفاتيح `KpiSuffixThousand/Million/Billion` في base/ar/en/fr + `Designer` (فرق 0). دليل: `LegacySchemaRepairTests` (4 فحوص ترقية عبر ترحيلات حقيقية: وجود تكرار ينجح ويُغلق الأقدم، وغياب التكرار لا يغيّر شيئاً، وقاعدة جديدة تفرض الفهرس، وفهرس موجود مسبقًا يجعل الترحيل بلا أثر) — الوحدة 84 والتكامل 41، وبناء محلول نظيف (0 أخطاء / 21 تحذير `NU1701`)، و`dotnet ef migrations has-pending-model-changes` نظيف |

## G — الترحيل إلى .NET 10 (مكتمل)

| # | المهمة | الحالة | دليل الإنجاز |
|---|---|---|---|
| G0 | تثبيت SDK عبر `global.json` | ✅ | commit `e5646fa`: الإصدار `10.0.403` مع `rollForward: latestFeature`، و`dotnet --version` داخل المستودع = `10.0.403` |
| G1 | إعادة استهداف الحل إلى `net10.0` / `net10.0-windows` ومحاذاة حزم `Microsoft.*` إلى `10.0.14` | ✅ | commit `3c07feb`: 0 أخطاء / 21 تحذير `NU1701`، الوحدة 84 والتكامل 41، وإصلاح WFO1000 عبر `.editorconfig` + `[DisallowNull]` على `EZComboBox.SelectedValue` + إضافة أعمدة ربط الجلسة إلى ترحيل `InitialCreate` |
| G2 | تدقيق الحزم (توافق / ترخيص / ثغرات / إهمال) | ✅ | commit `0d67e28`: لا حزم مصابة؛ `xunit 2.9.3` مُهمَل فقط؛ قرار تراخيص AutoMapper/QuestPDF/AForge؛ `docs/net10-package-audit.md` |
| G3 | مراجعة تغييرات .NET 10 وEF Core 10 المكسِرة | ✅ | commit `e57b7f8`: القائمة القابلة للتطبيق + غير القابلة في `docs/net10-breaking-changes.md` |
| G4 | التحقق الكامل (بناء / اختبارات / ترحيلات / لقطة النموذج) | ✅ | commit `78b833a`: 0 أخطاء / 21 تحذير، الوحدة 84 والتكامل 41، `has-pending-model-changes` نظيف، وترحيل فحص أثبت أن Up/Down فارغان؛ `docs/net10-verification.md` |
| G5 | تشغيل مسار الإقلاع على نسخ قواعد بيانات حقيقية + تحديث الوثائق | ✅ | ثلاث حالات بداية (dev/legacy/design) تُرقّى دون فقدان صفوف مع تهيئة كاملة، وREADME/CHANGELOG/Todos محدَّثة؛ `docs/net10-smoke-run.md` |

