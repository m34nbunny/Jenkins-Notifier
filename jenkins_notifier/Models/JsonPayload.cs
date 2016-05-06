using System;

namespace jenkins_notifier.Models
{
	public class JsonPayload
	{
		public JsonPayload ()
		{
		}

		public object Payload{ get; set;}
		public bool Errored{ get; set;}
		public string Exception{get;set;}
	}


	public class JsonPayload<T>
	{
		public JsonPayload ()
		{
		}

		public T Payload{ get; set;}
		public bool Errored{ get; set;}
		public string Exception{get;set;}
	}

}

