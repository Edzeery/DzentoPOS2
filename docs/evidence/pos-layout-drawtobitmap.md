# POS Layout Visual Verification (DrawToBitmap)

## Method

For each combination of (width × view mode × direction × theme), the `PosProductList`
control is rendered to a PNG via `Control.DrawToBitmap`. The resulting image is
inspected for:

- No text clipping (all text fits within its sub-rectangle).
- No control overflow (no element extends beyond the card bounds).
- Correct RTL mirroring (image on right, text on left in Arabic).
- Correct dark-mode colors (background, surface, text, borders).
- Card/list mode differences are visible.

## Combinations

| Width | Mode | Direction | Theme | File |
|---|---|---|---|---|
| 375 | Cards | LTR | Light | `pos-375-cards-ltr-light.png` |
| 375 | Cards | LTR | Dark | `pos-375-cards-ltr-dark.png` |
| 375 | Cards | RTL | Light | `pos-375-cards-rtl-light.png` |
| 375 | Cards | RTL | Dark | `pos-375-cards-rtl-dark.png` |
| 375 | List | LTR | Light | `pos-375-list-ltr-light.png` |
| 375 | List | RTL | Dark | `pos-375-list-rtl-dark.png` |
| 768 | Cards | LTR | Light | `pos-768-cards-ltr-light.png` |
| 768 | Cards | RTL | Dark | `pos-768-cards-rtl-dark.png` |
| 768 | List | LTR | Light | `pos-768-list-ltr-light.png` |
| 1440 | Cards | LTR | Light | `pos-1440-cards-ltr-light.png` |
| 1440 | Cards | RTL | Dark | `pos-1440-cards-rtl-dark.png` |
| 1440 | List | LTR | Light | `pos-1440-list-ltr-light.png` |

## How to generate

```csharp
var list = new PosProductList { Items = sampleItems, ViewMode = PosViewMode.Cards };
list.RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No;
list.ClientSize = new Size(width, 800);
list.CreateControl();
ThemeManager.SetMode(theme);

using var bmp = new Bitmap(width, 800);
list.DrawToBitmap(bmp, new Rectangle(0, 0, width, 800));
bmp.Save($"docs/evidence/{filename}", System.Drawing.Imaging.ImageFormat.Png);
```

## Verification checklist

- [ ] No text overflow beyond card bounds.
- [ ] Image renders at correct position (left in LTR, right in RTL).
- [ ] Price is right-aligned in LTR, left-aligned in RTL.
- [ ] Stock badge visible and correctly colored (red for 0, yellow for <5).
- [ ] Dark mode: background is `#090E16`, surface is `#111827`, text is `#E6EAF0`.
- [ ] Light mode: background is `#F5F7FA`, surface is `#FFFFFF`, text is `#1D1D1F`.
- [ ] Card mode shows 1/2/4 columns at 375/768/1440.
- [ ] List mode shows full-width rows.
- [ ] Empty state shows "No products" centered.

## Automated check

The `--layout-audit` tool (`UiLayoutAudit`) already covers `PosProductList` indirectly
through the `PosProductList` control's layout properties. For pixel-level verification,
run the snippet above and inspect the PNGs manually or via an image comparison tool.
EOF