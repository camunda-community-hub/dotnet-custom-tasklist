using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DemoDotNet.Models;
using Google.Apis.Auth.OAuth2;
using Zeebe.Client.Impl.Builder;
using Zeebe.Client.Impl.Responses;
using Zeebe.Client;
using static DemoDotNet.Models.Models;
using System.Collections;
using DemoDotNet.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using GraphQL;

namespace DemoDotNet.Controllers
{  

    public class TaskController : Controller
    {       
        private readonly ILogger<TaskController> _logger;
        private readonly TaskListService _taskListService;

        public TaskController(ILogger<TaskController> logger, TaskListService taskListService)
        {
            _logger = logger;
            _taskListService = taskListService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetAllTasks()
        {
            var alltasks = await _taskListService.FetchAllTasks();

            return new JsonResult(new { tasks = alltasks });
        }

        [HttpGet]
        public async Task<JsonResult> GetOpenTasks()
        {
            var alltasks = await _taskListService.FetchAllTasks();            

            return new JsonResult(new { tasks = alltasks.Where(i=>i.assignee is null).ToList() });
        }

        [HttpGet]
        public async Task<JsonResult> GetTask(string taskId)
        {            
            return new JsonResult(new { task = await _taskListService.GetTask(taskId) });       
        }

        [HttpGet]
        public async Task<JsonResult> GetTaskByProcessInstanceId([FromQuery] string processInstanceId)
        {
            var task = await _taskListService.GetTaskByProcessInstanceId(processInstanceId);

            return new JsonResult(new {task });

        }

        [HttpGet]
        public async Task<JsonResult> GetTasksByUser([FromQuery] string user)
        {
            var tasks = await _taskListService.GetTasksByUser(user);
            return new JsonResult(new { tasks });
        }

        [HttpGet]
        public async Task<JsonResult> GetCompletedTasks([FromQuery] string user)
        {
            var tasks = await _taskListService.GetCompletedTasks(user);
            return new JsonResult(new { tasks });
        }

        [HttpPost]
        public  async Task<JsonResult> ClaimTask([FromQuery]string taskId, [FromQuery] string user)
        {
            var task = await _taskListService.ClaimTask(taskId, user);
            return new JsonResult(new { claimedTask = task });
        }


        [HttpPost]
        public async Task<JsonResult> UnClaimTask([FromQuery] string taskId)
        {
            var task = await _taskListService.UnClaimTask(taskId);
            return new JsonResult(new { claimedTask = task});
        }

        [HttpPost]
        public async Task<JsonResult> CompleteTask([FromQuery] string taskId)
        {
            var task = await _taskListService.CompleteTask(taskId);
            return new JsonResult(new { claimedTask = task });
        }

        
                
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }        
    }
}

