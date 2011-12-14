using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ginnaydd.Utilities;

namespace Ginnaydd.Distributed
{
	public abstract class AbstractTaskPool
	{
		protected ProducerConsumerQueue<Task> taskQueue = new ProducerConsumerQueue<Task>();

		public ProducerConsumerQueue<Task> TaskQueue
		{
			get { return taskQueue; }
			set { taskQueue = value; }
		}

		public abstract Task ConsumeTask();
		public abstract void ProduceTask(Task task);
		public abstract void ProduceTasks(List<Task> tasks);
		public abstract void FailTask(Task task);
		public abstract void FinishTask(Task task);
	}
	public enum TaskState
	{
		NOT_FINISHED = 0,
		FINISHED = 1,
		FAILED = 2,
		IN_MEMORY = 3
	}
}
