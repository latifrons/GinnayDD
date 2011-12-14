using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ginnaydd.Utilities
{
	public class ProducerConsumerQueue<T>
	{
		object locker = new object();
		Queue<T> taskQ = new Queue<T>();

		public void Produce(T task)
		{
			lock (locker)
			{
				taskQ.Enqueue(task);
				//Monitor.PulseAll(locker);
				Monitor.Pulse(locker);
//				Console.WriteLine("Pulse {0}", Thread.CurrentThread.Name);
			}
		}
		public void Produce(IEnumerable<T> tasks)
		{
			lock (locker)
			{
				foreach (T task in tasks)
				{
					taskQ.Enqueue(task);	
				}
				
				Monitor.PulseAll(locker);
				//Monitor.Pulse(locker);
				//				Console.WriteLine("Pulse {0}", Thread.CurrentThread.Name);
			}
		}
		public bool TryConsume(out T value)
		{
			lock (locker)
			{
				if (taskQ.Count == 0)
				{
					value = default(T);
					return false;
				}
				value = taskQ.Dequeue();
				return true;
			}
		}

		public T Consume()
		{
			T task;
			lock (locker)
			{
				while (taskQ.Count == 0)
				{
					Monitor.Wait(locker);
//					Console.WriteLine("Wait over {0}", Thread.CurrentThread.Name);
				}
				task = taskQ.Dequeue();
			}
			return task;
		}
		public int Count
		{
			get { return taskQ.Count; }
		}
		public void Clear()
		{
			lock (locker)
			{
				taskQ.Clear();
			}
		}
	}
}
