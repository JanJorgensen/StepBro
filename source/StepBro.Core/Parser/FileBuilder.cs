using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Tree;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.File;
using StepBro.Core.General;
using StepBro.Core.Language;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SBP = StepBro.Core.Parser.Grammar.StepBro;
using Lexer = StepBro.Core.Parser.Grammar.StepBroLexer;

namespace StepBro.Core.Parser
{
    public class FileBuilder
    {
        private readonly ErrorCollector m_errors;
        private SBP m_parser = null;
        private readonly StepBroListener m_listener = null;
        private readonly ScriptFile m_file = null;
        public static readonly AccessModifier DefaultAccess = AccessModifier.Public;
        private static FileBuilder m_lastInstance = null;
        private static StepBroListener m_lastListener = null;
        private static ErrorCollector m_lastErrors = null;

        internal FileBuilder(AntlrInputStream code, IAddonManager addons = null, ScriptFile file = null)
        {
            m_file = file;
            m_errors = new ErrorCollector(file, false);
            var lexer = new Grammar.StepBroLexer(code);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(m_errors);
            ITokenStream tokens = new CommonTokenStream(lexer);
            m_parser = new SBP(tokens);
            m_parser.RemoveErrorListeners();
            m_parser.AddErrorListener(m_errors);
#if DEBUG
            m_parser.Interpreter.PredictionMode = PredictionMode.LL_EXACT_AMBIG_DETECTION;
#endif
            m_parser.BuildParseTree = true;
            m_listener = new StepBroListener(m_errors, addons, file);
            m_lastInstance = this;
        }

        public static ServiceManager.IServiceManagerAdministration LastServiceManager { get; internal set; }
        public static FileBuilder LastInstance { get { return m_lastInstance; } }
        internal static StepBroListener LastListener { get { return m_lastListener; } }
        internal static ErrorCollector LastErrors { get { return m_lastErrors; } }

        internal static FileBuilder Create(string content, Type typeUsing = null, Type[] typeNamespaces = null)
        {
            var addons = AddonManager.Create();
            addons.AddAssembly(AddonManager.StepBroCoreAssembly, true);
            addons.AddAssembly(typeof(System.DateTime).Assembly, false);
            addons.AddAssembly(typeof(System.Math).Assembly, false);
            addons.AddAssembly(typeof(System.Console).Assembly, false);
            addons.AddAssembly(typeof(System.Linq.Enumerable).Assembly, false);

            var file = new ScriptFile();
            file.AddNamespaceUsing(-1, addons.Lookup(null, "System"));
            file.AddNamespaceUsing(-1, addons.Lookup(null, "System.Linq"));
            file.AddNamespaceUsing(-1, addons.Lookup(null, typeof(StepBro.Core.DataReport).Namespace));
            if (typeUsing != null)
            {
                addons.AddAssembly(typeUsing.Assembly, false);
                file.AddNamespaceUsing(-1, addons.Lookup(null, typeUsing.FullName));
            }
            if (typeNamespaces != null)
            {
                foreach (var u in typeNamespaces)
                {
                    addons.AddAssembly(u.Assembly, false);
                    file.AddNamespaceUsing(-1, addons.Lookup(null, u.Namespace));
                }
            }
            file.UpdateRootIdentifiers();

            return new FileBuilder(new AntlrInputStream(content), addons, file);
        }

        public IErrorCollector Errors { get { return m_errors; } }
        public SBP Parser { get { return m_parser; } }
        internal StepBroListener Listener { get { return m_listener; } }
        internal ScriptFile File { get { return m_file; } }

        internal static IEnumerable<string> GetTokens(string content)
        {
            var lexer = new Grammar.StepBroLexer(new AntlrInputStream(content));
            var tokens = lexer.GetAllTokens();
            foreach (var t in tokens) yield return Grammar.StepBroLexer.ruleNames[t.Type - 1];
        }

        internal static SBExpressionData ParseLiteral(string content)
        {
            ITokenSource lexer = new Grammar.StepBroLexer(new AntlrInputStream(content));
            ITokenStream tokens = new CommonTokenStream(lexer);
            var parser = new SBP(tokens);
            var errors = new ErrorCollector(null);
            parser.AddErrorListener(errors);
            parser.BuildParseTree = true;
            StepBroListener listener = new StepBroListener(errors);
            listener.PrepareForExpressionParsing("StepBroFileBuilder.ParseLiteral");
            var context = parser.literal();

            var walker = new ParseTreeWalker();
            walker.Walk(listener, context);

            return listener.GetExpressionResult();
        }

        internal static TypeReference ParseType(string content)
        {
            var builder = FileBuilder.Create(content);
            var context = builder.Parser.type();

            var walker = new ParseTreeWalker();
            walker.Walk(builder.Listener, context);

            return builder.Listener.LastParsedType;
        }

        internal static FileBuilder ParseExpression(string content)
        {
            return ParseExpression((Type)null, null, content);
        }

