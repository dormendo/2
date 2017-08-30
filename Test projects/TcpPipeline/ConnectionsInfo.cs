using System;

namespace Maxima.Tcp
{
    public struct ConnectionsInfo
    {
        public int TotalConnectionsCount { get; private set; }
        public DateTime? LastConnectionTime { get; private set; }

        public ConnectionsInfo(int totalConnectionsCount, long lastConnectionTimeTicks)
            : this()
        {
            TotalConnectionsCount = totalConnectionsCount;
            LastConnectionTime = (lastConnectionTimeTicks == 0
                ? null
                : (DateTime?) new DateTime(lastConnectionTimeTicks));
        }
    }
}