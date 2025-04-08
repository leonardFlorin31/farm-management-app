using System.Net.Http;
using System.Net.Http.Json;

namespace licenta.ViewModel;

public class PolygonService
{
    private readonly HttpClient _httpClient;

    // BaseAddress should point to your API (update as needed)
    public PolygonService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PolygonResponseDto> CreatePolygonAsync(CreatePolygonRequest request)
    {
        // POST to the API endpoint
        var response = await _httpClient.PostAsJsonAsync("api/Polygons", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PolygonResponseDto>();
    }
    
}

public class CreatePolygonRequest
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public List<PointRequest> Points { get; set; }
}

public class PointRequest
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    
    public int Order { get; set; }
}

public class PolygonResponseDto
{
    public Guid PolygonId { get; set; }
    public string PolygonName { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<PolygonPointDto> Points { get; set; }
}

public class PolygonPointDto
{
    public Guid PointId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int Order { get; set; }
}
