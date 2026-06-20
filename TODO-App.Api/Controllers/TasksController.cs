using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TODO_App.Extensions;
using TODO_App.Services.DTOs;
using TODO_App.Services.Helpers;
using TODO_App.Services.Interfaces;

namespace TODO_App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetTasks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? listType = null,
        [FromQuery] string? dateFrom = null,
        [FromQuery] string? dateTo = null)
    {
        if (!TaskQueryDateParser.TryParse(dateFrom, out var parsedDateFrom, out var dateFromError))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = dateFromError
            });
        }

        if (!TaskQueryDateParser.TryParse(dateTo, out var parsedDateTo, out var dateToError))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = dateToError
            });
        }

        var (normalizedFrom, normalizedTo) = TaskQueryDateParser.NormalizeRange(parsedDateFrom, parsedDateTo);

        var result = await _taskService.GetTasksAsync(
            User.GetUserId(),
            pageNumber,
            pageSize,
            categoryId,
            search,
            listType,
            normalizedFrom,
            normalizedTo);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetTaskById(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(User.GetUserId(), id);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto dto)
    {
        try
        {
            var createdTask = await _taskService.CreateTaskAsync(User.GetUserId(), dto);
            return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = ex.Message
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
    {
        try
        {
            var success = await _taskService.UpdateTaskAsync(User.GetUserId(), id, dto);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = ex.Message
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var success = await _taskService.DeleteTaskAsync(User.GetUserId(), id);
        if (!success) return NotFound();
        return NoContent();
    }
}
