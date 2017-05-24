using System;
using System.Linq;

namespace GoogleDataCollection.DataAccess
{
    // REFERENCE: https://msdn.microsoft.com/en-us/library/system.console.cursorleft.aspx
    public static class DataProgress
    {
        public static int Counter = 0;
        public static char DisplayCharacter = '█';
        public static int DisplayRatio = 10;
        public static int Padding = 3;

        public static void Write()
        {
            try
            {
                //Console.SetCursorPosition(origCol + x, origRow + y);
                //var originalLeft = Console.CursorLeft;

                Console.SetCursorPosition((Counter), Console.CursorTop);
                Console.Write(String.Concat(Enumerable.Repeat(DisplayCharacter, Counter)));

                Console.SetCursorPosition((DisplayRatio + Padding), Console.CursorTop);
                Console.Write($"{Counter}%");
                //Console.CursorLeft += Counter;
                
            }
            catch (ArgumentOutOfRangeException e)
            {
                ResetCursor();
                Console.WriteLine(e.Message);
            }
        }

        public static void ResetCursor()
        {
            //Console.Clear();
            Console.CursorLeft = 0;
            Console.CursorTop += 1;
        }
    }
}
