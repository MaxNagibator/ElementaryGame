namespace Elementary.Business;

public class Question
{
    public string Value { get; set; }
    public string Type { get; set; }
    public List<string> Options { get; set; }
    public List<string>? TargetOptions { get; set; }
    public string Answer { get; set; }
}
