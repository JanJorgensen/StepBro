using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using StepBro.Core.Parser;
using System;
using System.Linq;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Data
{
    public static class PropertyBlockUtils
    {
        public static PropertyBlock ToPropertyBlock(this string s)
        {
            ErrorCollector errors = new ErrorCollector(null);
            ITokenSource lexer = new StepBro.Core.Parser.Grammar.StepBroLexer(new AntlrInputStream(s));
            ITokenStream tokens = new CommonTokenStream(lexer);
            var parser = new SBP(tokens);
            parser.AddErrorListener(errors);
            parser.BuildParseTree = true;
            StepBroListener listener = new StepBroListener(errors);
            var context = parser.elementPropertyList();

            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(listener, context);

            if (errors.ErrorCount > 0) throw new Exception("PARSING ERRORS");

            return listener.PopPropertyBlockData();
        }

        public static PropertyBlock GetPropertyBlockFromFile(this string file)
        {
            string text = System.IO.File.ReadAllText(file);
            return text.ToPropertyBlock();
        }

        /// <summary>
        /// Creates a clone of a PropertyBlock, where the general device entries used in the station properties file has been removed.
        /// </summary>
        /// <remarks>This function will typically be used on a device configuration entry read from the station properties file.</remarks>
        /// <param name="props">The device properties.</param>
        /// <returns>Filtered clone of the specified properties.</returns>
        public static PropertyBlock CloneWithoutGeneralDeviceConfigEntries(this PropertyBlock props)
        {
            var result = new PropertyBlock(props.Line, props.Name);
            string[] generalEntries = new string[] { "type", "aliases" };
            foreach (var entry in props)
            {
                if (entry.BlockEntryType == PropertyBlockEntryType.Value &&
                    generalEntries.Count(g => String.Equals(entry.Name, g, StringComparison.InvariantCultureIgnoreCase)) > 0)
                {
                    continue;
                }
                result.Add(entry.Clone());
            }
            return props;
        }

        public static PropertyBlock Merge(this PropertyBlock props, PropertyBlock other)
        {
            var otherClone = other.Clone() as PropertyBlock;
            var merged = new PropertyBlock(props.Line, props.Name).CloneBase(props) as PropertyBlock;
            foreach (var e in props)
            {
                var overrider = otherClone.FirstOrDefault(
                    oe => oe.BlockEntryType == e.BlockEntryType &&
                    String.Equals(oe.Name, e.Name, StringComparison.InvariantCultureIgnoreCase));
                if (overrider != null)
                {
                    overrider.IsUsedOrApproved = true;
                    if (overrider.IsAdditionAssignment)
                    {
                        if (e.BlockEntryType == PropertyBlockEntryType.Array)
                        {
                            var array = e.Clone() as PropertyBlockArray;
                            array.AddRange((overrider as PropertyBlockArray).Select(c => c.Clone()));
                            merged.Add(array);
                        }
                        else if (e.BlockEntryType == PropertyBlockEntryType.Block)
                        {
                            throw new NotImplementedException();
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        merged.Add(overrider.Clone());
                    }
                }
                else
                {
                    merged.Add(e.Clone());
                }
            }
            foreach (var e in otherClone.Where(ce => ce.IsUsedOrApproved == false))
            {
                merged.Add(e.Clone());
            }
            return merged;
        }

    }
}
