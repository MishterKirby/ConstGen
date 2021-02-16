using System.Text;
using System;

namespace ConstGen
{
    /// <summary>
    /// Method extensions for writing the code into the content
    /// </summary>
    public static class ContentWriter
    {
        /// <summary>tab == 4spaces</summary>
        public const string TabRepresentation = "    ";

        // =======================================================================================================================

        /// <summary>
        /// Appends/adds (Writes) indents to the stringbuilder content then the code text in the end
        /// </summary>
        /// <param name="self">StringBuilder target</param>
        /// <param name="indentCount">number of indents to add before the text</param>
        /// <param name="text">The text to be appended after the indents</param>
        public static void WriteIndentedLine(this StringBuilder self, int indentCount, string text)
        {
            self.WriteIndented(indentCount, text);
            self.Append(Environment.NewLine);
        }

        private static void WriteIndented(this StringBuilder self, int indentCount, string text)
        {
            // e if you use AppendFormat in this context, format will be broken if there is single '{' or '}' in the string. so implementations must be separated.
            
            // add the number of indents
            for (int i = 0; i < indentCount; i++) {
                self.Append(TabRepresentation);
            }

            // then add the text 
            self.Append(text);
        }

        // =======================================================================================================================

        /// <summary>
        /// Appends/adds (Writes) indents to the stringbuilder content then the code text in the end
        /// in a specified format
        /// </summary>
        /// <param name="self">StringBuilder target</param>
        /// <param name="indentCount">number of indents to add</param>
        /// <param name="text">The text added after the indents</param>
        /// <param name="param">the string variables or string values representing the indexs from the text</param>
        public static void WriteIndentedFormatLine(this StringBuilder self, int indentCount, string text, params object[] param)
        {
            self.WriteFormatted(indentCount, text, param);
            self.Append(Environment.NewLine);
        }

        private static void WriteFormatted(this StringBuilder self, int indentCount, string text, params object[] param)
        {
             // add the number of indents
            for (int i = 0; i < indentCount; i++) {
                self.Append(TabRepresentation);
            }

            // then add the text 
            self.AppendFormat(text, param);
        }

        // =======================================================================================================================

        public static void WriteImports(this StringBuilder self, params string[] param)
        {
            for (int i = 0; i < param.Length; i++)
            {
                self.Append( "using " + param[i] + ";" );
                self.Append(Environment.NewLine);
            }
            self.Append(Environment.NewLine);
        }
    }
}
