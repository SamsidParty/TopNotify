import "../CSS/SplashScreen.css";

export default function SplashScreen(props) {
    return (
        <div className={"splashScreen " + (!!props.shown ? "showSplashScreen" : "")}>
            <div className="splashInnerBox">
                <img alt="TopNotify Icon" src="/Image/Icon.png"></img>
                <h1>TopNotify</h1>
            </div>
        </div>
    )
}