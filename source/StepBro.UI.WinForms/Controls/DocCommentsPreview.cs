using CommandLine.Text;
using Markdig;
using Microsoft.Web.WebView2.Core;
using StepBro.Core.DocCreation;
using StepBro.Core.General;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using static StepBro.Core.DocCreation.ScriptDocumentation;

namespace StepBro.UI.WinForms.Controls
{
    public partial class DocCommentsPreview : UserControl
    {
        private ILoadedFilesManager m_loadedFilesManager = null;
        private List<string> m_currentDocComments = null;

        public DocCommentsPreview()
        {
            InitializeComponent();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.NavigateToString("<html><body><p>ready</p></body></html>");
            m_loadedFilesManager = StepBro.Core.Main.GetService<ILoadedFilesManager>();
        }

        public void Update(IFileElement element, List<string> docComments)
        {
            // Only update if data is different.
            if (m_currentDocComments == null || docComments.Count != m_currentDocComments.Count || !m_currentDocComments.SequenceEqual(docComments, StringComparer.InvariantCulture))
            {
                m_currentDocComments = new List<string>(docComments);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<!DOCTYPE html>");
                sb.AppendLine("<body>");

                string helpMarkdownContent = StepBro.Core.DocCreation.ScriptDocumentation.CreateFileElementDocumentation(
                    m_loadedFilesManager,
                    element,
                    docComments.Select(c => new Tuple<DocCommentLineType, string>(ScriptDocumentation.GetLineType(c), c)).ToList());

                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                sb.AppendLine(Markdown.ToHtml(helpMarkdownContent, pipeline));

                sb.AppendLine("</body>");
                sb.AppendLine("</html>");
                var zoom = webView.ZoomFactor;
                webView.CoreWebView2.NavigateToString(sb.ToString());
                webView.ZoomFactor = zoom;
            }
        }
    }
}