        internal static FileBuilder ParseExpression(Type typeUsing, Type[] typeNamespaces, string content)
        {
            var builder = FileBuilder.Create(content, typeUsing, typeNamespaces);
            builder.Listener.PrepareForExpressionParsing("StepBroFileBuilder.ParseExpression");
            var context = builder.Parser.expression();

            var walker = new ParseTreeWalker();
            walker.Walk(builder.Listener, context);

            return builder;
        }

        internal static FileBuilder ParseParExpression(Type typeUsing, Type[] typeNamespaces, string content)
        {
            var builder = FileBuilder.Create(content, typeUsing, typeNamespaces);
            builder.Listener.PrepareForExpressionParsing("StepBroFileBuilder.ParseParExpression");
            var context = builder.Parser.parExpression();

            var walker = new ParseTreeWalker();
            walker.Walk(builder.Listener, context);

            return builder;
        }

        internal static FileBuilder ParseExpression(ScriptFile fileContext, IAddonManager addons, string expression)
        {
            var builder = new FileBuilder(new AntlrInputStream(expression), addons, fileContext);
            builder.Listener.PrepareForExpressionParsing("StepBroFileBuilder.ParseExpression");
            var context = builder.Parser.expression();

            var walker = new ParseTreeWalker();
            walker.Walk(builder.Listener, context);

            return builder;
        }

        internal static Tuple<Stack<SBExpressionData>, IErrorCollector> ParseSimpleArguments(string expression)
        {
            var addons = AddonManager.Create();
            addons.AddAssembly(AddonManager.StepBroCoreAssembly, true);
            addons.AddAssembly(typeof(System.Math).Assembly, false);
            addons.AddAssembly(typeof(System.Linq.Enumerable).Assembly, false);
            //if (module != null) addons.AddAssembly(module, false);

            var file = new ScriptFile();
            file.AddNamespaceUsing(-1, addons.Lookup(null, "System"));
            file.AddNamespaceUsing(-1, addons.Lookup(null, "System.Linq"));
            //foreach (var u in usings)
            //{
            //    file.AddNamespaceUsing(addons.Lookup(null, u.FullName));
            //}

            var builder = new FileBuilder(new AntlrInputStream(expression), addons, file);
            builder.Listener.PrepareForExpressionParsing("StepBroFileBuilder.ParseExpression");
            var context = builder.Parser.arguments();

            var walker = new ParseTreeWalker();
            walker.Walk(builder.Listener, context);

            return new Tuple<Stack<SBExpressionData>, IErrorCollector>(builder.Listener.GetArguments(), builder.Errors);
        }

        internal static SBExpressionData ParsePrimary(string content, ScriptFile file = null)
        {
            ITokenSource lexer = new Grammar.StepBroLexer(new AntlrInputStream(content));
            ITokenStream tokens = new CommonTokenStream(lexer);
            var parser = new SBP(tokens);
            m_lastErrors = (file != null) ? file.Errors as ErrorCollector : new ErrorCollector(null, false);
            parser.AddErrorListener(m_lastErrors);
            parser.BuildParseTree = true;
            m_lastListener = new StepBroListener(m_lastErrors, null, file ?? new ScriptFile());
            m_lastListener.PrepareForExpressionParsing("StepBroFileBuilder.ParsePrimary");
            var context = parser.primary();

            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(m_lastListener, context);

            if (m_lastErrors.ErrorCount > 0) throw new Exception("PARSING ERRORS");

            return m_lastListener.GetExpressionResult();
        }

        internal static Data.PropertyBlock ParsePropertyBlock(string content)
        {
            m_lastErrors = new ErrorCollector(null);
            ITokenSource lexer = new Grammar.StepBroLexer(new AntlrInputStream(content));
            ITokenStream tokens = new CommonTokenStream(lexer);
            var parser = new SBP(tokens);
            parser.AddErrorListener(m_lastErrors);
            parser.BuildParseTree = true;
            m_lastListener = new StepBroListener(m_lastErrors);
            var context = parser.elementPropertyblock();

            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(m_lastListener, context);

            if (m_lastErrors.ErrorCount > 0) throw new ParsingErrorException("PARSING ERRORS");

            return m_lastListener.PopPropertyBlockData();
        }

        internal static IDatatable ParseDatatable(string content)
        {
            m_lastErrors = new ErrorCollector(null);
            ITokenSource lexer = new Grammar.StepBroLexer(new AntlrInputStream(content));
            ITokenStream tokens = new CommonTokenStream(lexer);
            var parser = new SBP(tokens);
            parser.AddErrorListener(m_lastErrors);
            parser.BuildParseTree = true;
            m_lastListener = new StepBroListener(m_lastErrors);
            var context = parser.datatableOnly();

            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(m_lastListener, context);

            if (m_lastErrors.ErrorCount > 0) throw new ParsingErrorException("PARSING ERRORS");

            return m_lastListener.GetLastDatatable();
        }

