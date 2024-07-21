import { Button, Switch, Slider, SliderTrack, SliderFilledTrack, SliderThumb, Divider } from '@chakra-ui/react'

export default function NotificationTransparency() {
    return (
        <div className="flexy fillx gap20">
            <label>Notification Transparency</label>
            {
                //Slider Is In Uncontrolled Mode For Performance Reasons
                //So We Need To Wait For The Config To Load Before Setting The Default Value
                (rerender == 0) ?
                    (<></>) :
                    (
                        <Slider size="lg" onChangeEnd={ChangeTransparency} defaultValue={Config.Opacity * 20}>
                            <SliderTrack>
                                <SliderFilledTrack />
                            </SliderTrack>
                            <SliderThumb />
                        </Slider>
                    )
            }
        </div>
    )
}

function ChangeTransparency(opacity) {
    Config.Opacity = (opacity * 0.05);
    window.UploadConfig();
    window.setRerender(rerender + 1);
}