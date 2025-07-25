import { Button, Switch, Divider, Select } from "@chakra-ui/react";

export default function MonitorSelect(props) {
    return (
        <Select className='monitorSelect' value={Config?.PreferredMonitor} onChange={(e) => { ChangeValue("PreferredMonitor", e.target.value); igniteView.commandBridge.RequestConfig(); } }>
            <option value="primary">Primary Monitor</option>
            {
                Config?.__MonitorData?.map((monitor) => {
                    return (<option key={monitor.ID} value={monitor.ID}>{monitor.DisplayName}</option>);
                })
            }
        </Select>
    );
}