        internal static List<Tuple<string, TypeReference, object>> ParseDatatableRow(string content)
        {
            m_lastErrors = new ErrorCollector(null);
            ITokenSource lexer = new Grammar.StepBroLexer(new AntlrInputStream(content));
            ITokenStream tokens = new CommonTokenStream(lexer);
            var parser = new SBP(tokens);
            parser.AddErrorListener(m_lastErrors);
            parser.BuildParseTree = true;
            m_lastListener = new StepBroListener(m_lastErrors);
            var context = parser.datatableRow();

            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(m_lastListener, context);

            if (m_lastErrors.ErrorCount > 0) throw new Exception("PARSING ERRORS");

            return m_lastListener.GetLastDatatableRow();
        }

        internal static void ParseKeywordProcedureCall(string content)
        {
            ITokenSource lexer = new Grammar.StepBroLexer(new AntlrInputStream(content));
            ITokenStream tokens = new CommonTokenStream(lexer);
            var parser = new SBP(tokens);
            var errors = new ErrorCollector(null);
            parser.AddErrorListener(errors);
            parser.BuildParseTree = true;
            var listener = new StepBroListener(errors);
            //listener.PrepareForExpressionParsing("StepBroFileBuilder.ParseFunction");
            var context = parser.keywordProcedureCall();

            var walker = new ParseTreeWalker();
            walker.Walk(listener, context);

            if (errors.ErrorCount > 0) throw new Exception("PARSING ERRORS");
        }

        internal static IValueContainer<T> ParseFileVariable<T>(Type typeUsing, Type[] typeNamespaces, string content)
        {
            var builder = FileBuilder.Create(content, typeUsing, typeNamespaces);
            builder.Listener.PrepareForExpressionParsing("StepBroFileBuilder.ParseExpression");
            var context = builder.Parser.fileVariable();

            var walker = new ParseTreeWalker();
            walker.Walk(builder.Listener, context);

            if (builder.Errors.ErrorCount > 0) throw new Exception("PARSING ERRORS");

            return builder.File.ListFileVariables().FirstOrDefault() as IValueContainer<T>;
        }

        internal static IValueContainer<T> ParseConfigValue<T>(string content)
        {
            var builder = FileBuilder.Create(content);
            builder.Listener.PrepareForExpressionParsing("StepBroFileBuilder.ParseExpression");
            var context = builder.Parser.constVariable();

            var walker = new ParseTreeWalker();
            walker.Walk(builder.Listener, context);

            if (builder.Errors.ErrorCount > 0) throw new Exception("PARSING ERRORS");

            return builder.File.ListConfigVariables().FirstOrDefault() as IValueContainer<T>;
        }

        internal static IFileProcedure ParseProcedureExpectNoErrors(params string[] content)
        {
            var builder = ParseProcedure(null, new string[] { }, null, content);
            System.Diagnostics.Debug.Assert(builder.Errors.ErrorCount == 0);
            return builder.Listener.LastParsedProcedure;
        }

        internal static IFileProcedure ParseProcedure(params string[] content)
        {
            var builder = ParseProcedure(null, new string[] { }, null, content);
            return builder.Listener.LastParsedProcedure;
        }

        internal static IFileProcedure ParseProcedure(Type usingType, params string[] content)
        {
            return ParseProcedure(null, new string[] { }, usingType, content).Listener.LastParsedProcedure;
        }

        internal static FileBuilder ParseProcedure(
            IAddonManager addonManager,
            string[] usings,
            Type usingType,
            params string[] content)
        {
            if (addonManager == null)
            {
                addonManager = AddonManager.Create();
                addonManager.AddAssembly(typeof(Math).Assembly, false);
                addonManager.AddAssembly(typeof(Enumerable).Assembly, false);
            }
            addonManager.AddAssembly(AddonManager.StepBroCoreAssembly, true);   // Add StepBro.Core always.
            addonManager.AddAssembly(typeof(System.Array).Assembly, false);
            if (usingType != null)
            {
                addonManager.AddAssembly(usingType.Assembly, false);
            }

            var contentBuilder = new StringBuilder();
            foreach (var s in content) contentBuilder.AppendLine(s);
            var file = new ScriptFile();
            file.AddNamespaceUsing(-1, addonManager.Lookup(null, "System"));
            file.AddNamespaceUsing(-1, addonManager.Lookup(null, "System.Linq"));
            file.AddNamespaceUsing(-1, addonManager.Lookup(null, typeof(StepBro.Core.DataReport).Namespace));
            if (usings != null)
            {
                foreach (var u in usings)
                {
                    file.AddNamespaceUsing(-1, addonManager.Lookup(null, u));
                }
            }
            if (usingType != null)
            {
                file.AddNamespaceUsing(-1, addonManager.Lookup(null, usingType.Namespace));
            }


            var builder = new FileBuilder(new AntlrInputStream(contentBuilder.ToString()), addonManager, file);
            builder.Listener.PrepareForExpressionParsing("StepBroFileBuilder.ParseExpression");

            var context = builder.Parser.procedureDeclaration();

            var walker = new ParseTreeWalker();
            walker.Walk(builder.Listener, context);

            if (file.Errors.ErrorCount > 0) throw new Exception("PARSING ERRORS");

            return builder;
        }

