﻿using ActiproSoftware.Text;
using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StepBro.Core.Parser
{
    public partial class SyntaxEditorStepBroClassificationTypeProvider : ISyntaxEditorStepBroClassificationTypeProvider
    {

        private IHighlightingStyleRegistry registryValue;

        private IClassificationType commentValue;

        private IClassificationType delimiterValue;

        private IClassificationType identifierValue;

        private IClassificationType keywordValue;

        private IClassificationType numberValue;

        private IClassificationType stringLiteralValue;

        private IClassificationType operatorValue;

        /// <summary>
        /// Initializes a new instance of the <c>SyntaxEditorStepBroClassificationTypeProvider</c> class.
        /// </summary>
        public SyntaxEditorStepBroClassificationTypeProvider() :
                this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>SyntaxEditorStepBroClassificationTypeProvider</c> class.
        /// </summary>
        /// <param name="targetRegistry">The <see cref="IHighlightingStyleRegistry"/> to use when registering classification types and highlighting styles.</param>
        public SyntaxEditorStepBroClassificationTypeProvider(IHighlightingStyleRegistry targetRegistry)
        {
            if ((targetRegistry != null))
            {
                this.registryValue = targetRegistry;
            }
            else
            {
                this.registryValue = AmbientHighlightingStyleRegistry.Instance;
            }
        }

        /// <summary>
        /// Gets the <c>Comment</c> classification type.
        /// </summary>
        /// <value>The <c>Comment</c> classification type.</value>
        public IClassificationType Comment
        {
            get
            {
                if ((this.commentValue == null))
                {
                    String key = "Comment";
                    this.commentValue = this.registryValue.GetClassificationType(key);
                    if ((this.commentValue == null))
                    {
                        this.commentValue = new ClassificationType(key, "Comment");
                        this.registryValue.Register(this.commentValue, new HighlightingStyle(Color.FromArgb(255, 0, 128, 0)));
                    }
                }
                return this.commentValue;
            }
        }

        /// <summary>
        /// Gets the <c>Delimiter</c> classification type.
        /// </summary>
        /// <value>The <c>Delimiter</c> classification type.</value>
        public IClassificationType Delimiter
        {
            get
            {
                if ((this.delimiterValue == null))
                {
                    String key = "Delimiter";
                    this.delimiterValue = this.registryValue.GetClassificationType(key);
                    if ((this.delimiterValue == null))
                    {
                        this.delimiterValue = new ClassificationType(key, "Delimiter");
                        this.registryValue.Register(this.delimiterValue, new HighlightingStyle());
                    }
                }
                return this.delimiterValue;
            }
        }

        /// <summary>
        /// Gets the <c>Identifier</c> classification type.
        /// </summary>
        /// <value>The <c>Identifier</c> classification type.</value>
        public IClassificationType Identifier
        {
            get
            {
                if ((this.identifierValue == null))
                {
                    String key = "Identifier";
                    this.identifierValue = this.registryValue.GetClassificationType(key);
                    if ((this.identifierValue == null))
                    {
                        this.identifierValue = new ClassificationType(key, "Identifier");
                        this.registryValue.Register(this.identifierValue, new HighlightingStyle());
                    }
                }
                return this.identifierValue;
            }
        }

        /// <summary>
        /// Gets the <c>Keyword</c> classification type.
        /// </summary>
        /// <value>The <c>Keyword</c> classification type.</value>
        public IClassificationType Keyword
        {
            get
            {
                if ((this.keywordValue == null))
                {
                    String key = "Keyword";
                    this.keywordValue = this.registryValue.GetClassificationType(key);
                    if ((this.keywordValue == null))
                    {
                        this.keywordValue = new ClassificationType(key, "Keyword");
                        this.registryValue.Register(this.keywordValue, new HighlightingStyle(Color.FromArgb(255, 0, 0, 255)));
                    }
                }
                return this.keywordValue;
            }
        }

        /// <summary>
        /// Gets the <c>Number</c> classification type.
        /// </summary>
        /// <value>The <c>Number</c> classification type.</value>
        public IClassificationType Number
        {
            get
            {
                if ((this.numberValue == null))
                {
                    String key = "Number";
                    this.numberValue = this.registryValue.GetClassificationType(key);
                    if ((this.numberValue == null))
                    {
                        this.numberValue = new ClassificationType(key, "Number");
                        this.registryValue.Register(this.numberValue, new HighlightingStyle());
                    }
                }
                return this.numberValue;
            }
        }

        /// <summary>
        /// Gets the <c>StringLiteral</c> classification type.
        /// </summary>
        /// <value>The <c>StringLiteral</c> classification type.</value>
        public IClassificationType StringLiteral
        {
            get
            {
                if ((this.stringLiteralValue == null))
                {
                    String key = "StringLiteral";
                    this.stringLiteralValue = this.registryValue.GetClassificationType(key);
                    if ((this.stringLiteralValue == null))
                    {
                        this.stringLiteralValue = new ClassificationType(key, "StringLiteral");
                        this.registryValue.Register(this.stringLiteralValue, new HighlightingStyle(Color.FromArgb(255, 255, 0, 0)));
                    }
                }
                return this.stringLiteralValue;
            }
        }

        /// <summary>
        /// Gets the <c>Operator</c> classification type.
        /// </summary>
        /// <value>The <c>Operator</c> classification type.</value>
        public IClassificationType Operator
        {
            get
            {
                if ((this.operatorValue == null))
                {
                    String key = "Operator";
                    this.operatorValue = this.registryValue.GetClassificationType(key);
                    if ((this.operatorValue == null))
                    {
                        this.operatorValue = new ClassificationType(key, "Operator");
                        this.registryValue.Register(this.operatorValue, new HighlightingStyle(Color.FromArgb(255, 0, 0, 255)));
                    }
                }
                return this.operatorValue;
            }
        }

        /// <summary>
        /// Gets the <see cref="IHighlightingStyleRegistry"/> to use when registering classification types and highlighting styles.
        /// </summary>
        /// <value>The <see cref="IHighlightingStyleRegistry"/> to use when registering classification types and highlighting styles.</value>
        public IHighlightingStyleRegistry Registry
        {
            get
            {
                return this.registryValue;
            }
        }

        /// <summary>
        /// Registers all classification types and highlighting styles with the <see cref="IHighlightingStyleRegistry"/> used by this class.
        /// </summary>
        /// <returns>The collection of <see cref="IClassificationType"/> objects that were registered.</returns>
        public IEnumerable<IClassificationType> RegisterAll()
        {
            return new IClassificationType[] {
                    Comment,
                    Delimiter,
                    Identifier,
                    Keyword,
                    Number,
                    StringLiteral,
                    Operator};
        }
    }
}
