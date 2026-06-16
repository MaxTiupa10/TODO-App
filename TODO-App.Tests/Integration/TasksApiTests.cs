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

        var searched = await GetTasksAsync(search: "Important");
        Assert.Single(searched.Items);
        Assert.Contains("Important", searched.Items.First().Description);

        var firstTask = page1.Items.First();
        var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{firstTask.Id}", new UpdateTaskDto
        {
            Title = "Updated title",
            Description = firstTask.Description,
            IsCompleted = true,
            CategoryId = category.Id
        });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getUpdated = await _client.GetFromJsonAsync<TaskDto>($"/api/tasks/{firstTask.Id}", _jsonOptions);
        Assert.NotNull(getUpdated);
        Assert.True(getUpdated.IsCompleted);
        Assert.Equal("Updated title", getUpdated.Title);

        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{firstTask.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await GetTasksAsync();
        Assert.Equal(11, afterDelete.TotalCount);
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
        string? search = null)
    {
        var query = $"/api/tasks?pageNumber={pageNumber}&pageSize={pageSize}";
        if (categoryId.HasValue)
            query += $"&categoryId={categoryId.Value}";
        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";

        var result = await _client.GetFromJsonAsync<PagedResult<TaskDto>>(query, _jsonOptions);
        Assert.NotNull(result);
        return result;
    }
}
