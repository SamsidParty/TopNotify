
static mut LoadedSettingsFile: Option<Settings> = None;

pub enum NotifyLocation
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Custom
}

pub struct Settings {
    pub Location: NotifyLocation,
    pub CustomPositionX: i32,
    pub CustomPositionY: i32
}

