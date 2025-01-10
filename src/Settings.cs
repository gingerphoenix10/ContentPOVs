using Zorro.Settings;

namespace ContentPOVs;

[ContentWarningSetting]
public class OnlyOwnerPickup : BoolSetting, IExposedSetting
{
    public override void ApplyValue()
    {
        POVPlugin.OwnerPickup = Value;
        POVPlugin.UpdateConfig();
    }

    public string GetDisplayName() => "[ContentPOVs] Only owner can pickup camera";

    protected override bool GetDefaultValue() => true;
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
}

[ContentWarningSetting]
public class OnlyOwnerPickupBroken : BoolSetting, IExposedSetting
{
    public override void ApplyValue()
    {
        POVPlugin.OwnerPickupBroken = Value;
        POVPlugin.UpdateConfig();
    }

    public string GetDisplayName() => "[ContentPOVs] Only owner can pickup broken camera";

    protected override bool GetDefaultValue() => false;
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
}

[ContentWarningSetting]
public class CameraColorable : BoolSetting, IExposedSetting
{
    public override void ApplyValue()
    {
        POVPlugin.Colorable = Value;
        POVPlugin.UpdateConfig();
    }

    public string GetDisplayName() => "[ContentPOVs] Match camera color to player's visor color";

    protected override bool GetDefaultValue() => true;
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
}

[ContentWarningSetting]
public class CameraNameable : BoolSetting, IExposedSetting
{
    public override void ApplyValue()
    {
        POVPlugin.Nameable = Value;
        POVPlugin.UpdateConfig();
    }

    public string GetDisplayName() => "[ContentPOVs] Show user's name while hovering over camera";

    protected override bool GetDefaultValue() => true;
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
}

[ContentWarningSetting]
public class CameraNameDisplay : BoolSetting, IExposedSetting
{
    public override void ApplyValue()
    {
        POVPlugin.NameDisplay = Value;
        POVPlugin.UpdateConfig();
    }

    public string GetDisplayName() => "[ContentPOVs] Display the camera's owner at the bottom right of recordings";

    protected override bool GetDefaultValue() => true;
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
}

[ContentWarningSetting]
public class DivideScore : BoolSetting, IExposedSetting
{
    public override void ApplyValue()
    {
        POVPlugin.ScoreDivision = Value;
        POVPlugin.UpdateConfig();
    }

    public string GetDisplayName() => "[ContentPOVs] Divide the score you get by the amount of players in the lobby to balance out gameplay";

    protected override bool GetDefaultValue() => true;
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
}

[ContentWarningSetting]
public class PickupDeadCameras : BoolSetting, IExposedSetting
{
    public override void ApplyValue()
    {
        POVPlugin.DeadCameras = Value;
        POVPlugin.UpdateConfig();
    }

    public string GetDisplayName() => "[ContentPOVs] Let anyone pickup the cameras of dead players (but not record)";

    protected override bool GetDefaultValue() => false;
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
}

[ContentWarningSetting]
public class RecordDeadCameras : BoolSetting, IExposedSetting
{
    public override void ApplyValue()
    {
        POVPlugin.DeadRecord = Value;
        POVPlugin.UpdateConfig();
    }

    public string GetDisplayName() =>
        "[ContentPOVs] Allow recording with dead players' cameras (Requires above setting)";

    protected override bool GetDefaultValue() => false;
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
}