using StepBro.Core;
using StepBro.Core.General;
using StepBro.HostSupport.Models;
using System.Linq;
using StepBroMain = StepBro.Core.Main;

namespace StepBro.HostSupport.Test;

[TestClass]
public class TestLogViewer
{
    private HostAppModel m_app = null;
    private LogViewerModel<LogViewTestEntry> m_logViewer = null;

    [TestInitialize]
    public void Setup()
    {
        m_app = new HostAppModel();
        IService testFileSystemService = null;
        var mockFileSystem = new StepBro.Core.Test.Mocks.TextFileSystemMock(out testFileSystemService);
        m_logViewer = new LogViewerModel<LogViewTestEntry>(new LogViewTestEntryFactory());
        IService hostAccessService = null;
        var host = new HostAccess(m_app, out hostAccessService);
        m_app.Initialize(m_logViewer, testFileSystemService, hostAccessService);
        Assert.AreEqual(0, m_app.Views.Where(v => v.IsOpen).Count());
        m_logViewer.Setup();
    }

    [TestCleanup]
    public void Cleanup()
    {
        StepBroMain.DeinitializeInternal(true);
    }

    [TestMethod]
    public void TestInitialization()
    {
        var viewPort = m_logViewer.ListViewModel.ViewPort;
        viewPort.Height = 105;
        viewPort.LineHeight = 10;
        Assert.AreEqual(10, viewPort.MaxLinesVisible);
        Assert.AreEqual(11, viewPort.MaxLinesPartlyVisible);
        Assert.IsFalse(viewPort.IsViewFilled());
        Assert.IsTrue(viewPort.IsInvalidated);      // Invalidated during setup.

        m_logViewer.ListViewModel.RequestUpdate();
        Assert.IsTrue(viewPort.IsInvalidated);
        var viewPortEntries = viewPort.Refresh();
        Assert.IsFalse(viewPort.IsInvalidated);
        var entryCount = viewPortEntries.Count;
        Assert.AreEqual(2, entryCount);

        // Checking again with no changes; expecting no update.
        m_logViewer.ListViewModel.RequestUpdate();
        Assert.IsFalse(viewPort.IsInvalidated);
    }

    [TestMethod]
    public void TestAddingEntriesInTailMode()
    {
        var listView = m_logViewer.ListViewModel;
        var viewPort = m_logViewer.ListViewModel.ViewPort;
        viewPort.Height = 105;
        viewPort.LineHeight = 10;
        Assert.IsTrue(viewPort.IsInvalidated);      // Invalidated during setup.

        listView.RequestUpdate();
        Assert.IsTrue(viewPort.IsInvalidated);
        var viewPortEntries = viewPort.Refresh();
        Assert.IsFalse(viewPort.IsInvalidated);
        var entryCount = viewPortEntries.Count;
        Assert.AreEqual(2, entryCount);

        m_app.RootLogger.Log("Activity A");
        Assert.IsFalse(viewPort.IsInvalidated);
        listView.RequestUpdate();
        Assert.IsTrue(viewPort.IsInvalidated);
        viewPortEntries = viewPort.Refresh();
        Assert.IsFalse(viewPort.IsInvalidated);
        entryCount = viewPortEntries.Count;
        Assert.AreEqual(3, entryCount);
        Assert.AreEqual("Activity A", viewPortEntries[2].LogEntry.Text);
        Assert.IsFalse(viewPort.IsViewFilled());

        for (int i = 0; i < 7; i++)
        {
            m_app.RootLogger.Log("Activity B " + i);
        }
        listView.RequestUpdate();
        Assert.IsTrue(viewPort.IsInvalidated);
        viewPortEntries = viewPort.Refresh();
        Assert.IsFalse(viewPort.IsInvalidated);
        entryCount = viewPortEntries.Count;
        Assert.AreEqual(10, entryCount);
        Assert.IsFalse(viewPort.IsViewFilled());
        Assert.AreEqual("Activity B 0", viewPortEntries[3].LogEntry.Text);
        Assert.AreEqual("Activity B 1", viewPortEntries[4].LogEntry.Text);
        Assert.AreEqual("Activity B 2", viewPortEntries[5].LogEntry.Text);
        Assert.AreEqual("Activity B 3", viewPortEntries[6].LogEntry.Text);
        Assert.AreEqual("Activity B 4", viewPortEntries[7].LogEntry.Text);
        Assert.AreEqual("Activity B 5", viewPortEntries[8].LogEntry.Text);
        Assert.AreEqual("Activity B 6", viewPortEntries[9].LogEntry.Text);

        m_app.RootLogger.Log("Activity C");
        Assert.IsFalse(viewPort.IsInvalidated);
        listView.RequestUpdate();
        Assert.IsTrue(viewPort.IsInvalidated);
        viewPortEntries = viewPort.Refresh();
        Assert.IsFalse(viewPort.IsInvalidated);
        entryCount = viewPortEntries.Count;
        Assert.AreEqual(10, entryCount);
        Assert.IsFalse(viewPort.IsViewFilled());
        Assert.AreEqual("Activity A", viewPortEntries[1].LogEntry.Text);    // Now scrolled one line up.
        Assert.AreEqual("Activity B 0", viewPortEntries[2].LogEntry.Text);
        Assert.AreEqual("Activity B 1", viewPortEntries[3].LogEntry.Text);
        Assert.AreEqual("Activity B 2", viewPortEntries[4].LogEntry.Text);
        Assert.AreEqual("Activity B 3", viewPortEntries[5].LogEntry.Text);
        Assert.AreEqual("Activity B 4", viewPortEntries[6].LogEntry.Text);
        Assert.AreEqual("Activity B 5", viewPortEntries[7].LogEntry.Text);
        Assert.AreEqual("Activity B 6", viewPortEntries[8].LogEntry.Text);
        Assert.AreEqual("Activity C", viewPortEntries[9].LogEntry.Text);
        listView.RequestUpdate();
        Assert.IsFalse(viewPort.IsInvalidated);
    }

}
