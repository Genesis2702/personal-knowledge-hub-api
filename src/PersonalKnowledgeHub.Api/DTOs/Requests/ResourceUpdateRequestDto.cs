

using System.Text.Json.Serialization;

namespace PersonalKnowledgeHub.DTOs.Requests;

public class ResourceUpdateRequestDto
{
    private string? _title;
    private string? _url;
    private string? _description;

    [JsonIgnore] public bool TitleSpecified { get; private set; }
    [JsonIgnore] public bool UrlSpecified { get; private set; }
    [JsonIgnore] public bool DescriptionSpecified { get; private set; }

    public string? Title
    {
        get => _title;
        set
        {
            _title = value;
            TitleSpecified = true;
        }
    }

    public string? Url
    {
        get => _url;
        set
        {
            _url = value;
            UrlSpecified = true;
        }
    }

    public string? Description
    {
        get => _description;
        set
        {
            _description = value;
            DescriptionSpecified = true;
        }
    }
}