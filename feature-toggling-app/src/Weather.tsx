import * as React from 'react'

interface IProps {
    defaultLocation: string;
}

interface IState {
    choosenLocation: string;
    inputText: string;
}

class Weather extends React.Component<IProps, IState> {
    static defaultLocation: string = "Lyon, France";

    constructor(props: IProps, state: IState) {
        super(props, state)
    }
    public static defaultProps: Partial<IProps> = {
        defaultLocation: Weather.defaultLocation
    };

    public state: IState = {
        choosenLocation: Weather.defaultLocation,
        inputText: ''
    };

    public get = () => {
        this.setState({ choosenLocation: `${this.state.inputText}` })
    }

    public render() {
        return (
            <div>
                <p>We find the following location: {this.state.choosenLocation}</p>
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