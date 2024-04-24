var Config = {
    Location: -1,
    RunOnStartup: true,
    Opacity: 0
}

window.SetConfig = (e) => {
    Config = JSON.parse(e);
    window.setRerender(Math.random());
}

const lightTheme = NextUI.createTheme({
    type: 'light'
})
  
const darkTheme = NextUI.createTheme({
    type: 'dark'
})

function UploadConfig() {

    if (Config.Location == -1) {
        //Config Hasn't Loaded Yet
        return;
    }

    var ev = new CustomEvent("uploadConfig");
    ev.newConfig = JSON.stringify(Config);
    document.body.dispatchEvent(ev);
    window.setRerender(Math.random());
}

function App() {
    
    var [rerender, setRerender] = React.useState(Math.random());
    window.setRerender = setRerender;

    return (
        <NextUI.NextUIProvider theme={(window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) ? darkTheme : lightTheme}>
            <h2>Settings</h2>

            <NextUI.Spacer></NextUI.Spacer>

            <div className="locationCard">
                <div className="notifyLocation tl"><Button onPress={() => ChangeLocation(0)} flat auto>{Config.Location == 0 ? "\uea5e" : "\ued27"}</Button></div>
                <div className="notifyLocation tr"><Button onPress={() => ChangeLocation(1)} flat auto>{Config.Location == 1 ? "\uea5e" : "\ued27"}</Button></div>
                <div className="notifyLocation bl"><Button onPress={() => ChangeLocation(2)} flat auto>{Config.Location == 2 ? "\uea5e" : "\ued27"}</Button></div>
                <div className="notifyLocation br"><Button onPress={() => ChangeLocation(3)} flat auto>{Config.Location == 3 ? "\uea5e" : "\ued27"}</Button></div>
            </div>

            <div className="divider"></div>

            <div className="flexx facenter fillx gap20 buttonContainer">
                <label>Spawn Test Notification</label>
                <Button css={{ marginLeft: "auto" }} className="iconButton" auto onPress={SpawnTestNotification}>
                    &#xea99;
                </Button>
            </div>

            <div className="divider"></div>

            <div className="flexx facenter fillx gap20">
                <label>Run On Startup</label>
                <Switch onChange={(e) => ChangeSwitch("RunOnStartup", e)} checked={Config.RunOnStartup} css={{ marginLeft: "auto" }}>
                    &#xea99;
                </Switch>
            </div>

            <div className="divider"></div>

            <div className="flexy fillx gap20">
                <label>Notification Opacity</label>
                <NextUI.Pagination onChange={ChangeOpacity} page={Math.round(6 - Config.Opacity)} rounded onlyDots total={6} />
            </div>

        </NextUI.NextUIProvider>
    )
}

function SpawnTestNotification() {
    document.body.dispatchEvent(new Event("spawnTestNotification"));
}

function ChangeLocation(location) {
    Config.Location = location;
    UploadConfig();
    window.setRerender(Math.random());
}

function ChangeOpacity(opacity) {
    Config.Opacity = (6 - opacity);
    UploadConfig();
    window.setRerender(Math.random());
}

function ChangeSwitch(key, e) {
    Config[key] = e.target.checked;
    UploadConfig();
    window.setRerender(Math.random());
}

addEventListener("spawnTestNotification", console.error);

var domNode = document.getElementById('root');
var root = ReactDOM.createRoot(domNode);
root.render(<App/>);
document.body.dispatchEvent(new Event("reactReady"));
setTimeout(() => document.body.dispatchEvent(new Event("reactReady")), 1000);