        internal static StepBroTypeScanListener.FileContent TypeScanFile(string content)
        {
            ITokenSource lexer = new Grammar.StepBroLexer(new AntlrInputStream(content));
            ITokenStream tokens = new CommonTokenStream(lexer);
            var parser = new SBP(tokens);
            var errors = new ErrorCollector(null);
            parser.AddErrorListener(errors);
            parser.BuildParseTree = true;
            var listener = new StepBroTypeScanListener("Angus");     // Some random default namespace
            var context = parser.compilationUnit();

            var walker = new ParseTreeWalker();
            walker.Walk(listener, context);

            if (errors.ErrorCount > 0) throw new Exception("PARSING ERRORS");

            return listener.GetContent();
        }

        internal static ScriptFile ParseFile(IAddonManager addonManager, string content)
        {
            if (addonManager == null)
            {
                addonManager = AddonManager.Create();
                addonManager.AddAssembly(typeof(Math).Assembly, false);
                addonManager.AddAssembly(typeof(Enumerable).Assembly, false);
            }
            addonManager.AddAssembly(AddonManager.StepBroCoreAssembly, true);   // Add StepBro.Core always.
            ITokenSource lexer = new Grammar.StepBroLexer(new AntlrInputStream(content));
            ITokenStream tokens = new CommonTokenStream(lexer);
            var parser = new SBP(tokens);
            var file = new ScriptFile();
            parser.RemoveErrorListeners();
            parser.AddErrorListener(file.Errors as ErrorCollector);
            parser.BuildParseTree = true;
            var listener = new StepBroListener(file.Errors as ErrorCollector, addonManager, file);
            var context = parser.compilationUnit();

            var walker = new ParseTreeWalker();
            walker.Walk(listener, context);

            if (file.Errors.ErrorCount > 0) throw new ParsingErrorException(file.Errors[0].Line, "PARSING ERROR", file.Errors[0].Message);

            file?.InitializeFileVariables((ILogger)null);
            return file;
        }

        internal static ScriptFile[] ParseFiles(ILogger logger, params Tuple<string, string>[] files)
        {
            return ParseFiles(logger, (Assembly)null, files);
        }

        internal static ScriptFile[] ParseFiles(ILogger logger, Assembly testAssembly, params Tuple<string, string>[] files)
        {
            var fileObjects = new List<ScriptFile>();
            bool first = true;
            foreach (var file in files)
            {
                var fileObject = new ScriptFile(file.Item1, new AntlrInputStream(file.Item2));
                if (!first) fileObject.WasLoadedByNamespace = true; // All but the first file should have the WasLoadedByNamespace prop set.
                fileObjects.Add(fileObject);
                first = false;
            }
            return ParseFiles(logger, testAssembly, fileObjects.ToArray());
        }

        internal static ScriptFile[] ParseFiles(ServiceManager services, ILogger logger, params Tuple<string, string>[] files)
        {
            var fileObjects = new List<ScriptFile>();
            bool first = true;
            foreach (var file in files)
            {
                var fileObject = new ScriptFile(file.Item1, new AntlrInputStream(file.Item2));
                //fileObject.AddNamespaceUsing(services.Get<IAddonManager>().Lookup(null, typeof(Execution.ScriptUtils).FullName));
                if (!first) fileObject.WasLoadedByNamespace = true; // All but the first file should have the WasLoadedByNamespace prop set.
                fileObjects.Add(fileObject);
                first = false;
                services.Get<ILoadedFilesManager>().RegisterLoadedFile(fileObject);
            }
            var errorsCount = ParseFiles(services, logger, fileObjects[0]);
            if (errorsCount > 0) throw new Exception("Parsing errors");
            return fileObjects.ToArray();
        }

        internal static ServiceManager.IServiceManagerAdministration CreateFileParsingSetup(Assembly testAssembly, params ScriptFile[] files)
        {
            ServiceManager.IServiceManagerAdministration services = ServiceManager.Create();
            LastServiceManager = services;

            IService service;
            var addonManager = new AddonManager(null, out service);
            services.Manager.Register(service);
            var objectManager = new DynamicObjectManager(out service);
            services.Manager.Register(service);
            var configFileManager = new Mocks.ConfigurationFileManagerMock(out service);
            services.Manager.Register(service);
            var loadedFiles = new LoadedFilesManager(out service);
            services.Manager.Register(service);
            var mainLogger = new Logger("", false, "StepBro", "Main logger created in CreateFileParsingSetup");
            services.Manager.Register(mainLogger.RootScopeService);
            var taskManager = new TaskManager(out service);
            services.Manager.Register(service);

            TaskContextDummy taskContext = new TaskContextDummy();
            services.StartServices(taskContext);

            addonManager.AddAssembly(typeof(System.Convert).Assembly, false);
            addonManager.AddAssembly(typeof(Math).Assembly, false);
            addonManager.AddAssembly(typeof(Enumerable).Assembly, false);
            addonManager.AddAssembly(AddonManager.StepBroCoreAssembly, true);

            if (testAssembly != null) addonManager.AddAssembly(testAssembly, false);

            foreach (var f in files)
            {
                loadedFiles.RegisterLoadedFile(f);
            }

            return services;
        }

