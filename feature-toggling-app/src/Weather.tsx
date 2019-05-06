import * as React from 'react'
import axios from 'axios';
import { BACKEND_URI } from './config'

interface IProps {
    defaultLocation: string;
}

interface IState {
    choosenLocation: string;
    inputText: string;
    temperature?: string;
    cloudCover?: number;
    weatherSource?: string;
}

class Weather extends React.Component<IProps, IState> {
    static defaultLocation: string = "Lyon, France";

    constructor(props: IProps, state: IState) {
        super(props, state)
        console.log(`backend: ${BACKEND_URI}`)
    }
    public static defaultProps: Partial<IProps> = {
        defaultLocation: Weather.defaultLocation
    };

    public state: IState = {
        choosenLocation: Weather.defaultLocation,
        inputText: ''
    };

    public get = () => {
        axios.get(`${BACKEND_URI}/api/weather?location=${this.state.inputText}`)
            .then(response => {
                this.setState({
                    choosenLocation: `${this.state.inputText}`,
                    cloudCover: response.data.cloudCover,
                    temperature: response.data.temperature,
                    weatherSource: response.data.source
                })
            })
    }

    public render() {
        return (
            <div>
                <p>The weather data source is {this.state.weatherSource}</p>
                <p>
                    Temperature: {this.state.temperature} <br />
                    Cloud Cover: {this.state.cloudCover}
                </p>
                <p>
                    <input type="text" value={this.state.inputText} onChange={this.onTextChanged} />
                    <button onClick={this.get}>Change location</button>
                </p>
            </div>
        )
    }

    private onTextChanged = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({ inputText: e.target.value });
    }
}

export default Weather;