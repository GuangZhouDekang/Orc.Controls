﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RichTextBoxExtensions.cs" company="WildGums">
//   Copyright (c) 2008 - 2018 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.Controls
{
    using System.Linq;
    using System.Text;
    using System.Windows.Controls;
    using System.Windows.Documents;

    public static class RichTextBoxExtensions
    {
        #region Methods
        public static string GetInlineText(this RichTextBox richTextBox)
        {
            var sb = new StringBuilder();

            foreach (var block in richTextBox.Document.Blocks.OfType<Paragraph>())
            {
                foreach (var inline in block.Inlines.OfType<Run>())
                {
                    sb.AppendLine(inline.Text);
                }
            }

            return sb.ToString();
        }
        #endregion
    }
}
