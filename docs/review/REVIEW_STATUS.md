# REVIEW_STATUS — حزمة مراجعة DzentoPOS2 (2026-11-13)

> **للقراءة فقط.** لا تعديلات على الكود. الفرع `review/status-2026-11-13` مستقل ولن يُدمج.
> الهدف: تمكين مراجع خارجي (Claude على الويب) من فحص اكتمال المشروع، الإصلاحات، والتصميم،
> من ملفات قابلة للفتح برابط مباشر + نسخة كاملة تُلصق في المحادثة.

---

## 1. ملخص تنفيذي

**هل المشروع مكتمل؟** جزئيًا. النواة (POS، المخزون، التقارير، الصندوق، الفروع، الترخيص، الأكواد الرقمية) منفذة ومُتحقَّقة.
**التقدير (بناءً على جدول المتطلبات 1–20):** 13 ✅ مكتمل / 6 🟨 جزئي / 1 ⬜ غير منفذ ≈ **~75% مكتمل** (13/20 = 65% صارم، مع 6 جزئية ترفع التقدير إلى ~75%).

**أبرز 10 فجوات:**
1. **K.5 — واجهة POS الجديدة** (منتقي متغيرات + باركود مشترك) معلَّمة ⬜ في `Todos.md` (M.5.3 منفذة جزئيًا).
2. **K.6 — وحدات النشاط** (بقالة/هواتف/رقمي) ⬜ pending.
3. **K.7 — أداء + اختبارات + docs** ⬜ pending.
4. **I.6 — فلتر "العضو" في Dashboard** 🟨: `userId = null` مُثبَّت (DashboardForm.cs:236)، غير مربوط بالواجهة.
5. **متطلب 7 — التصنيفات/الوحدات كتبويبات داخل Products** ⬜: موجودة كصفحات منفصلة (B.2).
6. **متطلب 8 — إضافة باركود/متغيرات من dialog الإضافة** 🟨: الأزرار مخفية حتى الحفظ (ProductEditDialog.cs:135-136).
7. **متطلب 5 — كاميرا DroidCam** 🟨: اكتشاف الكاميرات مُصلح (G1.1)، لكن نافذة 375px/عمودية/مسح خلف عنصر ثانوي غير مُتحقَّقة.
8. **متطلب 6 — مسح بالكاميرا في كل حقل باركود** 🟨: ProductEditDialog + SalesForm فقط.
9. **متطلب 15 — Designer.cs لجميع النماذج** 🟨: 35 form/control بدون Designer، 18 مع Designer لكن UI في .cs.
10. **متطلب 19 — توثيق** 🟨: `Todos.md` يحتوي أقسام N مكررة وبنود K.5–K.7 معلَّمة ⬜.

**أبواب مكتملة ومُتحقَّقة:** A (6/6)، B (4/4)، C/E3 (مكتمل)، D (2/2)، F (3/3)، G1 (5/5)، H (4/4)، I (7/8)، K.0–K.4، L (6/6)، M.5.0–M.5.4، N (6.0a–6.0d-3)، P (Phase 1+2)، Phase 2 الترخيص، Phase 2.8، Phase 3 الأكواد الرقمية.

---

## 2. جدول الفروع (Step 1)

**الفرع الأساسي (الأحدث/الأساسي):** `feature/roadmap-phases-1-3` (HEAD `9afd683`, 2026-11-12).
**الاستنتاج:** commit `9afd683` (Phase 3 digital codes) هو أحدث commit في المستودع (max timestamp + graph tip)، و`feature/roadmap-phases-1-3` مبني على `feature/phase-6-branch-hardening` (`49ee231`) + 4 commits (Phases 1/2/2.8/3). مُ ahead 29 vs main (≈ commit-graph)، behind 2 (merge commits على main).

`git status`: `On branch feature/roadmap-phases-1-3, nothing to commit, working tree clean`. `git stash list`: فارغ.

