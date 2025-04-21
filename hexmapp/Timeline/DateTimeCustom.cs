using Godot;

public class DateTimeCustom
{
    private Calendar calendar;
    public int currentYear { get; set; }
    public int currentMonthIndex { get; set; }
    public int currentDay { get; set; }
    public int currentTime_minutes { get; set; }

    public DateTimeCustom(Calendar calendar)
    {
        this.calendar = calendar;
    }

    public Godot.Collections.Dictionary<string, Variant> Save()
    {
        return new Godot.Collections.Dictionary<string, Variant>();
        // save calendar & current datetime
    }

    public void Load(Godot.Collections.Dictionary<string, Variant> data)
    {

    }
}