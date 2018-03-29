using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogMonitor
{
    public abstract class LogEventPublisher<T>
    {
        protected List<LogEventSubscriber<T>> ObservorList = new List<LogEventSubscriber<T>>();
        public LogEventPublisher()
        {

        }
        public void Subscribe(LogEventSubscriber<T> observor)
        {
            ObservorList.Add(observor);
        }
        public void UnSubscribe(LogEventSubscriber<T> observor)
        {
            ObservorList.Remove(observor);
        }
        public abstract void Notify(params object[] args);
    }


    public interface LogEventSubscriber<T>
    {
        void Update(T Input);
    }
}
