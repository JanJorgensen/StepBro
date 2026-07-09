using StepBro.Core.Data;
using StepBro.Core.Logging;
using StepBro.HostSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using StepBro.HostSupport.Models;

namespace StepBro.UI.Controls;

public class LogViewEntry : ChronoTimestampedListViewEntry
{
    protected LogEntry m_entry;
    protected long m_sourceIndex;

    private static Pen s_parentPen = new Pen(Brushes.Orange, 3.0f);
    private static Pen s_siblingPen = new Pen(Brushes.Yellow, 1.0f);

    public LogViewEntry(LogEntry entry, long index) : base()
    {
        m_entry = entry;
        m_sourceIndex = index;
    }

    public override ITimestampedData DataObject { get { return m_entry; } }

    protected virtual string GetHeaderText()
    {
        string headerText = m_entry.EntryType switch
        {
            //LogEntry.Type.Async => "<A>",
            LogEntry.Type.CommunicationOut => "<Out>",
            LogEntry.Type.CommunicationIn => "<In>",
            LogEntry.Type.TaskEntry => "TaskEntry",
            LogEntry.Type.Error => "Error",
            LogEntry.Type.Failure => "Fail",
            LogEntry.Type.UserAction => "UserAction",
            _ => ""
        };
        return headerText;
    }

    protected virtual string GetLocationText()
    {
        return m_entry.Location;
    }
    protected virtual string GetDetailsText()
    {
        return m_entry.Text;
    }

    public override string GetTextForSearchMatching(bool includeExtraFields)
    {
        return m_entry.Text;
    }

    protected override void PaintRest(DrawingContext context, ChronoListViewPort.IView view, ref Rect rect, EntryMarkState markings)
    {
        var color = ((markings & EntryMarkState.Selected) != EntryMarkState.None) ? Brushes.White : GetDefaultEntryTypeColor(m_entry.EntryType);
        var fontSize = view.FontSize;
        string headerText = this.GetHeaderText();
        var headerWidth = view.ViewSettings.LineHeaderWidth * fontSize;
        var w = DrawTextField(context, view, color, headerText, rect, headerWidth);
        if ((w / fontSize) > headerWidth)
        {
            headerWidth = w;
            view.ViewSettings.LineHeaderWidth = headerWidth / fontSize;
        }
        rect = new Rect(new Point(rect.X + headerWidth + 4 + (m_entry.IndentLevel * fontSize * 4.0), rect.Y), rect.BottomRight);

        if ((markings & EntryMarkState.Parent) != EntryMarkState.None)
        {
            //pe.Graphics.DrawLine(s_parentPen, new Point(rect.X - 3, rect.Top), new Point(rect.X - 3, rect.Bottom));           // TODO
        }
        if ((markings & EntryMarkState.Sibling) != EntryMarkState.None)
        {
            //pe.Graphics.DrawLine(s_siblingPen, new Point(rect.X - 3, rect.Top), new Point(rect.X - 3, rect.Bottom));          // TODO
        }

        var location = this.GetLocationText();
        var text = this.GetDetailsText();
        if (location != null)
        {
            w = DrawTextField(context, view, color, location, rect);
            if (text != null)
            {
                rect = new Rect(new Point(rect.X + w + fontSize, rect.Y), rect.BottomRight);
                w = DrawTextField(context, view, color, "-", rect);
                rect = new Rect(new Point(rect.X + w + fontSize, rect.Y), rect.BottomRight);
            }
        }
        if (text != null)
        {
            w = DrawTextField(context, view, color, text, rect);
        }
    }

    public static IBrush GetDefaultEntryTypeColor(LogEntry.Type type)
    {
        switch (type)
        {
            case LogEntry.Type.Pre:
            case LogEntry.Type.PreHighLevel:
            case LogEntry.Type.TaskEntry:
                return Brushes.Cyan;
            case LogEntry.Type.Normal:
            case LogEntry.Type.Post:
                return Brushes.White;
            case LogEntry.Type.Async:
            case LogEntry.Type.CommunicationOut:
            case LogEntry.Type.CommunicationIn:
                return Brushes.DarkKhaki;
            case LogEntry.Type.Error:
            case LogEntry.Type.Failure:
                return Brushes.OrangeRed;
            case LogEntry.Type.UserAction:
                return Brushes.DeepSkyBlue;
            case LogEntry.Type.Detail:
                return Brushes.LightGray;
            case LogEntry.Type.System:
                return Brushes.Plum;
            case LogEntry.Type.Special:
                return Brushes.Pink;
            default:
                return Brushes.White;
        }

    }
}


public class LogViewEntryFactory : IChronoListViewEntryFactory
{
    public void CreatePresentationEntry(ITimestampedData entry, long sourceIndex, Action<ITimestampedViewEntry> adder)
    {
        adder(new LogViewEntry(entry as LogEntry, sourceIndex));
    }
}
