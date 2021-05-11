using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TennisEmulator
{
    public interface IEventDispatcher
    {
        public void Dispatch<TS, TE>(TS source, TE even);
        public void AddHandler<TS, TE>(TS source, IEventHandler<TS, TE> handler);
        public void RemoveHandler<TS, TE>(TS source, IEventHandler<TS, TE> handler);
        public void CleanupSource<TS>(TS Source);
    }

    public interface IEventHandler<TS,TE>
    {
        public void Handle(TS source, TE e);
    }
}
