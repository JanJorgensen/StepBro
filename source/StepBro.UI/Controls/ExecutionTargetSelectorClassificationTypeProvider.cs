using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActiproSoftware.Text;
using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting.Implementation;
using System.Windows.Media;

namespace StepBro.UI.Controls
{
    public partial class ExecutionTargetSelectorClassificationTypeProvider : IExecutionTargetSelectorClassificationTypeProvider
    {

        private IHighlightingStyleRegistry registryValue;
        private IClassificationType delimiterValue;
        private IClassificationType mainIdentifierValue;
        private IClassificationType subIdentifierValue;
        private IClassificationType identifierValue;
        private IClassificationType valueValue;
        private IClassificationType stringValue;

        public ExecutionTargetSelectorClassificationTypeProvider() :
                this(null)
        {
        }

        public ExecutionTargetSelectorClassificationTypeProvider(IHighlightingStyleRegistry targetRegistry)
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

        public IClassificationType MainIdentifier
        {
            get
            {
                if ((this.mainIdentifierValue == null))
                {
                    String key = "MainIdentifier";
                    this.mainIdentifierValue = this.registryValue.GetClassificationType(key);
                    if ((this.mainIdentifierValue == null))
                    {
                        this.mainIdentifierValue = new ClassificationType(key, "Identifier");
                        this.registryValue.Register(this.mainIdentifierValue, new HighlightingStyle());
                    }
                }
                return this.mainIdentifierValue;
            }
        }

        public IClassificationType SubIdentifier
        {
            get
            {
                if ((this.subIdentifierValue == null))
                {
                    String key = "SubIdentifier";
                    this.subIdentifierValue = this.registryValue.GetClassificationType(key);
                    if ((this.subIdentifierValue == null))
                    {
                        this.subIdentifierValue = new ClassificationType(key, "SubIdentifier");
                        this.registryValue.Register(this.subIdentifierValue, new HighlightingStyle());
                    }
                }
                return this.subIdentifierValue;
            }
        }

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
        /// Gets the <c>Number</c> classification type.
        /// </summary>
        /// <value>The <c>Number</c> classification type.</value>
        public IClassificationType Value
        {
            get
            {
                if ((this.valueValue == null))
                {
                    String key = "Value";
                    this.valueValue = this.registryValue.GetClassificationType(key);
                    if ((this.valueValue == null))
                    {
                        this.valueValue = new ClassificationType(key, "Value");
                        this.registryValue.Register(this.valueValue, new HighlightingStyle());
                    }
                }
                return this.valueValue;
            }
        }

        /// <summary>
        /// Gets the <c>Operator</c> classification type.
        /// </summary>
        /// <value>The <c>Operator</c> classification type.</value>
        public IClassificationType String
        {
            get
            {
                if ((this.stringValue == null))
                {
                    String key = "String";
                    this.stringValue = this.registryValue.GetClassificationType(key);
                    if ((this.stringValue == null))
                    {
                        this.stringValue = new ClassificationType(key, "String");
                        this.registryValue.Register(this.stringValue, new HighlightingStyle());
                    }
                }
                return this.stringValue;
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
                    Delimiter,
                    MainIdentifier,
                    SubIdentifier,
                    Identifier,
                    Value,
                    String};
        }
    }
}