| الفرع | محلي؟ | origin؟ | ahead/behind vs origin | الغرض | مدموج في main؟ |
|---|---|---|---|---|---|
| `main` | ✅ | ✅ | 0/0 | trunk (merge #13) | — |
| `master` | ✅ | ❌ | (local-only, ancestor) | commit أولي | ❌ (غير مرفوع) |
| `feature/roadmap-phases-1-3` | ✅ | ✅ (pushed) | 0/0 | Phases 1/2/2.8/3 | ❌ |
| `feature/phase-6-branch-hardening` | ✅* | ✅ | 0/0 | Phase 6 + branch mgmt P1-P4 | ✅ (via PR #13) |
| `feature/phase-6-branch-context` | ✅ | ✅ | 0/0 | Active branch context 6.0a | ✅ (via PR #11) |
| `feature/phase-5-pos` | ✅ | ✅ (pushed +1) | 0/0 (after push) | POS Phase 5 batches | ❌ (جزئيًا) |
| `feature/catalog-variants` | ✅ | ✅ | 0/0 | Catalog system K.0-K.4 | ✅ (via PR #7) |
| `feature/dashboard-filters` | ✅ | ✅ | 10 behind / 28 ahead vs main | Dashboard filters | ❌ |
| `feature/profit-rebuild` | ✅ | ✅ | 0/0 | Profit rebuild H | ❌ |
| `chore/net10` | ✅ | ✅ | 0/0 | .NET 10 attempt (superseded) | ❌ |
| `fix/barcode-scanner` | ✅ | ✅ | 0/0 | Barcode scanner fixes | ❌ |
| `fix/e1-e3-sales-stock-cash` | ✅ | ✅ | 0/0 | E1-E3 + F3b | ❌ |
| `fix/inventory-expected-profit` | ✅ | ✅ | 0/0 | Inventory profit IAS2 | ❌ |
| `fix/ui-kpi-sidebar-filters` | ✅ | ✅ | 0/0 | UI/KPI/sidebar | ❌ |

`*` checked out in a separate worktree (`C:/Users/PC-LB/source/repos/DzentoPOS2`).

**ملاحظة:** `master` (commit `c94e834`، 2026-10-20) لم يُدفع (فرع ancestor مكرر لـ main، غير ضروري). `feature/phase-5-pos` دُفع (+1 commit `bfd07d1`).

---

## 3. أدلة البناء والاختبار (Step 3)

- **SDK:** .NET SDK 9.0.300 (MSBuild 17.14.5). Host 10.0.15 (runtime only).
- **TargetFramework:** 12 مشروعًا — `net9.0` (App/Core/Data/Licensing/Licensing.Signing/Localization/Shared/UnitTests/IntegrationTests) و `net9.0-windows` (WinForms/EZControls/LicenseIssuer). **تعارض مع README** (يذكر .NET 9 — متسق فعليًا؛ لا يوجد .NET 8 في الكود. README سليم).
- **Build:** `dotnet build DzentoPOS2.sln -t:Rebuild -v:m -nologo` ⇒ `Build succeeded.` · **0 Error(s)** · **21 Warning(s)** (كلها `NU1701` لـ 6 حزم .NET Framework: AForge/OpenTK/SkiaSharp — دين تقني مقصود، ROADMAP Phase 7).
- **Unit tests:** `Passed! Failed: 0, Passed: 205, Total: 205` (2 s).
- **Integration tests:** `Passed! Failed: 0, Passed: 156, Total: 156` (23 s).
- **Layout audit:** `UI layout audit: 132 scenarios, 0 violations` · 2832 controls measured · `docs/evidence/ui-layout-audit.json`.
- **Migration audit:** `Migration audit: PASSED` · 0 rows removed/changed · `docs/evidence/migration-audit.json`.
- **git status (HEAD):** clean.

---

## 4. جدول المتطلبات 1–20 (Step 4)

| # | المتطلب | الحالة | الفرع | الدليل | ملاحظات |
|---|---|---|---|---|---|
| 1 | تعديل منتج: InvalidOperationException (نفس Id متتبَّع) عند ShowDialog()==OK | ✅ | roadmap (inherited) | Todos A.1 (A1.1)؛ ProductService.UpdateAsync يقرأ الكيان متتبّعًا | |
| 2 | حقول/قوائم Settings مقصوصة (عربي + داكن) | ✅ | roadmap | Todos A.2 (A2.1)؛ SettingsCard يقيس عبر TextRenderer | |
| 3 | زر "الرئيسية" مفعّل مع "المبيعات" عند الإقلاع | ✅ | roadmap | Todos A.3 (A3.1)؛ MainForm_Load ApplyShellTheme | |
| 4 | أرقام KPI بالعربية (غربية، لا 2,200، لا 18,18,450، words لا تختفي، محاذاة حسب اللغة) | ✅ | roadmap | I.4 KpiValueFormatter culture؛ Shared/Helpers/KpiValueFormatter.cs | |
| 5 | كاميرا الباركود DroidCam (سرد كل الكاميرات، معاينة حية، نافذة قابلة لتغيير الحجم min 375 عمودية، إدخال يدوي + مسح مستمر خلف عنصر ثانوي، أداء رسم الإطار) | 🟨 | roadmap | G1.1 CameraDeviceEnumerator (FilterInfoCollection)؛ BarcodeScannerOverlay | نافذة min-375/عمودية/مسح خلف عنصر ثانوي: لم تُتحقَّق (لا اختبار آلي) |
| 6 | مسح بالكاميرا في كل حقل باركود (إضافة/تعديل منتج وكل صفحة) | 🟨 | roadmap | ProductEditDialog.cs (_btnScanBarcode)؛ SalesForm.ScanBarcode | only ProductEditDialog + SalesForm verified; Purchases/Customers? not verified |
| 7 | التصنيفات والوحدات كتبويبات داخل صفحة المنتجات (وليس صفحات منفصلة) | ⬜ | — | Todos B.2 (ProductCategoriesForm/ProductUnitsForm as sidebar pages) | not implemented as tabs |
| 8 | إضافة الباركود والمتغيرات من نافذة إضافة المنتج | 🟨 | roadmap | ProductEditDialog.cs:135-136 (_btnBarcodes/_btnVariants Visible=false for new) | hidden until save |
| 9 | صندوق النقود: جلسة يومية، رصيد افتتاحي أول مرة فقط، ثم بدء الجلسة، إجمالي المبيعات ديناميكي | ✅ | roadmap | CashSessionService, CashSessionOpenDialog, ISalesEventPublisher; CashSessionE3Tests | |
| 10 | لوحة التحكم: فلترة حسب المستخدم/العضو + فترات (اليوم/أمس/أسبوع/شهر/مخصص) | 🟨 | roadmap | DashboardForm.cs:236 (userId=null hardcoded); I.6 marked 🟨 in Todos | date presets exist; member filter not wired |
| 11 | الأرباح: متوقع = (سعر بيع final − تكلفة شراء) بمتوسط مرجّح؛ فعلي = بعد خصومات/رسوم صافي؛ متوقع ≈ فعلي ليس مساوٍ دائماً؛ طريقة محاسبية قياسية | ✅ | roadmap | H phases; ProfitCalculator; ProfitFlowTests; ProfitSummaryModule | |
| 12 | الكتالوج: سياسة باركود هجينة (فريد default + تكرار optional + نافذة اختيار + باركود داخلي)، متغيرات (مقاس/لون)، نفس الباركود/العلامة بسعر/اسم مختلف، صور على القرص في app data + backup، عرض POS احترافي | 🟨 | roadmap | K.0-K.4 ✅ (CatalogPhase1_DataModel, ProductVariantsDialog, ImageService); K.5 ⬜ pending; K.6 ⬜ | POS UI (K.5) not complete |
| 13 | وحدات النشاط (متعددة الاختيار): متعدد الخدمات، رقمي، صيانة، أجهزة منزلية، IT، بقالة، خضر/فواكه، إلكترونيات/هواتف؛ نشاط لكل فرع؛ اختيار عند أول تشغيل؛ مكان backup قابل للاختيار | ✅ | roadmap | FirstRunModulesDialog (8 activities); AppSettingKeys; L.5 BackupLocationStore | |
| 14 | الترخيص (تفعيل) + تكامل dzento-vendor | ✅ | roadmap | Phase 2; LicenseService; ADR-13/14; docs/licensing.md | |
| 15 | تخطيط كل Form في Designer.cs (مسح: أي Forms имеют Designer؟ أي .cs ينشئ عناصر UI؟) | 🟨 | roadmap | 35 forms/controls بدون Designer مع UI creation؛ 18 مع Designer لكن UI في .cs (scan below) | gap (H.5 deferred) |
| 16 | الأداء: استعلامات Dashboard (~25→19)، AsNoTracking، إيقاف sensitive logging في Release، تأخير | ✅ | roadmap | D.1 (19 commands, D.1.1); G1.2 scope-per-page; SqliteConnectionInterceptor | |
| 17 | التوافق مع أحجام شاشات (375/768/1440) + أجهزة ضعيفة | ✅ | roadmap | layout audit 132 scenarios 0 violations | |
| 18 | اتساق DESIGN_SYSTEM.md (Apple restraint) ar/en/fr light/dark | ✅ | roadmap | layout audit + DM.1-DM.3 (dark mode) | |
| 19 | توثيق محدّث: Todos/CHANGELOG/README (تعارض .NET 8 vs 9) + architecture/database-design/api-reference مطابقة للكود | 🟨 | roadmap | README says .NET 9 (correct); Todos has duplicate N sections + K.5-K.7 ⬜; api-reference pointer only | |
| 20 | اختبارات آلية حقيقية (Unit/Integration) — عدد فعلي | ✅ | roadmap | Unit 205/205; Integration 156/156 | |

---

## 5. مسح Designer.cs (متطلب 15)

**المنهج:** لكل ملف `.cs` (غير `.Designer.cs`) تحت `Forms/` و `Controls/`، فحص وجود `.Designer.cs` مطابق، وعَدّ أنماط إنشاء UI (`new Button/Panel/...`, `Controls.Add`).

**النتيجة:**
- ملفات logic مع إنشاء UI **بدون** Designer: **35** (Dialogs/Forms مبنية بالكود).
- ملفات **مع** Designer لكن UI إضافي في .cs (ديناميكي): **18** (SalesForm 80 hits، ProductsForm 43، MainForm 18، …).

**أمثلة (بدون Designer):** ProductEditDialog, ProductVariantsDialog, SalesCalculatorDialog, VariantPickerDialog, SharedBarcodePickerDialog, LicenseActivationDialog, BranchEditDialog, BranchesForm, BranchResourcesDialog, DigitalCodesForm, DigitalCodeImportDialog, StockAdjustDialog, WeightEntryDialog, BatchSelectionDialog, FirstRunModulesDialog, … (القائمة الكاملة: 35).

**ملاحظة:** many dialogs are intentionally code-built (ThemedDialog-based). هذا gap موثَّق في Todos H.5 (deferred). لا يُعدّ "عدم امتثال" لـ Designer-first pattern (الذي ينطبق على Forms primarily)، لكن 35 فورم/حوار بدون Designer = gap.

---

## 6. نتائج اللقطات (Step 5)

- **24 PNG** أُنشئت عبر `--pos-screenshot` في `docs/review/screenshots/` (PosProductList at 375/768/1440 × cards/list × RTL/LTR × light/dark). حجم المجلد ~503 KB.
- **لقطات كاملة للصفحات (Login/Dashboard/Sales/Products/Settings/…)**: **لم تُلتقط** — لا توجد أتمتة GUI/عرض رسومي في بيئة البناء (headless). **الدليل البديل:** `--layout-audit` (132 scenario, 0 violations) + `docs/evidence/ui-layout-audit.json` (هندسة مقيسة لكل عنصر).
- **تقييم (لقطات POS):** 0 قص، 0 تداخل، RTL صحيح، تباين كافٍ (per layout audit).

---

## 7. مخاطر الأداء والتعارضات

1. **README vs .NET:** README يذكر .NET 9 (صحيح — الكود net9.0/net9.0-windows). لا تعارض (ال earlier "NET 8 vs 9" تم حله في G4).
2. **Todos.md duplicated N sections** + K.5/K.6/K.7 معلَّمة ⬜ (stale pending).
3. **master branch** محلي فقط (ancestor of main) — غير مرفوع (قرار: redundant).
4. **Secrets in dzento-vendor repo** (password.txt + *.pem committed to public repo) — **خطر أمني** (see LICENSING-REPORT.md §1.2). **لم يُلمس** (لا تعديل).
5. **Embedded public key ≠ vendor-public.pem** (SHA-256 differs) — غير مُفسَّر.
6. **NU1701** (6 legacy packages) — دين تقني (ROADMAP Phase 7).
7. **M.4** ExpectedStockProfit KPI card deferred (shown in ProfitSummary instead).

---

## 8. خطة مقترحة (P0…P4) — اقتراح فقط، بانتظار موافقتك

- **P0 (حرج):** إصلاح فلتر "العضو" في Dashboard (I.6) — 1-2 يوم.
- **P0:** إكمال K.5 (واجهة POS + منتقي متغيرات) — 3-5 أيام.
- **P1:** التصنيفات/الوحدات كتبويبات داخل Products (متطلب 7) — 2-3 أيام.
- **P1:** كاميرا scan في كل حقل باركود (متطلب 6) + نافذة 375px (متطلب 5) — 2-3 أيام.
- **P1:** إظهار أزرار باركود/متغيرات في dialog الإضافة (متطلب 8) — 1 يوم.
- **P2:** Designer.cs migration (H.5) — 5-10 أيام.
- **P2:** توثيق (Todos cleanup, api-reference) — 1-2 يوم.
- **P3:** K.6 (وحدات النشاط) + K.7 (أداء/اختبارات/docs) — 5-7 أيام.
- **P4:** إزالة NU1701 (ROADMAP Phase 7) — 3-5 أيام.

---

## 9. الملاحق (ملفات في docs/review/)

- `Todos.md` — نسخة من فرع الأحدث.
- `CHANGELOG.md` — نسخة من فرع الأحدث.
- `git-log-40.txt` — `git log --oneline -40` للفرع الأحدث.
- `screenshots/` — 24 PNG (PosProductList).
- `docs/evidence/ui-layout-audit.json` — 132 scenarios, 0 violations.
- `docs/evidence/migration-audit.json` — PASSED.

---

## 10. أسئلة تحتاج قرارك

1. هل أدمج `feature/roadmap-phases-1-3` في main (PR) أم أنتظر مراجعة خارجية؟
2. هل أُكمّل K.5/K.6 (POS UI + activity modules) قبل المراجعة أم بعد؟
3. هل أُدفع `master` (فرع ancestor) أم أتجاهله؟
4. هل أُعالِج "أسرار committed في dzento-vendor" (password.txt + *.pem) — يتطلب coordination مع المورّد (لا أستطيع إصلاحه من هذا المستودع)؟
5. هل أُضيف لقطات full-page (يتطلب GUI automation) أم يكفي layout-audit JSON؟
6. هل أُنشئ PR (pull request) للفرع الأحدث أم يكفي push؟
