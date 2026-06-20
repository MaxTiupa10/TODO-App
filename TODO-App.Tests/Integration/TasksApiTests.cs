using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TODO_App.Services.DTOs;

namespace TODO_App.Tests.Integration;

public class TasksApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TasksApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTasks_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TasksCrud_SearchFilterAndPagination_WorkEndToEnd()
    {
        var token = await RegisterAndLoginAsync("tasks_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", new CreateCategoryDto { Name = "Work" });
        categoryResponse.EnsureSuccessStatusCode();
        var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryDto>(_jsonOptions);
        Assert.NotNull(category);

        for (var i = 1; i <= 12; i++)
        {
            var createResponse = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskDto
            {
                Title = i <= 6 ? $"Work task {i}" : $"Personal task {i}",
                Description = i == 1 ? "Important description" : null,
                CategoryId = i <= 6 ? category.Id : null
            });
            createResponse.EnsureSuccessStatusCode();
        }

        var page1 = await GetTasksAsync(pageNumber: 1, pageSize: 5);
        Assert.Equal(5, page1.Items.Count());
        Assert.Equal(12, page1.TotalCount);
        Assert.Equal(3, page1.TotalPages);

        var filtered = await GetTasksAsync(categoryId: category.Id);
        Assert.Equal(6, filtered.TotalCount);
        Assert.All(filtered.Items, t => Assert.Equal(category.Id, t.CategoryId));

        var searched = await GetTasksAsync(search: "important");
        Assert.Single(searched.Items);
        Assert.Contains("Important", searched.Items.First().Description, StringComparison.OrdinalIgnoreCase);

        var firstTask = page1.Items.First();
        var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{firstTask.Id}", new UpdateTaskDto
        {
            Title = "Updated title",
            Description = firstTask.Description,
            IsCompleted = true,
            IsImportant = firstTask.IsImportant,
            Deadline = firstTask.Deadline,
            CategoryId = category.Id
        });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getUpdated = await _client.GetFromJsonAsync<TaskDto>($"/api/tasks/{firstTask.Id}", _jsonOptions);
        Assert.NotNull(getUpdated);
        Assert.True(getUpdated.IsCompleted);
        Assert.NotNull(getUpdated.CompletedAt);
        Assert.Equal("Updated title", getUpdated.Title);

        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{firstTask.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await GetTasksAsync();
        Assert.Equal(11, afterDelete.TotalCount);
    }

    [Fact]
    public async Task GetTasks_ListTypeFilters_ReturnExpectedTasks()
    {
        var token = await RegisterAndLoginAsync("list_type_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var importantResponse = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskDto
        {
            Title = "Important task",
            IsImportant = true,
            Deadline = DateTime.UtcNow.AddDays(1)
        });
        importantResponse.EnsureSuccessStatusCode();

        var plannedResponse = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskDto
        {
            Title = "Planned task",
            Deadline = DateTime.UtcNow.AddDays(2)
        });
        plannedResponse.EnsureSuccessStatusCode();

        var completedResponse = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskDto
        {
            Title = "Completed task"
        });
        completedResponse.EnsureSuccessStatusCode();
        var completedTask = await completedResponse.Content.ReadFromJsonAsync<TaskDto>(_jsonOptions);
        Assert.NotNull(completedTask);

        var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{completedTask.Id}", new UpdateTaskDto
        {
            Title = completedTask.Title,
            Description = completedTask.Description,
            IsCompleted = true,
            IsImportant = completedTask.IsImportant,
            Deadline = completedTask.Deadline,
            CategoryId = completedTask.CategoryId
        });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var important = await GetTasksAsync(listType: "important");
        Assert.Single(important.Items);
        Assert.Equal("Important task", important.Items.First().Title);

        var planned = await GetTasksAsync(listType: "planned");
        Assert.Equal(2, planned.TotalCount);

        var completed = await GetTasksAsync(listType: "completed");
        Assert.Single(completed.Items);
        Assert.Equal("Completed task", completed.Items.First().Title);

        var assignedToMe = await GetTasksAsync(listType: "assignedtome");
        Assert.Equal(2, assignedToMe.TotalCount);

        var allTasks = await GetTasksAsync(listType: "tasks");
        Assert.Equal(3, allTasks.TotalCount);
    }

    [Fact]
    public async Task GetTasks_DateRangeFilter_ReturnsTasksWithinDeadlineRange()
    {
        var token = await RegisterAndLoginAsync("date_range_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var earlyResponse = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskDto
        {
            Title = "Early task",
            Deadline = new DateTime(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc)
        });
        earlyResponse.EnsureSuccessStatusCode();

        var middleResponse = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskDto
        {
            Title = "Middle task",
            Deadline = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc)
        });
        middleResponse.EnsureSuccessStatusCode();

        var lateResponse = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskDto
        {
            Title = "Late task",
            Deadline = new DateTime(2026, 6, 20, 12, 0, 0, DateTimeKind.Utc)
        });
        lateResponse.EnsureSuccessStatusCode();

        var ranged = await GetTasksAsync(
            listType: "tasks",
            deadlineFromUtc: new DateTime(2026, 6, 12, 0, 0, 0, DateTimeKind.Utc),
            deadlineToUtc: new DateTime(2026, 6, 18, 23, 59, 59, 999, DateTimeKind.Utc));

        Assert.Equal(1, ranged.TotalCount);
        Assert.Single(ranged.Items, t => t.Title == "Middle task");
        Assert.DoesNotContain(ranged.Items, t => t.Title == "Early task");
        Assert.DoesNotContain(ranged.Items, t => t.Title == "Late task");
    }

    [Fact]
    public async Task GetTasks_DeadlineToUtc_ExcludesTasksAfterSelectedLocalDayEnd()
    {
        var token = await RegisterAndLoginAsync("deadline_tz_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await CreateTaskAsync(
            "June 21 local midnight",
            new DateTime(2026, 6, 20, 21, 0, 0, DateTimeKind.Utc));
        await CreateTaskAsync(
            "June 20 task",
            new DateTime(2026, 6, 20, 12, 0, 0, DateTimeKind.Utc));

        var deadlineToUtc = new DateTime(2026, 6, 20, 20, 59, 59, 999, DateTimeKind.Utc);

        var result = await GetTasksAsync(listType: "tasks", deadlineToUtc: deadlineToUtc);

        Assert.Single(result.Items);
        Assert.Equal("June 20 task", result.Items.First().Title);
    }

    [Fact]
    public async Task GetTasks_MyDay_ReturnsOverdueTodayAndCreatedTodayWithoutDeadline()
    {
        var token = await RegisterAndLoginAsync("myday_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var today = DateTime.UtcNow.Date;

        await CreateTaskAsync("Overdue task", today.AddDays(-2));
        await CreateTaskAsync("Today task", today.AddHours(15));
        await CreateTaskAsync("Created today without deadline");
        await CreateTaskAsync("Future task", today.AddDays(3));
        await CreateTaskAsync("Completed overdue", today.AddDays(-1), isCompleted: true);

        var myDay = await GetTasksAsync(listType: "myday", pageSize: 20);

        Assert.Equal(3, myDay.TotalCount);
        Assert.Contains(myDay.Items, t => t.Title == "Overdue task");
        Assert.Contains(myDay.Items, t => t.Title == "Today task");
        Assert.Contains(myDay.Items, t => t.Title == "Created today without deadline");
        Assert.DoesNotContain(myDay.Items, t => t.Title == "Future task");
        Assert.DoesNotContain(myDay.Items, t => t.Title == "Completed overdue");
    }

    [Fact]
    public async Task GetTasks_Planned_ReturnsIncompleteTasksWithDeadlineFromTodaySortedAscending()
    {
        var token = await RegisterAndLoginAsync("planned_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var today = DateTime.UtcNow.Date;

        await CreateTaskAsync("Overdue planned", today.AddDays(-2));
        await CreateTaskAsync("Later planned", today.AddDays(5));
        await CreateTaskAsync("Soon planned", today.AddDays(1));
        await CreateTaskAsync("Today planned", today.AddHours(18));
        await CreateTaskAsync("No deadline planned");
        await CreateTaskAsync("Done planned", today.AddDays(2), isCompleted: true);

        var planned = await GetTasksAsync(listType: "planned", pageSize: 20);

        Assert.Equal(3, planned.TotalCount);
        Assert.Equal(
            ["Today planned", "Soon planned", "Later planned"],
            planned.Items.Select(t => t.Title).ToArray());
        Assert.DoesNotContain(planned.Items, t => t.Title == "Overdue planned");
        Assert.DoesNotContain(planned.Items, t => t.Title == "No deadline planned");
        Assert.DoesNotContain(planned.Items, t => t.Title == "Done planned");
    }

    private async Task CreateTaskAsync(string title, DateTime? deadline = null, bool isCompleted = false)
    {
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskDto
        {
            Title = title,
            Deadline = deadline
        });
        createResponse.EnsureSuccessStatusCode();

        if (!isCompleted)
            return;

        var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>(_jsonOptions);
        Assert.NotNull(created);

        var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{created.Id}", new UpdateTaskDto
        {
            Title = created.Title,
            Description = created.Description,
            IsCompleted = true,
            IsImportant = created.IsImportant,
            Deadline = created.Deadline,
            CategoryId = created.CategoryId
        });
        updateResponse.EnsureSuccessStatusCode();
    }

    private async Task<string> RegisterAndLoginAsync(string username)
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Username = username,
            Password = "Password123!"
        });
        registerResponse.EnsureSuccessStatusCode();
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        Assert.NotNull(auth);
        return auth.Token;
    }

    private async Task<PagedResult<TaskDto>> GetTasksAsync(
        int pageNumber = 1,
        int pageSize = 10,
        int? categoryId = null,
        string? search = null,
        string? listType = null,
        DateTime? deadlineFromUtc = null,
        DateTime? deadlineToUtc = null)
    {
        var query = $"/api/tasks?pageNumber={pageNumber}&pageSize={pageSize}";
        if (categoryId.HasValue)
            query += $"&categoryId={categoryId.Value}";
        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(listType))
            query += $"&listType={Uri.EscapeDataString(listType)}";
        if (deadlineFromUtc.HasValue)
            query += $"&deadlineFromUtc={Uri.EscapeDataString(deadlineFromUtc.Value.ToString("O"))}";
        if (deadlineToUtc.HasValue)
            query += $"&deadlineToUtc={Uri.EscapeDataString(deadlineToUtc.Value.ToString("O"))}";

        var result = await _client.GetFromJsonAsync<PagedResult<TaskDto>>(query, _jsonOptions);
        Assert.NotNull(result);
        return result;
    }
}
