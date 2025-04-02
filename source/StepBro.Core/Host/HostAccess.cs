using System;
using System.Linq;
using System.Collections.Generic;
using StepBro.Core.Data;
using StepBro.Core.Tasks;
using StepBro.Core.Logging;
using StepBro.Core.Execution;
using System.Collections.ObjectModel;

namespace StepBro.Core.Host;

//public abstract class ServiceBase<TService, TThis> where TThis : ServiceBase<TService, TThis>, TService
public abstract class HostAccessBase<TThis> :
    ServiceBase<IHost, TThis>, 
    IHost 
    where TThis : HostAccessBase<TThis>, IHost
{
    private ILogger m_logger;
    private ObservableCollection<IObjectContainer> m_objectContainers = new ObservableCollection<IObjectContainer>();

    protected HostAccessBase(string name, out IService serviceAccess, params Type[] dependencies) :
        base(name, out serviceAccess, dependencies.Concat(new Type[] { typeof(ILogger) }).ToArray())
    {
        m_objectContainers.CollectionChanged += ObjectContainers_CollectionChanged;
    }

    private void ObjectContainers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        this.ObjectContainerListChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void Start(ServiceManager manager, ITaskContext context)
    {
        m_logger = manager.Get<ILogger>();
    }

    public abstract HostType Type { get; }

    public virtual IEnumerable<IObjectContainer> ListObjectContainers()
    {
        foreach (var c in m_objectContainers) yield return c;
    }

    public void AddObject(string name, object o)
    {
        var container = new SimpleObjectContainer(name, o);
        m_objectContainers.Add(container);
    }
    public void AddObject(IObjectContainer container)
    {
        m_objectContainers.Add(container);
    }

    public event EventHandler ObjectContainerListChanged;

    public virtual IEnumerable<Type> ListHostCodeModuleTypes()
    {
        yield break;
    }

    public virtual void LogSystem(string text)
    {
        m_logger.LogSystem(text);
    }

    public virtual void LogUserAction(string text)
    {
        m_logger.LogUserAction(text);
    }

    public virtual bool SupportsUserInteraction { get { return false; } }


    public virtual UserInteraction SetupUserInteraction(ICallContext context, string header)
    {
        context.ReportError("The used host application dors not support user ainteraction from scripts.");
        return null;
    }

}
