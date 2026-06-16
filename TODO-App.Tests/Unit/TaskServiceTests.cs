using Moq;
using TODO_App.Domain.Entities;
using TODO_App.Domain.Interfaces;
using TODO_App.Services.DTOs;
using TODO_App.Services.Services;

namespace TODO_App.Tests.Unit;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _sut = new TaskService(_taskRepository.Object, _categoryRepository.Object);
    }

    [Fact]
    public async Task CreateTaskAsync_WithForeignCategory_ThrowsArgumentException()
    {
        _categoryRepository
            .Setup(r => r.GetByIdAsync(1, 99))
            .ReturnsAsync((Category?)null);

        var dto = new CreateTaskDto { Title = "Test", CategoryId = 99 };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateTaskAsync(1, dto));
    }

    [Fact]
    public async Task CreateTaskAsync_WithValidCategory_CreatesTask()
    {
        _categoryRepository
            .Setup(r => r.GetByIdAsync(1, 2))
            .ReturnsAsync(new Category { Id = 2, Name = "Work", UserId = 1 });

        _taskRepository
            .Setup(r => r.AddTaskAsync(It.IsAny<ToDoTask>()))
            .Callback<ToDoTask>(t => t.Id = 10)
            .Returns(Task.CompletedTask);

        _taskRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _sut.CreateTaskAsync(1, new CreateTaskDto { Title = "New task", CategoryId = 2 });

        Assert.Equal(10, result.Id);
        Assert.Equal("New task", result.Title);
        Assert.Equal(2, result.CategoryId);
    }

    [Fact]
    public async Task GetTasksAsync_ReturnsPagedResult()
    {
        var tasks = new List<ToDoTask>
        {
            new() { Id = 1, Title = "A", UserId = 1, CreatedAt = DateTime.UtcNow }
        };

        _taskRepository
            .Setup(r => r.GetTasksAsync(1, 1, 10, null, null))
            .ReturnsAsync((tasks, 1));

        var result = await _sut.GetTasksAsync(1, 1, 10, null, null);

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
    }
}
