using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TennisEmulator
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly Dictionary<object, Dictionary<Type, List<object>>> handlers = new Dictionary<object, Dictionary<Type, List<object>>>();

        public void Dispatch<TS, TE>(TS source, TE even)
        {
            Dictionary<Type, List<object>> eventToHandlers = this.handlers.GetValueOrDefault(source);
            if (eventToHandlers != null)
            {
                List<object> handlersList = eventToHandlers.GetValueOrDefault(typeof(TE));
                if (handlersList != null)
                {
                    List<object> handlersClone = handlersList.ToList();
                    Task.Factory.StartNew(() => {
                        foreach (IEventHandler<TS,TE> h in handlersClone)
                        {
                            h.Handle(source, even);
                        }
                    });
                }
            }
        }

        public void AddHandler<TS, TE>(TS source, IEventHandler<TS, TE> handler)
        {
            Dictionary<Type, List<object>> eventToHandlers = this.handlers.GetValueOrDefault(source);
            if (eventToHandlers == null)
            {
                this.handlers.Add(source, eventToHandlers = new Dictionary<Type, List<object>>());
            }

            List<object> handlersList = eventToHandlers.GetValueOrDefault(typeof(TE));
            if (handlersList == null)
            {
                eventToHandlers.Add(typeof(TE), handlersList = new List<object>());
            }

            if (!handlersList.Contains(handler))
            {
                handlersList.Add(handler);
            }
        }

        public void CleanupSource<TS>(TS source)
        {
            this.handlers.Remove(source);
        }

        public void RemoveHandler<TS, TE>(TS source, IEventHandler<TS, TE> handler)
        {
            Dictionary<Type, List<object>> eventToHandlers = this.handlers.GetValueOrDefault(source);
            if (eventToHandlers != null)
            {
                List<object> handlersList = eventToHandlers.GetValueOrDefault(typeof(TE));
                if (handlersList != null)
                {
                    handlersList.Remove(handler);
                }
            }
        }
    }
}
