
use serde::{Deserialize, Serialize};
use serde_repr::{Serialize_repr, Deserialize_repr};

static mut LoadedSettingsFile: Option<Settings> = None;

#[derive(Serialize_repr, Deserialize_repr, PartialEq, Debug)]
#[repr(u8)]
pub enum NotifyLocation
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Custom
}

#[derive(Deserialize, Debug)]
pub struct Settings {
    pub Location: NotifyLocation,
    pub CustomPositionX: i32,
    pub CustomPositionY: i32,
    pub SoundPath: String
}

