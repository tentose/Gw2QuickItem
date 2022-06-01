using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    /// <summary>
    /// Aggregates requests to load items
    /// </summary>
    internal class ItemRequestAggregator
    {
        private const int AGGREGATION_INTERVAL_MILLIS = 200;
        private static ConcurrentQueue<int> IdsToLoad;

        private static async Task LoadItem(int id)
        {

        }
    }
}
