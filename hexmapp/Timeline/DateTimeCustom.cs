using Godot;

public class DateTimeCustom
{
    private Calendar calendar;
    public int currentYear { get; set; } = 1;
    public int currentMonthIndex { get; set; } = 0;
    public int currentDay { get; set; } = 1;
    public int currentTime_minutes { get; set; } = 0;

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