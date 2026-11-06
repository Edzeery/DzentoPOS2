using System;
using System.Drawing;

namespace DzentoPOS2.App.Services.Sales;

/// <summary>
/// Pure layout calculator for POS product cards. No UI dependency.
/// Testable at any width / mode / direction.
/// </summary>
public enum PosViewMode { Cards, List }

public static class PosCardLayoutCalculator
{
    public const int CardGap = 8;
    public const int CardPadding = 8;
    public const int ImageSizeNarrow = 64;
    public const int ImageSizeMedium = 48;
    public const int ImageSizeWide = 40;

    /// <summary>Returns (columns, itemWidth, itemHeight, gap) for the given width and mode.</summary>
    public static (int Columns, int ItemWidth, int ItemHeight, int Gap) GetGrid(int availableWidth, PosViewMode mode)
    {
        if (mode == PosViewMode.List)
        {
            int height = availableWidth < 768 ? 72 : (availableWidth < 1440 ? 64 : 56);
            return (1, Math.Max(1, availableWidth), height, 0);
        }

        // Cards mode
        int columns;
        int itemHeight;
        if (availableWidth < 768)
        {
            columns = 1;
            itemHeight = 104;
        }
        else if (availableWidth < 1440)
        {
            columns = 2;
            itemHeight = 96;
        }
        else
        {
            columns = 4;
            itemHeight = 112;
        }

        int gap = CardGap;
        int totalGap = gap * (columns - 1);
        int itemWidth = (availableWidth - totalGap) / columns;
        if (itemWidth < 120) { columns = 1; itemWidth = availableWidth; }

        return (columns, itemWidth, itemHeight, gap);
    }

    /// <summary>
    /// Calculates the sub-rectangles inside a single card of the given size.
    /// Coordinates are relative to the card's top-left corner.
    /// </summary>
    public static PosCardRects Calculate(int cardWidth, int cardHeight, bool rtl)
    {
        int imgSize = cardWidth < 200 ? ImageSizeNarrow
                    : cardWidth < 320 ? ImageSizeMedium
                    : ImageSizeWide;

        int y = CardPadding;

        if (rtl)
        {
            // RTL: image on right, text on left
            int imgX = cardWidth - imgSize - CardPadding;
            var imageRect = new Rectangle(imgX, y, imgSize, imgSize);

            int textX = CardPadding;
            int textWidth = cardWidth - imgSize - CardPadding * 3;
            if (textWidth < 40) textWidth = 40;

            int nameHeight = 36;
            var nameRect = new Rectangle(textX, y, textWidth, nameHeight);

            int skuY = y + nameHeight + 2;
            var skuRect = new Rectangle(textX, skuY, textWidth, 16);

            int priceWidth = 80;
            int priceX = CardPadding;
            int priceY = cardHeight - 24 - CardPadding;
            var priceRect = new Rectangle(priceX, priceY, priceWidth, 20);

            int badgeWidth = 48;
            int badgeX = priceX + priceWidth + 4;
            var badgeRect = new Rectangle(badgeX, priceY, badgeWidth, 20);

            int btnSize = 28;
            int btnX = cardWidth - btnSize - CardPadding;
            int btnY = cardHeight - btnSize - CardPadding;
            var btnRect = new Rectangle(btnX, btnY, btnSize, btnSize);

            return new PosCardRects(imageRect, nameRect, skuRect, priceRect, badgeRect, btnRect);
        }
        else
        {
            // LTR: image on left, text on right
            int imgX = CardPadding;
            var imageRect = new Rectangle(imgX, y, imgSize, imgSize);

            int textX = imgX + imgSize + CardPadding;
            int textWidth = cardWidth - imgSize - CardPadding * 3;
            if (textWidth < 40) textWidth = 40;

            int nameHeight = 36;
            var nameRect = new Rectangle(textX, y, textWidth, nameHeight);

            int skuY = y + nameHeight + 2;
            var skuRect = new Rectangle(textX, skuY, textWidth, 16);

            int priceWidth = 80;
            int priceX = cardWidth - priceWidth - CardPadding;
            int priceY = cardHeight - 24 - CardPadding;
            var priceRect = new Rectangle(priceX, priceY, priceWidth, 20);

            int badgeWidth = 48;
            int badgeX = priceX - badgeWidth - 4;
            var badgeRect = new Rectangle(badgeX, priceY, badgeWidth, 20);

            int btnSize = 28;
            int btnX = CardPadding;
            int btnY = cardHeight - btnSize - CardPadding;
            var btnRect = new Rectangle(btnX, btnY, btnSize, btnSize);

            return new PosCardRects(imageRect, nameRect, skuRect, priceRect, badgeRect, btnRect);
        }
    }

    /// <summary>Validates that all sub-rects fit within the card bounds.</summary>
    public static bool IsValid(PosCardRects rects, int cardWidth, int cardHeight)
    {
        return rects.Image.Right <= cardWidth && rects.Image.Bottom <= cardHeight
            && rects.Name.Right <= cardWidth && rects.Name.Bottom <= cardHeight
            && rects.Price.Right <= cardWidth && rects.Price.Bottom <= cardHeight
            && rects.Badge.Right <= cardWidth && rects.Badge.Bottom <= cardHeight
            && rects.AddButton.Right <= cardWidth && rects.AddButton.Bottom <= cardHeight;
    }
}

public record PosCardRects(
    Rectangle Image,
    Rectangle Name,
    Rectangle Sku,
    Rectangle Price,
    Rectangle Badge,
    Rectangle AddButton);
