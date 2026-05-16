using ELearningPlatform.Models;

public class ClientSettingsDashboardViewModel
{
    public ClientSetting Settings { get; set; }
    public List<License> Licenses { get; set; }
    public List<string> Logos { get; set; }   // ← هذا هو الاسم الصحيح
}
