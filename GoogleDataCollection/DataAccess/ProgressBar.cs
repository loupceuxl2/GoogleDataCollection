using System;
using System.Diagnostics;
using System.Linq;

namespace GoogleDataCollection.DataAccess
{
    // REFERENCE: https://msdn.microsoft.com/en-us/library/system.console.cursorleft.aspx
    public class ProgressBar
    {
        //public static int DefaultCounter = 0;

        protected int OriginalLeft { get; set; }
        protected int OriginalTop { get; set; }
        protected int TotalValues { get; set; }
        protected int Counter { get; set; }
        protected char Character { get; set; }
        protected int Ratio { get; set; }
        protected int Padding { get; set; }

        public ProgressBar(int totalValues, char character = '█', int ratio = 1000, int padding = 3)
        {
            Console.Clear();
            OriginalLeft = 0;
            OriginalTop = 0;
            Counter = 0;
            Character = character;
            Ratio = ratio;
            Padding = padding;
            TotalValues = totalValues;
        }

        public void CheckProgress(int currentIndex)
        {
            if (!HasMadeProgress(currentIndex)) { return; }

            Write();
        }

        private bool HasMadeProgress(int currentIndex)
        {
            Debug.WriteLine($"PROGRESS: {TotalValues / Ratio * Counter}.");
            return currentIndex >= TotalValues / Ratio * Counter;
        }

        private void Write()
        {
            try
            {
                //Console.SetCursorPosition(origCol + x, origRow + y);
                //var originalLeft = Console.CursorLeft;

                Debug.WriteLine($"LEFT: {OriginalLeft + Counter}");
                Console.SetCursorPosition(OriginalLeft + Counter, OriginalTop);
                Console.Write(string.Concat(Enumerable.Repeat(Character, Counter)));

                //Console.SetCursorPosition(OriginalLeft + Ratio + Padding, OriginalTop);
                //Console.Write($"{Counter}%");
                Counter++;
                //Console.CursorLeft += Counter;

            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}
