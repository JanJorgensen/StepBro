using CommandLine.Text;
using Markdig;
using Microsoft.Web.WebView2.Core;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using StepBro.Core.Data;
using StepBro.Core.DocCreation;
using StepBro.Core.General;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Windows.Media;
using static StepBro.Core.Data.PropertyBlockDecoder;
using static StepBro.Core.DocCreation.ScriptDocumentation;

namespace StepBro.UI.WinForms.Controls
{
    public partial class DocCommentsPreview : UserControl
    {
        private enum Mode { Empty, ObjectOrSymbol, ScriptDocPreview }
        private ILoadedFilesManager m_loadedFilesManager = null;
        private ISymbolLookupService m_symbolLookupService = null;
        private Mode m_mode = Mode.Empty;
        private object m_shownObject = null;
        private List<string> m_currentDocComments = null;
        private bool m_coreIsSetup = false;


        public DocCommentsPreview()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var task = webView.EnsureCoreWebView2Async();
            //task.Wait(5000);
            //webView.CoreWebView2.NavigateToString("<html><body><p>ready</p></body></html>");
            m_loadedFilesManager = StepBro.Core.Main.GetService<ILoadedFilesManager>();
            m_symbolLookupService = StepBro.Core.Main.GetService<ISymbolLookupService>();
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
        }

