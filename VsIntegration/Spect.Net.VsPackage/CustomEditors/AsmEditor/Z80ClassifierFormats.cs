﻿using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Spect.Net.VsPackage.CustomEditors.AsmEditor
{
    /// <summary>
    /// Defines an editor format for a Z80 assembly label
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "Z80Label")]
    [Name("Z80Label")]
    [UserVisible(true)] 
    [Order(Before = Priority.Default)]
    internal sealed class Z80LabelClassifierFormat : ClassificationFormatDefinition
    {
        public Z80LabelClassifierFormat()
        {
            DisplayName = "Z80 Asm - Label";
            ForegroundColor = Colors.DarkOrange;
        }
    }

    /// <summary>
    /// Defines an editor format for a Z80 assembly pragma
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "Z80Pragma")]
    [Name("Z80Pragma")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class Z80PragmaClassifierFormat : ClassificationFormatDefinition
    {
        public Z80PragmaClassifierFormat()
        {
            DisplayName = "Z80 Asm - Pragma";
            ForegroundColor = Colors.CadetBlue;
        }
    }

    /// <summary>
    /// Defines an editor format for a Z80 assembly directive
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "Z80Directive")]
    [Name("Z80Directive")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class Z80DirectiveClassifierFormat : ClassificationFormatDefinition
    {
        public Z80DirectiveClassifierFormat()
        {
            DisplayName = "Z80 Asm - Directive";
            ForegroundColor = Colors.LightGray;
        }
    }

    /// <summary>
    /// Defines an editor format for a Z80 assembly instruction
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "Z80Instruction")]
    [Name("Z80Instruction")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class Z80InstructionClassifierFormat : ClassificationFormatDefinition
    {
        public Z80InstructionClassifierFormat()
        {
            DisplayName = "Z80 Asm - Instruction";
            ForegroundColor = Colors.NavajoWhite;
            IsBold = true;
        }
    }

    /// <summary>
    /// Defines an editor format for a Z80 assembly comment
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "Z80Comment")]
    [Name("Z80Comment")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class Z80CommentClassifierFormat : ClassificationFormatDefinition
    {
        public Z80CommentClassifierFormat()
        {
            DisplayName = "Z80 Asm - Comment";
            ForegroundColor = Colors.LimeGreen;
            IsBold = true;
        }
    }

    /// <summary>
    /// Defines an editor format for a Z80 assembly number
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "Z80Number")]
    [Name("Z80Number")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class Z80NumberClassifierFormat : ClassificationFormatDefinition
    {
        public Z80NumberClassifierFormat()
        {
            DisplayName = "Z80 Asm - Number";
            ForegroundColor = Colors.Cyan;
        }
    }

    /// <summary>
    /// Defines an editor format for a Z80 assembly number
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "Z80Identifier")]
    [Name("Z80Identifier")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class Z80IdentifierClassifierFormat : ClassificationFormatDefinition
    {
        public Z80IdentifierClassifierFormat()
        {
            DisplayName = "Z80 Asm - Identifier";
            ForegroundColor = Colors.DarkCyan;
        }
    }
}
