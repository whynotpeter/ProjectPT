//using System;
//using System.Text;
//using System.Windows.Forms;
//using ScintillaNET;

//namespace Utilities
//{
//    public class HexEditor : Scintilla
//    {
//        public HexEditor()
//        {
//            Caret.CurrentLineBackgroundColor = System.Drawing.Color.LemonChiffon;
//            Caret.HighlightCurrentLine = true;
//            LineWrapping.IndentSize = 3;
//            //LineWrapping.Mode = LineWrappingMode.Word;
//            //LineWrapping.VisualFlags = LineWrappingVisualFlags.End | LineWrappingVisualFlags.Start;
//            Margins.Margin0.Width = 30;
//            Styles.BraceBad.FontName = "Verdana\0";
//            Styles.BraceBad.Size = 9F;
//            Styles.BraceLight.FontName = "Verdana\0";
//            Styles.BraceLight.Size = 9F;
//            Styles.ControlChar.FontName = "Verdana\0";
//            Styles.ControlChar.Size = 9F;
//            Styles.Default.BackColor = System.Drawing.SystemColors.Window;
//            Styles.Default.FontName = "Verdana\0";
//            Styles.Default.Size = 9F;
//            Styles.IndentGuide.FontName = "Verdana\0";
//            Styles.IndentGuide.Size = 9F;
//            Styles.LastPredefined.FontName = "Verdana\0";
//            Styles.LastPredefined.Size = 9F;
//            Styles.LineNumber.FontName = "Verdana\0";
//            Styles.LineNumber.Size = 9F;
//            Styles.Max.FontName = "Verdana\0";
//            Styles.Max.Size = 9F;
//            TabIndex = 2;
//            Whitespace.ForeColor = System.Drawing.Color.Purple;
//            Whitespace.Mode = WhitespaceMode.VisibleAlways;
//            KeyPress += this.keyPressed;

//            /*
//             * E6 - czerwona
//             * A5 - żółta
//             * D4 - niebieska
//             * G3 - pomarańczowa
//             * H2 - zielona
//             * e1 - fioletowa
//             */
//        }

//        private void keyPressed(object sender, KeyPressEventArgs e)
//        {
//            if (char.IsControl(e.KeyChar) || this.IsHex(e.KeyChar) || char.IsWhiteSpace(e.KeyChar))
//            {
//                if ((Keys)e.KeyChar != Keys.Back && (Keys)e.KeyChar != Keys.Enter)
//                {
//                    e.KeyChar = char.ToUpper(e.KeyChar);
//                    this.SetText(e.KeyChar);
//                }
//                e.Handled = true;
//            }
//            else
//                e.Handled = true;
//        }

//        private bool IsHex(char pressed)
//        {
//            string test = "" + pressed;
//            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-9a-fA-F]+\b\Z");
//        }

//        private void SetText(char c)
//        {
//            int selectionStart = Selection.Start;
//            int nrOfLine = GetLineFromCharIndex(selectionStart);
//            int indexOfFirstLetterInLine = GetFirstCharIndexFromLine(nrOfLine);
//            var stringBuilder = new StringBuilder(Lines[nrOfLine]);
//            string newString = "";

//            string[] lines = new string[Lines.Length];
//            for (int i = 0; i < Lines.Length; i++) Lines[i] = Lines[i];
//            if ((selectionStart + 1 - indexOfFirstLetterInLine) % 3 == 0) selectionStart++;
//            if (selectionStart - indexOfFirstLetterInLine < stringBuilder.Length)
//            {

//                if (selectionStart < indexOfFirstLetterInLine) indexOfFirstLetterInLine = selectionStart;
//                stringBuilder[selectionStart - indexOfFirstLetterInLine] = c;
//                selectionStart++;
//                for (int i = 0; i < stringBuilder.Length; i++)
//                {
//                    newString += stringBuilder[i];
//                }
//                Lines[nrOfLine] = newString;
//                Lines = lines;
//                Selection.Start = selectionStart;
//            }
//        }
//    }
//}