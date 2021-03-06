﻿using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Tagging.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.SyntaxEditorSupport
{
    public class StepBroTokenTaggerProvider : TaggerProviderBase<StepBroTokenTagger>, ICodeDocumentTaggerProvider
    {
        private IStepBroClassificationTypeProvider classificationTypeProviderValue;

        /// <summary>
        /// Initializes a new instance of the <c>SyntaxEditorStepBroTokenTaggerProvider</c> class.
        /// </summary>
        /// <param name="classificationTypeProvider">A <see cref="IStepBroClassificationTypeProvider"/> that provides classification types.</param>
        public StepBroTokenTaggerProvider(IStepBroClassificationTypeProvider classificationTypeProvider)
        {
            if ((classificationTypeProvider == null))
            {
                throw new ArgumentNullException("classificationTypeProvider");
            }

            // Initialize
            this.classificationTypeProviderValue = classificationTypeProvider;
        }

        /// <summary>
        /// Returns a tagger for the specified <see cref="ICodeDocument"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ITag"/> created by the tagger.</typeparam>
        /// <param name="document">The <see cref="ICodeDocument"/> that requires a tagger.</param>
        /// <returns>A tagger for the specified <see cref="ICodeDocument"/>.</returns>
        public ITagger<T> GetTagger<T>(ICodeDocument document)
            where T : ITag
        {
            if (typeof(ITagger<T>).IsAssignableFrom(typeof(StepBroTokenTagger)))
            {
                TaggerFactory factory = new TaggerFactory(this, document);
                return ((ITagger<T>)(document.Properties.GetOrCreateSingleton(typeof(ITagger<ITokenTag>), new ActiproSoftware.Text.Utility.PropertyDictionary.Creator<StepBroTokenTagger>(factory.CreateTagger))));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Implements a factory that can creates <see cref="SimpleTokenTagger"/> objects for a document.
        /// </summary>
        public class TaggerFactory
        {
            private ICodeDocument documentValue;

            private StepBroTokenTaggerProvider providerValue;

            /// <summary>
            /// Initializes a new instance of the <c>TaggerFactory</c> class.
            /// </summary>
            /// <param name="provider">The owner <see cref="SimpleTokenTaggerProvider"/>.</param>
            /// <param name="document">The <see cref="ICodeDocument"/> that requires an <see cref="SimpleTokenTagger"/>.</param>
            internal TaggerFactory(StepBroTokenTaggerProvider provider, ICodeDocument document)
            {
                // Initialize
                this.providerValue = provider;
                this.documentValue = document;
            }

            /// <summary>
            /// Creates an <see cref="SimpleTokenTagger"/> for the <see cref="ICodeDocument"/>.
            /// </summary>
            /// <returns>An <see cref="SimpleTokenTagger"/> for the <see cref="ICodeDocument"/>.</returns>
            public StepBroTokenTagger CreateTagger()
            {
                return new StepBroTokenTagger(this.documentValue, this.providerValue.classificationTypeProviderValue);
            }
        }
    }
}