        internal static ScriptFile[] ParseFiles(ILogger logger, Assembly testAssembly, params ScriptFile[] files)
        {
            var services = CreateFileParsingSetup(testAssembly, files);
            ParseFiles(services.Manager, logger, files[0]);
            return services.Manager.Get<ILoadedFilesManager>().ListFiles<ScriptFile>().ToArray();
        }

        public static int ParseFiles(ServiceManager services, ILogger logger, IScriptFile topfile)
        {
            var addons = services.Get<IAddonManager>();
            var filesManager = services.Get<ILoadedFilesManager>();
            var shortcutsManager = ServiceManager.Global.Get<IFolderManager>();
            var configFileManager = ServiceManager.Global.Get<IConfigurationFileManager>();

            configFileManager.ResetFolderConfigurations();

            var filesToParse = new List<ScriptFile>();
            if (topfile == null)
            {
                topfile = filesManager.TopScriptFile as ScriptFile;
            }
            filesToParse.Add(topfile as ScriptFile);

            object parserUser = new object();
            var filesInTreeBeforeParsing = ((ScriptFile)topfile).ListResolvedFileUsings(false, true).Append(topfile as ScriptFile).Distinct().ToList();
            foreach (var file in filesInTreeBeforeParsing)
            {
                file.RegisterDependant(parserUser);
            }

            var fileListeners = new Dictionary<ScriptFile, StepBroListener>();
            var fileContexts = new Dictionary<ScriptFile, SBP.CompilationUnitContext>();
            var namespaceFiles = new Dictionary<string, IdentifierInfo>();

            //==============================================================//
            #region STEP 1: PRE-SCAN ALL FILES TO OPEN ALL DEPENDENCY FILES //
            //==============================================================//
            var fileParsingStack = new Queue<ScriptFile>(filesToParse);
            while (fileParsingStack.Count > 0)
            {
                var file = fileParsingStack.Dequeue();
                file.ResetBeforeParsing();
                file.MarkForTypeScanning();
                ITokenSource lexer = new Grammar.StepBroLexer(file.GetParserFileStream(services.Get<ITextFileSystem>()));
                var tokens = new CommonTokenStream(lexer);
                var parser = new SBP(tokens);
                parser.RemoveErrorListeners();
                (file.Errors as ErrorCollector).Clear();
                parser.AddErrorListener(file.Errors as ErrorCollector);
                parser.BuildParseTree = true;
                var context = parser.compilationUnit();
                fileContexts.Add(file, context);

                if (String.IsNullOrEmpty(file.Namespace))
                {
                    file.SetNamespace(System.IO.Path.GetFileNameWithoutExtension(file.FileName));
                }

                var docComments = new List<Tuple<int,string>>();
                foreach (var token in tokens.GetTokens())
                {
                    // token.Type == Lexer.DOC_COMMENT_INDENTED || 
                    if (token.Type == Lexer.DOC_COMMENT)
                    {
                        docComments.Add(new Tuple<int, string>(token.Line, token.Text));
                    }
                }
                file.SetDocumentComments(docComments);

                var visitor = new StepBroTypeVisitor();
                visitor.Visit(context);

                var scanListener = new StepBroTypeScanListener(file.Namespace);
                var walker = new ParseTreeWalker();
                walker.Walk(scanListener, context);

                if (file.Errors.ErrorCount > 0) continue;   // Stop parsing this file

                var parserListener = new StepBroListener(file.Errors as ErrorCollector, addons, file);
                fileListeners.Add(file, parserListener);
                var fileScanData = scanListener.GetContent();
                file.PreScanFileContent = fileScanData;

                System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(fileScanData.TopElement.Name));
                file.SetNamespace(fileScanData.TopElement.Name);    // Update the namespace from the scanning.

                foreach (var @using in fileScanData.ListUsings())
                {
                    var line = @using.Line;
                    var type = @using.Type;
                    var name = @using.Name;
                    if (type == "i")
                    {
                        if (!file.AddNamespaceUsing(line, name))
                        {
                            throw new Exception($"Namespace using already added ({name}).");
                        }
                    }
                    else if (type == "p")
                    {
                        if (!file.AddFileUsing(line, @using.IsPublic, name))
                        {
                            throw new Exception($"File using already added ({name}).");
                        }
                    }
                    else throw new Exception($"Unhandled using type: {type}");
                }

                file.ResolveFileUsings(
                    (fu, line) =>
                    {
                        string basefolder = Path.GetDirectoryName(file.GetFullPath());
                        var fuName = Path.GetFileName(fu);

                        // Check the already parsed or loaded files first.
                        foreach (var f in filesToParse)
                        {
                            if (!Object.ReferenceEquals(file, f) && String.Equals(fuName, f.FileName, StringComparison.InvariantCulture))
                            {
                                return f;
                            }
                        }
                        foreach (var f in filesManager.ListFiles<ScriptFile>())
                        {
                            if (!Object.ReferenceEquals(file, f) && String.Equals(fuName, f.FileName, StringComparison.InvariantCulture))
                            {
                                fileParsingStack.Enqueue(f);
                                filesToParse.Insert(0, f);      // Put in front
                                return f;
                            }
                        }

                        // File was not found among the already loaded files. Try loading the file.

                        string foundMatchingFile = null;

                        if (fu.Contains("["))     // It's a path using a folder shortcut.
                        {
                            string error = null;
                            string path = shortcutsManager.ListShortcuts().ResolveShortcutPath(fu, ref error);
                            if (String.IsNullOrEmpty(path))
                            {
                                string errorText = ".";
                                if (!String.IsNullOrEmpty(error))
                                {
                                    errorText = "; " + error;
                                }
                                file.ErrorsInternal.SymanticError(line, -1, false, $"Parsing '{file.FileName}': Unable to resolve using path \"{fu}\"{errorText}");
                            }
                            else
                            {
                                if (System.IO.File.Exists(path))
                                {
                                    foundMatchingFile = path;
                                }
                            }
                        }
                        else if (fu.Contains("\\") || fu.Contains("/"))     // It's a relative or absolute path
                        {
                            string path = Path.GetFullPath(Path.Combine(basefolder, fu));
                            if (System.IO.File.Exists(path))
                            {
                                foundMatchingFile = path;
                            }
                        }
                        else
                        {
                            // Start at the location of this file.
                            while (basefolder != Path.GetPathRoot(basefolder))
                            {
                                string path = Path.GetFullPath(Path.Combine(basefolder, fu));
                                if (System.IO.File.Exists(path))
                                {
                                    foundMatchingFile = path;
                                    break;
                                }

                                var cfgFile = Path.GetFullPath(Path.Combine(basefolder, Constants.STEPBRO_FOLDER_CONFIG_FILE));
                                if (System.IO.File.Exists(cfgFile))
                                {
                                    var folderConfig = file.TryOpenFolderConfiguration(configFileManager, line, cfgFile);


                                    //var errors = new List<Tuple<int, string>>();
                                    //var folderConfig = configFileManager.ReadFolderConfig(cfgFile, errors);

                                    //if (errors.Count > 0)
                                    //{
                                    //    var errortext = "";
                                    //    foreach (var e in errors)
                                    //    {
                                    //        if (e.Item1 <= 0) errortext = $"Config file '{cfgFile}': {e.Item2}";
                                    //        else errortext = $"Config file '{cfgFile}' line {e.Item1}: {e.Item2}";
                                    //    }

                                    //    file.ErrorsInternal.ConfigError(line, 0, errortext);
                                    //}

                                    if (folderConfig != null)
                                    {
                                        //file.AddFolderConfig(folderConfig);

                                        foreach (var lib in folderConfig.LibFolders)
                                        {
                                            path = Path.GetFullPath(Path.Combine(basefolder, lib, fu));
                                            if (System.IO.File.Exists(path))
                                            {
                                                foundMatchingFile = path;
                                                break;
                                            }
                                        }
                                        if (folderConfig.IsSearchRoot)
                                        {
                                            break;  // Don't search deeper now.
                                        }
                                    }
                                }

                                // Not found yet; try the parent folder.
                                basefolder = Path.GetDirectoryName(basefolder);
                            }
                        }

                        if (foundMatchingFile != null)
                        {
                            // Load and add file to fileParsingStack
                            if (Main.LoadScriptFile(parserUser, filepath: foundMatchingFile) is ScriptFile loadedFile)
                            {
                                // Note: The parser will set the current scriptfile as a dependant.

                                fileParsingStack.Enqueue(loadedFile);
                                filesToParse.Add(loadedFile);
                                return loadedFile;
                            }
                        }

                        return null;    // Not found!
                    }
                );
                file.ResolveNamespaceUsings(
                    id =>
                    {
                        var foundIdentifier = addons.Lookup(null, id);
                        if (foundIdentifier != null)
                        {
                            return foundIdentifier;
                        }
                        var scriptFilesInNamespace = new List<ScriptFile>();
                        foreach (var f in filesToParse)
                        {
                            if (f.Namespace.Equals(id, StringComparison.InvariantCulture))
                            {
                                if (foundIdentifier == null)
                                {
                                    foundIdentifier = new IdentifierInfo(id, id, IdentifierType.FileNamespace, null, scriptFilesInNamespace);
                                }
                                scriptFilesInNamespace.Add(f);  // More files can have same namespace.
                            }
                        }
                        if (foundIdentifier != null)
                        {
                            return foundIdentifier;
                        }
                        foreach (var f in filesManager.ListFiles<ScriptFile>())
                        {
                            if (f.Namespace.Equals(id, StringComparison.InvariantCulture))
                            {
                                if (foundIdentifier == null)
                                {
                                    foundIdentifier = new IdentifierInfo(id, id, IdentifierType.FileNamespace, null, scriptFilesInNamespace);
                                }
                                scriptFilesInNamespace.Add(f);  // Different files can have the same namespace.
                                fileParsingStack.Enqueue(f);
                                filesToParse.Insert(0, f);      // Put in front
                            }
                        }

                        return foundIdentifier;     // Is null if not found
                    }
                );

            }

