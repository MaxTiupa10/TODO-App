// Файл: Controllers/TasksController.cs
using Microsoft.AspNetCore.Mvc;
using TODO_App.DataAccess.Interfaces;
using TODO_App.Services.DTOs;


namespace TODO_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    
    // Тимчасово хардкодимо ID користувача до реалізації логіну (JWT)
    private readonly int _currentUserId = 1;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null)
    {
        var tasks = await _taskService.GetTasksAsync(_currentUserId, pageNumber, pageSize, categoryId, search);
        return Ok(tasks); // 200 OK
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetTaskById(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(_currentUserId, id);
        if (task == null) return NotFound(); // 404 Not Found

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto dto)
    {
        var createdTask = await _taskService.CreateTaskAsync(_currentUserId, dto);
        
        // Повертає 201 Created та посилання на метод GetTaskById
        return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
    {
        var success = await _taskService.UpdateTaskAsync(_currentUserId, id, dto);
        if (!success) return NotFound();

        return NoContent(); // 204 No Content (успішно оновлено, повертати тіло не треба)
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        var success = await _taskService.DeleteTaskAsync(_currentUserId, id);
        if (!success) return NotFound();

        return NoContent();
    }
}