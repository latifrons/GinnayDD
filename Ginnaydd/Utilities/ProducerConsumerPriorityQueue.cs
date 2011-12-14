using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PriorityQueueDemo;

namespace Ginnaydd.Utilities
{
	public class ProducerConsumerPriorityQueue<TPriority,TValue>
	{
		object locker = new object();
		PriorityQueue<TPriority, TValue> queue = new PriorityQueue<TPriority, TValue>();

		public void Produce(TPriority p, TValue v)
		{
			lock (locker)
			{
				queue.Enqueue(p,v);
				//Monitor.PulseAll(locker);
				Monitor.Pulse(locker);
//				Console.WriteLine("Pulse {0}", Thread.CurrentThread.Name);
			}
		}
		public KeyValuePair<TPriority, TValue> Consume()
		{
			KeyValuePair<TPriority, TValue> kv;
			lock (locker)
			{
				while (queue.Count == 0)
				{
					Monitor.Wait(locker);
//					Console.WriteLine("Wait over {0}", Thread.CurrentThread.Name);
				}
				kv = queue.Dequeue();
			}
			return kv;
		}
		public bool TryConsumeValue(out TValue value)
		{
			lock (locker)
			{
				if (queue.Count == 0)
				{
					value = default(TValue);
					return false;
				}
				else
				{
					value = queue.DequeueValue();
					return true;
				}
			}
		}
		public bool TryConsume(out KeyValuePair<TPriority, TValue> value)
		{
			lock (locker)
			{
				if (queue.Count == 0)
				{
					value = default(KeyValuePair<TPriority, TValue>);
					return false;
				}
				else
				{
					value = queue.Dequeue();
					return true;
				}
			}
		}


		public TValue ConsumeValue()
		{
			TValue task;
			lock (locker)
			{
				while (queue.Count == 0)
				{
					Monitor.Wait(locker);
//					Console.WriteLine("Wait over {0}", Thread.CurrentThread.Name);
				}
				task = queue.DequeueValue();
			}
			return task;
		}
		public int Count
		{
			get { return queue.Count; }
		}
		public void Clear()
		{
			lock (locker)
			{
				queue.Clear();
			}
		}
	}
}
