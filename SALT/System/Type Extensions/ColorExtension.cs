using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
namespace System.Drawing
{
    public static class ColorExtension
    {
        public static Color DrawingColor(this ConsoleColor consoleColor)
        {
            switch (consoleColor)
            {
                case ConsoleColor.Black:
                    return Color.Black;

                case ConsoleColor.Blue:
                    return Color.Blue;

                case ConsoleColor.Cyan:
                    return Color.Cyan;

                case ConsoleColor.DarkBlue:
                    return ColorTranslator.FromHtml("#000080");

                case ConsoleColor.DarkGray:
                    return ColorTranslator.FromHtml("#808080");

                case ConsoleColor.DarkGreen:
                    return ColorTranslator.FromHtml("#008000");

                case ConsoleColor.DarkMagenta:
                    return ColorTranslator.FromHtml("#800080");

                case ConsoleColor.DarkRed:
                    return ColorTranslator.FromHtml("#800000");

                case ConsoleColor.DarkYellow:
                    return ColorTranslator.FromHtml("#808000");

                case ConsoleColor.Gray:
                    return ColorTranslator.FromHtml("#C0C0C0");

                case ConsoleColor.Green:
                    return ColorTranslator.FromHtml("#00FF00");

                case ConsoleColor.Magenta:
                    return Color.Magenta;

                case ConsoleColor.Red:
                    return Color.Red;

                case ConsoleColor.White:
                    return Color.White;

                default:
                    return Color.Green;
            }
        }
    }
}