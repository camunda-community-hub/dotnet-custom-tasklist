using System;
namespace DemoDotNet.Models
{
	public class TaskListAuthDto
	{
		public TaskListAuthDto()
		{
		}

		public string scope { get; set; }
		public int expires_in { get; set; }
		public string access_token { get; set; }
	}
}

