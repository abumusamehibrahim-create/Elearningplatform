using ELearningPlatform.Models;

public class MenuItemViewModel
{
    public string Name { get; set; }
    public string Url { get; set; }



    public MenuItem MenuItem { get; set; }

    public List<(string Name, string Url)> Pages { get; set; }

    public List<MenuItem> MenuItems { get; set; }
}