        private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            webView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
        }

        private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            try
            {
                if (e.IsUserInitiated)
                {
                    e.Cancel = true;
                    if (e.Uri.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.RequestOpenBrowser(e.Uri);
                    }
                    else if (e.Uri.StartsWith("type:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var typeName = e.Uri.Substring(e.Uri.IndexOf("://") + 3);
                        var type = m_symbolLookupService.TryResolveSymbol(typeName);
                        if (type != null && type is System.Type)
                        {
                            this.ShowMarkdown(TypeDocumentation.CreateDoc(1, type as System.Type, true));
                        }
                        else
                        {
                            // TODO: show error
                        }
                    }
                    else if (e.Uri.StartsWith("method:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var methodName = e.Uri.Substring(e.Uri.IndexOf("://") + 3);
                        string hash = null;
                        if (methodName.Contains('-'))
                        {
                            hash = methodName.Substring(methodName.IndexOf("-") + 1);
                            methodName = methodName.Substring(0, methodName.IndexOf("-"));
                        }
                        var method = m_symbolLookupService.TryResolveSymbol(methodName);
                        if (method != null && method is MethodInfo)
                        {
                            this.ShowMarkdown(TypeDocumentation.CreateDoc(1, method as MethodInfo, true));
                        }
                        else
                        {
                            // TODO: show error
                        }
                    }
                    else if (e.Uri.StartsWith("property:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var propertyName = e.Uri.Substring(e.Uri.IndexOf("://") + 3);
                        var property = m_symbolLookupService.TryResolveSymbol(propertyName);
                        if (property != null && property is PropertyInfo)
                        {
                            this.ShowMarkdown(TypeDocumentation.CreateDoc(1, property as PropertyInfo, true));
                        }
                        else
                        {
                            // TODO: show error
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"### Exception thrown: {ex.GetType().FullName}");
                sb.AppendLine(ex.Message);
                this.ShowMarkdown(sb.ToString());
            }
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            if (e.IsUserInitiated)
            {
                e.Handled = true;
                this.RequestOpenBrowser(e.Uri);
            }
        }

        private void RequestOpenBrowser(string uri)
        {
            System.Diagnostics.Debug.WriteLine("RequestOpenBrowser - " + uri);

            if (uri.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo() { FileName = uri, UseShellExecute = true });
                }
                catch (System.ComponentModel.Win32Exception noBrowser)
                {
                    if (noBrowser.ErrorCode == -2147467259)
                        MessageBox.Show(noBrowser.Message);
                }
                catch (System.Exception other)
                {
                    MessageBox.Show(other.Message);
                }
            }
        }

        public void ShowObjectDocumentation(object @object)
        {
            if (@object != null && (m_mode != Mode.ObjectOrSymbol || !Object.ReferenceEquals(@object, m_shownObject)))
            {
                m_mode = Mode.ObjectOrSymbol;
                m_shownObject = @object;

                StringBuilder sb = new StringBuilder();
                if (@object is System.Type type)
                {
                    sb.AppendLine(TypeDocumentation.CreateDoc(1, type, true));
                }
                else if (@object is IFileElement element)
                {
                    sb.AppendLine(
                        ScriptDocumentation.CreateFileElementDocumentation(
                            1,
                            StepBro.Core.Main.GetService<ILoadedFilesManager>(),
                            element));
                    if (element is IFileVariable var)
                    {
                    }
                }
                else if (@object is IValueContainer container)
                {
                    sb.AppendLine("# " + container.Name);
                    sb.AppendLine("variable </br>");
                    sb.AppendLine("Type: [" + container.DataType.Type.TypeNameSimple() + "](www.google.com)</br>");
                    sb.AppendLine(TypeDocumentation.CreateDoc(2, container.DataType.Type, false));
                    //this.CreateValueContainerInfo(container, sb);
                }
                else if (@object is MethodInfo method)
                {
                    sb.AppendLine($"# {method.DeclaringType.TypeName()}.{method.Name} method");
                }
                else if (@object is PropertyInfo property)
                {
                    sb.AppendLine($"# {property.DeclaringType.TypeName()}.{property.Name} property");
                }
                this.ShowMarkdown(sb.ToString());
            }
            //if (m_mode != Mode.DotNetType || !Object.ReferenceEquals(m_shownObject, type))
            //{

            //}
        }

        private void CreateValueContainerInfo(IValueContainer container, StringBuilder sb)
        {
            sb.AppendLine($"#### Variable value/state");
            var variableObject = container.Object;
            if (variableObject != null)
            {
                if (variableObject.GetType().IsClass)
                {
                    var props = new List<Tuple<string, string>>();
                    foreach (var prop in variableObject.GetType().GetProperties())
                    {
                        try
                        {
                            string value = StringUtils.ObjectToString(prop.GetValue(variableObject));
                            props.Add(new Tuple<string, string>(prop.Name, value));
                            //sb.AppendLine($"| {prop.Name} | {value} |");
                            //sb.AppendLine("+-------+--------+");
                        }
                        catch { }
                    }
                    int nameWidth = "Property".Length;
                    int valueWidth = "Property".Length;
                    foreach (var prop in props)
                    {
                        nameWidth = Math.Max(nameWidth, prop.Item1.Length);
                        valueWidth = Math.Max(valueWidth, prop.Item2.Length);
                    }
                    //sb.AppendLine("+-------+--------+");
                    //sb.AppendLine("| Name  | Value  |");
                    //sb.AppendLine("+=======+========+");
                    //sb.AppendLine("| Olsen | 123456 |");
                    //sb.AppendLine("+-------+--------+");
                    sb.AppendLine($"+--{new String('-', nameWidth)}+--{new String('-', valueWidth)}+");
                    sb.AppendLine($"| Property{new String(' ', nameWidth - "Property".Length)} | Value{new String(' ', valueWidth - "Property".Length)} |");
                    sb.AppendLine($"+=={new String('=', nameWidth)}+=={new String('=', valueWidth)}+");

                    foreach (var prop in props)
                    {
                        sb.AppendLine($"| {prop.Item1}{new String(' ', nameWidth - prop.Item1.Length)} | {prop.Item2}{new String(' ', valueWidth - prop.Item2.Length)} |");
                        sb.AppendLine($"+--{new String('-', nameWidth)}+--{new String('-', valueWidth)}+");
                    }

                    //sb.AppendLine("| -------- | ----- |");
                    //sb.AppendLine("| Property | Value |");
                    //sb.AppendLine("| -------- | ----- |");
                    //sb.AppendLine("| Petersen | 12 |");
                    //sb.AppendLine("| -------- | ----- |");
                    //sb.AppendLine("| Frederiksen | Andusien |");
                    //sb.AppendLine("| -------- | ----- |");
                }
                else
                {

                }
            }
            else
            {
                sb.AppendLine("_Value is not set (==null==)_");
            }
        }

        public void ShowDocCommentsPreview(List<string> docComments)
        {
            // Only update if data is different.
            if (m_mode != Mode.ScriptDocPreview || m_currentDocComments == null || docComments.Count != m_currentDocComments.Count || !m_currentDocComments.SequenceEqual(docComments, StringComparer.InvariantCulture))
            {
                m_mode = Mode.ScriptDocPreview;
                m_currentDocComments = new List<string>(docComments);

                string helpMarkdownContent = StepBro.Core.DocCreation.ScriptDocumentation.CreateFileElementDocumentation(
                    1,
                    m_loadedFilesManager,
                    null,
                    docComments.Select(c => new Tuple<DocCommentLineType, string>(ScriptDocumentation.GetLineType(c), c)).ToList());

                this.ShowMarkdown(helpMarkdownContent);
            }
        }

        private void ShowMarkdown(string markdownText)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<body>");

            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseGridTables().Build();
            sb.AppendLine(Markdown.ToHtml(markdownText, pipeline));

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            var zoom = webView.ZoomFactor;
            webView.CoreWebView2.NavigateToString(sb.ToString());
            webView.ZoomFactor = zoom;
        }
    }
}