            foreach (var file in filesToParse)
            {
                if (!file.AllFolderConfigsRead)
                {
                    string basefolder = Path.GetDirectoryName(file.GetFullPath());

                    while (basefolder != Path.GetPathRoot(basefolder))
                    {
                        var cfgFile = Path.GetFullPath(Path.Combine(basefolder, Constants.STEPBRO_FOLDER_CONFIG_FILE));
                        if (System.IO.File.Exists(cfgFile))
                        {
                            var folderConfig = file.TryOpenFolderConfiguration(configFileManager, -1, cfgFile);
                            if (folderConfig != null && folderConfig.IsSearchRoot)
                            {
                                break;  // Don't search deeper now.
                            }
                        }

                        // Not found yet; try the parent folder.
                        basefolder = Path.GetDirectoryName(basefolder);
                    }
                }
            }

            #endregion

            //===================================================//
            #region STEP 2: COLLECT ALL THE PROCEDURE SIGNATURES //
            //===================================================//
            // TODO: Sort files after dependencies (usings)

            var beforeSorting = filesToParse;
            List<ScriptFile> sortedAfterDependencies = new List<ScriptFile>();
            var filesToCheck = new Queue<ScriptFile>(filesToParse);
            int parsingFloor = 0;
            while (filesToCheck.Count > 0)
            {
                var file = filesToCheck.Dequeue();
                bool addNow = true;
                foreach (var fu in file.ListReferencedScriptFiles())
                {
                    if (!sortedAfterDependencies.Contains(fu))
                    {
                        filesToCheck.Enqueue(file); // Put back in queue.
                        addNow = false;
                        break;
                    }
                }
                if (addNow)
                {
                    file.ParsingFloor = parsingFloor++;
                    sortedAfterDependencies.Add(file);
                }
            }
            filesToParse = sortedAfterDependencies;

