using System;

namespace DeCorrespondent.Impl
{
    public class ColoredConsole : IDisposable
    {
        public ColoredConsole(ConsoleColor? foregroundColor, ConsoleColor? backgroundColor = null)
        {
            if (foregroundColor.HasValue)
                Console.ForegroundColor = foregroundColor.Value;
            if (backgroundColor.HasValue)
                Console.BackgroundColor = backgroundColor.Value;
        }

        public void Dispose()
        {
            Console.ResetColor();
        }
    }
}