            foreach (var file in filesToParse)
            {
                var fileScanData = file.PreScanFileContent;
                if (fileScanData != null)
                {
                    foreach (var element in fileScanData.TopElement.Childs)
                    {
                        var firstPropFlag = (element.PropertyFlags != null) ? element.PropertyFlags[0] : null;
                        var accessModifier = (element.Modifiers != null && element.Modifiers.Count > 0) ? (AccessModifier)Enum.Parse(typeof(AccessModifier), element.Modifiers[0], true) : DefaultAccess;
                        switch (element.Type)
                        {
                            case FileElementType.Using:
                                break;
                            case FileElementType.Namespace:
                                file.SetNamespace(element.Name);
                                throw new Exception();
                            //break;
                            case FileElementType.EnumDefinition:
                                break;
                            case FileElementType.ProcedureDeclaration:
                                {
                                    var procedure = new FileProcedure(file, accessModifier, element.Line, null, file.Namespace, element.Name)
                                    {
                                        Flags = (element.IsFunction ? ProcedureFlags.IsFunction : ProcedureFlags.None),
                                        HasBody = element.HasBody,
                                        BaseElementName = firstPropFlag,
                                    };
                                    file.AddElement(procedure);
                                    procedure.CheckForPrototypeChange(element.Parameters, element.ReturnTypeData);
                                }
                                break;
                            case FileElementType.FileVariable:
                                // TODO: Add a temporary file element here, to make all file elements available after the pre-scan (to be able to remove "redundant" UpdateRootIdentifiers calls).
                                // Add file variable with temporary type and data.
                                //file.CreateOrGetFileVariable(file.Namespace, accessModifier, element.Name, (TypeReference)default(Type), false, element.Line, 0, 0);
                                break;
                            case FileElementType.TestList:
                                {
                                    var testlist = new FileTestList(file, accessModifier, element.Line, null, file.Namespace, element.Name)
                                    {
                                        BaseElementName = firstPropFlag,
                                    };
                                    file.AddElement(testlist);
                                }
                                break;
                            case FileElementType.Datatable:
                                break;
                            case FileElementType.Override:
                                {
                                    var overrider = file.CreateOrGetOverrideElement(element.Line, element.Name);
                                    overrider.SetAsType(element.AsType);
                                    file.AddElement(overrider);
                                }
                                break;
                            case FileElementType.TypeDef:
                                {
                                    var typedef = file.CreateOrGetTypeDefElement(element.Line, element.Name);
                                    typedef.SetDeclaration(element.DataType);
                                    file.AddElement(typedef);
                                }
                                break;
                            case FileElementType.UsingAlias:
                                {
                                    var typedef = new FileElementUsingAlias(file, element.Line, file.Namespace, element.Name);
                                    typedef.SetDeclaration(element.DataType);
                                    file.AddElement(typedef);
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    file.LastTypeScan = DateTime.UtcNow;
                }
            }
            #endregion

            //====================================================//
            #region STEP 3: PARSE ALL SIGNATURES AND DECLARATIONS //
            //====================================================//
            var signaturesToParseNow = new List<Tuple<FileElement, StepBroListener>>();
            // Collect file elements to parse from _all_ files
            foreach (var file in filesToParse)
            {
                StepBroListener listener;
                if (fileListeners.TryGetValue(file, out listener))
                {
                    signaturesToParseNow.AddRange(
                        file.ListElements().Cast<FileElement>().Select(e => new Tuple<FileElement, StepBroListener>(e, listener)));
                }
            }

            // Update the lookup tables before further parsing.
            foreach (var file in filesToParse)
            {
                file.UpdateRootIdentifiers();
            }

            var numberItemsToParse = (signaturesToParseNow.Count > 0) ? int.MaxValue - 1 : 0;
            var numberUnparsedItemsBefore = int.MaxValue;
            var signaturesToParseAgain = new List<Tuple<FileElement, StepBroListener>>();
            // Continue parsing signatures until no more elements can be resolved
            while (numberItemsToParse > 0 && numberItemsToParse < numberUnparsedItemsBefore)
            {
                numberUnparsedItemsBefore = numberItemsToParse;
                numberItemsToParse = 0;
                foreach (var d in signaturesToParseNow)
                {
                    d.Item1.ParseBaseElement();
                    var unparsedItemsInElement = d.Item1.ParseSignature(d.Item2, false);
                    if (unparsedItemsInElement > 0)
                    {
                        numberItemsToParse += unparsedItemsInElement;
                        signaturesToParseAgain.Add(d);
                    }
                }
                signaturesToParseNow = signaturesToParseAgain;
                signaturesToParseAgain = new List<Tuple<FileElement, StepBroListener>>();
            }

            if (numberItemsToParse > 0)
            {
                foreach (var d in signaturesToParseNow)
                {
                    d.Item1.ParseSignature(d.Item2, true);  // Parse again and report the errors
                }
                //throw new Exception("Not all signatures could be parsed.");     // TBD
                return signaturesToParseNow.Count;
            }
            #endregion

            //=================================//
            #region STEP 4: PARSE ALL THE FILES #
            //=================================//
            int totalErrors = 0;
            try
            {
                foreach (var file in filesToParse)
                {
                    file.SaveCurrentFileVariables();
                    StepBroListener listener;
                    if (file.Errors.ErrorCount == 0)
                    {
                        if (fileListeners.TryGetValue(file, out listener))
                        {
                            var context = fileContexts[file];

                            try
                            {
                                file.UpdateRootIdentifiers();
                                var walker = new ParseTreeWalker();
                                walker.Walk(listener, context);
                                file.UpdateRootIdentifiers();
                            }
                            catch (Exception e)
                            {
                                file.ErrorsInternal.InternalError(-1, -1, e.GetType().Name + "; " + e.Message);
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                // Run through the files once more, to initialize variables and to check the error count.
                foreach (var file in filesToParse)
                {
                    if (file.Errors.ErrorCount == 0)
                    {
                        // Initialize file variables after ALL files have been parsed,
                        // because parsing a file can change data in other files.
                        // Note: errors can occur during this, thus this has to be done before checking the error status.
                        file.InitializeFileVariables(logger);
                        file.DisposeUnusedFileVariables(logger);
                    }

                    totalErrors += file.Errors.ErrorCount;
                    file.LastParsing = DateTime.UtcNow;
                    if (file.Errors.ErrorCount == 0)
                    {
                        file.LastSuccessfulParsing = file.LastParsing;
                    }
                }
#if DEBUG
                // Dump the id's of all the elements
                foreach (var file in filesToParse)
                {
                    foreach (var element in file.ListElements())
                    {
                        System.Diagnostics.Debug.WriteLine(file.FileName + ": " + element.UniqueID.ToString() + " " + element.Name);
                    }
                }
#endif
            }
            finally
            {
                foreach (var file in filesInTreeBeforeParsing)
                {
                    file.UnregisterDependant(parserUser);
                }
                foreach (var file in filesToParse)
                {
                    file.DisposeFileStream();
                    if (file.IsDependantOf(parserUser))
                    {
                        file.UnregisterDependant(parserUser);
                    }
                }
            }
            #endregion

            return totalErrors;
        }

        //public static void ParseKeywordTest()
        //{
        //    string input = "a4.Send.Mogens( 12 );";
        //    AntlrInputStream stream = new AntlrInputStream(input);
        //    ITokenSource lexer = new KeywordTestLexer(stream);
        //    ITokenStream tokens = new CommonTokenStream(lexer);
        //    KeywordTestParser parser = new KeywordTestParser(tokens);
        //    parser.BuildParseTree = true;
        //    KeywordTestListener listener = new KeywordTestListener();
        //    var context = parser.procedureUnit();

        //    ParseTreeWalker walker = new ParseTreeWalker();
        //    walker.Walk(listener, context);
        //}

        public class Account
        {
            public string Email { get; set; }
            public bool Active { get; set; }
            public DateTime CreatedDate { get; set; }
            public IList<string> Roles { get; set; }
        }

        public static void Test()
        {
            //var resolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();

            //var contract = resolver.ResolveContract(typeof(Account));
        }
    }
}
